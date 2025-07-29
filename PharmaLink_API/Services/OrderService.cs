using AutoMapper;
using Microsoft.Extensions.Options;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Core.Results;
using PharmaLink_API.Models;
using PharmaLink_API.Models.DTO.CartDTO;
using PharmaLink_API.Models.DTO.OrderDTO;
using PharmaLink_API.Repository;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;

namespace PharmaLink_API.Services
{
    public class OrderService : IOrderService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IPharmacyStockRepository _pharmacyStockRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IMapper _mapper;
        private readonly IStripeService _stripeService;

        public OrderService(
            ICartRepository cartRepository,
            IPatientRepository patientRepository,
            IOrderHeaderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository,
            IPharmacyStockRepository pharmacyStockRepository,
            IPharmacyRepository pharmacyRepository,
            IMapper mapper,
            IStripeService stripeService)
        {
            _cartRepository = cartRepository;
            _patientRepository = patientRepository;
            _orderHeaderRepository = orderHeaderRepository;
            _orderDetailRepository = orderDetailRepository;
            _pharmacyStockRepository = pharmacyStockRepository;
            _pharmacyRepository = pharmacyRepository;
            _mapper = mapper;
            _stripeService = stripeService;
        }

        /// <summary>
        /// Submits a new order for the specified account using the provided order details.
        /// Validates cart items, creates order and order details, updates stock, and clears cart.
        /// </summary>
        public async Task<ServiceResult<OrderResponseDTO>> SubmitOrderAsync(string accountId, SubmitOrderRequestDTO dto)
        {
            var patientResult = await GetPatientWithCartAsync(accountId);
            if (!patientResult.Success)
                return ServiceResult<OrderResponseDTO>.ErrorResult(patientResult.ErrorMessage, patientResult.ErrorType ?? ErrorType.Internal);

            var patient = patientResult.Data;
            var cartItems = patient.CartItems.ToList();
            int pharmacyId = cartItems.First().PharmacyId;

            var validationResult = await ValidateCartItemsAsync(cartItems);
            if (!validationResult.Success)
                return ServiceResult<OrderResponseDTO>.ErrorResult(validationResult.ErrorMessage, validationResult.ErrorType ?? ErrorType.Internal);

            decimal totalPrice = validationResult.Data;

            var order = await CreateOrderAsync(patient, dto.PaymentMethod, totalPrice, pharmacyId);
            await CreateOrderDetailsAsync(order.OrderID, cartItems);
            await UpdateStockAsync(cartItems);
            await ClearCartAsync(patient.CartItems.ToList());

            return ServiceResult<OrderResponseDTO>.SuccessResult(new OrderResponseDTO
            {
                OrderId = order.OrderID,
                PaymentMethod = dto.PaymentMethod,
                Message = "Order submitted successfully from cart."
            });
        }

        /// <summary>
        /// Cancels an existing order for the specified account.
        /// Handles refund if applicable and restocks drugs.
        /// </summary>
        public async Task<ServiceResult<string>> CancelOrderAsync(string accountId, int orderId)
        {
            var order = await _orderHeaderRepository.GetAsync(
                o => o.OrderID == orderId, tracking: true, x => x.OrderDetails
            );

            if (order == null)
                return ServiceResult<string>.ErrorResult("Order not found.", ErrorType.NotFound);

            var patient = await _patientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null || patient.PatientId != order.PatientId)
                return ServiceResult<string>.ErrorResult("You are not authorized to cancel this order.", ErrorType.Authorization);

            if (order.Status == SD.StatusCancelled)
                return ServiceResult<string>.ErrorResult("Order is already cancelled.", ErrorType.Conflict);

            if (order.Status == SD.StatusApproved)
            {
                if (order.PaymentMethod != "Cash" && order.PaymentStatus == SD.PaymentStatusApproved)
                {
                    var refundResult = await _stripeService.RefundStripePaymentAsync(order.PaymentIntentId);
                    if (!refundResult.Success)
                        return refundResult;
                }
                else
                {
                    return ServiceResult<string>.ErrorResult("Cannot cancel an already approved order paid in cash.", ErrorType.BusinessRule);
                }
            }
            else
            {
                order.PaymentStatus = SD.PaymentStatusRefunded;
            }

            order.Status = SD.StatusCancelled;

            await RestockDrugsAsync(order);
            await _orderHeaderRepository.SaveAsync();
            await _pharmacyStockRepository.SaveAsync();

            return ServiceResult<string>.SuccessResult($"Order #{orderId} has been cancelled and refund issued if applicable.");
        }

        /// <summary>
        /// Accepts an order for the specified account and order ID.
        /// Updates order status to approved.
        /// </summary>
        public async Task<ServiceResult> AcceptOrderAsync(int orderId, string accountId)
        {
            var order = await _orderHeaderRepository.GetAsync(o => o.OrderID == orderId, true, x => x.OrderDetails);
            if (order == null)
                return ServiceResult.ErrorResult("Order not found", ErrorType.NotFound);

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return ServiceResult.ErrorResult("Pharmacy not found", ErrorType.NotFound);

            if (order.PharmacyId != pharmacy.PharmacyID)
                return ServiceResult.ErrorResult("You are not authorized to update this order.", ErrorType.Authorization);

            if (order.Status != SD.StatusPending)
                return ServiceResult.ErrorResult("Only pending orders can be accepted.", ErrorType.Validation);

            order.Status = SD.StatusApproved;
            await _orderHeaderRepository.SaveAsync();

            return ServiceResult.SuccessResult();
        }

        /// <summary>
        /// Rejects an order for the specified account and order ID.
        /// Handles refund if payment was approved and method was not cash, and restocks drugs.
        /// </summary>
        public async Task<ServiceResult> RejectOrderAsync(int orderId, string accountId)
        {
            var order = await _orderHeaderRepository.GetAsync(o => o.OrderID == orderId, true, x => x.OrderDetails);
            if (order == null)
                return ServiceResult.ErrorResult("Order not found", ErrorType.NotFound);

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return ServiceResult.ErrorResult("Pharmacy not found", ErrorType.NotFound);

            if (order.PharmacyId != pharmacy.PharmacyID)
                return ServiceResult.ErrorResult("You are not authorized to update this order.", ErrorType.Authorization);

            if (order.Status != SD.StatusPending)
                return ServiceResult.ErrorResult("Only pending orders can be rejected.", ErrorType.Validation);

            order.Status = SD.StatusRejected;

            //Handle refund if payment was approved and method was not Cash
            if (order.PaymentStatus == SD.PaymentStatusApproved && order.PaymentMethod != "Cash")
            {
                var refundResult = await _stripeService.RefundStripePaymentAsync(order.PaymentIntentId);
                if (!refundResult.Success)
                    return refundResult;
            }

            // Restock drugs
            await RestockDrugsAsync(order);

            await _orderHeaderRepository.SaveAsync();
            await _pharmacyStockRepository.SaveAsync();

            return ServiceResult.SuccessResult();
        }

        /// <summary>
        /// Retrieves all pharmacy orders associated with the specified account.
        /// Returns a collection of pharmacy order DTOs.
        /// </summary>
        public async Task<ServiceResult<IEnumerable<PharmacyOrderDTO>>> GetPharmacyOrdersAsync(string accountId)
        {
            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return ServiceResult<IEnumerable<PharmacyOrderDTO>>.ErrorResult("Pharmacy not found", ErrorType.NotFound);

            var orders = await _orderHeaderRepository.GetAllAsync(
                o => o.PharmacyId == pharmacy.PharmacyID,
                x => x.OrderDetails,
                x => x.Patient,
                x => x.Pharmacy
            );

            if (orders == null || !orders.Any())
                return ServiceResult<IEnumerable<PharmacyOrderDTO>>.ErrorResult("No orders found for this pharmacy.", ErrorType.NotFound);

            var result = _mapper.Map<List<PharmacyOrderDTO>>(orders);
            return ServiceResult<IEnumerable<PharmacyOrderDTO>>.SuccessResult(result);
        }

        // ** Helper Methods **//

        /// <summary>
        /// Retrieves the patient with their cart items for the specified account ID.
        /// Returns error if no items in cart.
        /// </summary>
        private async Task<ServiceResult<Patient>> GetPatientWithCartAsync(string accountId)
        {
            var user = await _patientRepository.GetAsync(
                u => u.AccountId == accountId, true,
                x => x.Account,
                x => x.CartItems
            );

            if (user == null || user.Account == null || user.CartItems == null || !user.CartItems.Any())
            {
                return ServiceResult<Patient>.ErrorResult("No items in cart.", ErrorType.NotFound);
            }

            return ServiceResult<Patient>.SuccessResult(user);
        }

        /// <summary>
        /// Validates the cart items for stock availability and calculates total price.
        /// Returns error if any item is not available or insufficient quantity.
        /// </summary>
        private async Task<ServiceResult<decimal>> ValidateCartItemsAsync(List<CartItem> cartItems)
        {
            decimal totalPrice = 0;

            foreach (var item in cartItems)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );

                if (stock == null)
                    return ServiceResult<decimal>.ErrorResult($"Drug ID {item.DrugId} not found in pharmacy stock.", ErrorType.NotFound);

                if (item.Quantity > stock.QuantityAvailable)
                    return ServiceResult<decimal>.ErrorResult($"Not enough stock for Drug ID {item.DrugId}.", ErrorType.Validation);

                totalPrice += stock.Price * item.Quantity;
            }

            return ServiceResult<decimal>.SuccessResult(totalPrice);
        }

        /// <summary>
        /// Creates a new order for the specified patient, payment method, total price, and pharmacy ID.
        /// Persists the order to the repository.
        /// </summary>
        private async Task<PharmaLink_API.Models.Order> CreateOrderAsync(Patient user, string paymentMethod, decimal totalPrice, int pharmacyId)
        {
            var order = new PharmaLink_API.Models.Order
            {
                PatientId = user.PatientId,
                Name = user.Name,
                PhoneNumber = user.Account.PhoneNumber,
                Email = user.Account.Email,
                Country = user.Country,
                Address = user.Address,
                TotalPrice = totalPrice,
                OrderDate = DateTime.UtcNow,
                PharmacyId = pharmacyId,
                PaymentStatus = SD.PaymentStatusPending,
                Status = SD.StatusPending,
                PaymentMethod = paymentMethod
            };

            await _orderHeaderRepository.CreateAsync(order);
            await _orderHeaderRepository.SaveAsync();

            return order;
        }

        /// <summary>
        /// Creates order details for the specified order ID and cart items.
        /// Persists the order details to the repository.
        /// </summary>
        private async Task CreateOrderDetailsAsync(int orderId, List<CartItem> cartItems)
        {
            foreach (var item in cartItems)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );

                var orderDetail = new OrderDetail
                {
                    OrderId = orderId,
                    DrugId = item.DrugId,
                    PharmacyId = item.PharmacyId,
                    Quantity = item.Quantity,
                    Price = stock.Price
                };

                await _orderDetailRepository.CreateAsync(orderDetail);
            }

            await _orderDetailRepository.SaveAsync();
        }

        /// <summary>
        /// Updates the stock quantities for the specified cart items by decreasing available quantity.
        /// Persists the changes to the repository.
        /// </summary>
        private async Task UpdateStockAsync(List<CartItem> cartItems)
        {
            foreach (var item in cartItems)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );

                if (stock != null)
                    stock.QuantityAvailable -= item.Quantity;
            }

            await _pharmacyStockRepository.SaveAsync();
        }

        /// <summary>
        /// Clears the cart items for the patient by removing them from the repository.
        /// </summary>
        private async Task ClearCartAsync(List<CartItem> cartItems)
        {
            await _cartRepository.RemoveRangeAsync(cartItems);
            await _cartRepository.SaveAsync();
        }

        /// <summary>
        /// Restocks drugs for the specified order by increasing the available quantity in stock.
        /// </summary>
        private async Task RestockDrugsAsync(PharmaLink_API.Models.Order order)
        {
            foreach (var item in order.OrderDetails)
            {
                var stock = await _pharmacyStockRepository.GetAsync(
                    s => s.DrugId == item.DrugId && s.PharmacyId == item.PharmacyId
                );

                if (stock != null)
                    stock.QuantityAvailable += item.Quantity;
            }
        }

    }
}
