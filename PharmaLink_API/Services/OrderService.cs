using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Core.Results;
using PharmaLink_API.Data;
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
        private readonly ApplicationDbContext _dbContext;
        private readonly ICartRepository _cartRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IPharmacyStockRepository _pharmacyStockRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IMapper _mapper;
        private readonly IStripeService _stripeService;

        public OrderService(
            ApplicationDbContext dbContext,
            ICartRepository cartRepository,
            IPatientRepository patientRepository,
            IOrderHeaderRepository orderHeaderRepository,
            IOrderDetailRepository orderDetailRepository,
            IPharmacyStockRepository pharmacyStockRepository,
            IPharmacyRepository pharmacyRepository,
            IMapper mapper,
            IStripeService stripeService)
        {
            _dbContext = dbContext;
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

            if (order.Status == SD.StatusPending)
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
        /// Updates the status of the specified order to "Out For Delivery" for the given pharmacy account.
        /// Only pending orders can be updated to out for delivery.
        /// </summary>
        public async Task<ServiceResult> OutForDeliveryOrderAsync(int orderId, string accountId)
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
                return ServiceResult.ErrorResult("Only pending orders can be out for delivery.", ErrorType.Validation);

            order.Status = SD.StatusOutForDelivery;
            await _orderHeaderRepository.SaveAsync();

            return ServiceResult.SuccessResult();
        }

        /// <summary>
        /// Updates the status of the specified order to "Reviewing" for the given pharmacy account if the order is under review.
        /// Returns the order details DTO.
        /// </summary>
        public async Task<ServiceResult<OrderDetailsDTO>> ReviewingOrderAsync(int orderId, string accountId)
        {
            var order = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.PharmacyProduct)
                .ThenInclude(pp => pp.Drug)
                .Include(o => o.Patient)
                .ThenInclude(p => p.Account)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);
            if (order == null)
                return ServiceResult<OrderDetailsDTO>.ErrorResult("Order not found", ErrorType.NotFound);

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return ServiceResult<OrderDetailsDTO>.ErrorResult("Pharmacy not found", ErrorType.NotFound);

            if (order.PharmacyId != pharmacy.PharmacyID)
                return ServiceResult<OrderDetailsDTO>.ErrorResult("You are not authorized to update this order.", ErrorType.Authorization);

            if (order.Status != SD.StatusUnderReview)
            {
                var dto = BuildOrderDetailsDto(order);
                return ServiceResult<OrderDetailsDTO>.SuccessResult(dto);
            }

            order.Status = SD.StatusReviewing;

            await _orderHeaderRepository.SaveAsync();

            var updatedDto = BuildOrderDetailsDto(order);
            return ServiceResult<OrderDetailsDTO>.SuccessResult(updatedDto);
        }

        /// <summary>
        /// Updates the status of the specified order to "Pending" for the given pharmacy account if the order is currently being reviewed.
        /// </summary>
        public async Task<ServiceResult> PendingOrderAsync(int orderId, string accountId)
        {
            var order = await _orderHeaderRepository.GetAsync(o => o.OrderID == orderId, true, x => x.OrderDetails);
            if (order == null)
                return ServiceResult.ErrorResult("Order not found", ErrorType.NotFound);

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return ServiceResult.ErrorResult("Pharmacy not found", ErrorType.NotFound);

            if (order.PharmacyId != pharmacy.PharmacyID)
                return ServiceResult.ErrorResult("You are not authorized to update this order.", ErrorType.Authorization);

            if (order.Status != SD.StatusReviewing)
                return ServiceResult.ErrorResult("Only orders reviewed can be updated.", ErrorType.Validation);

            order.Status = SD.StatusPending;
            await _orderHeaderRepository.SaveAsync();
            return ServiceResult.SuccessResult();
        }

        /// <summary>
        /// Updates the status of the specified order to "Delivered" for the given pharmacy account if the order is out for delivery.
        /// </summary>
        public async Task<ServiceResult> OrderDeliveredAsync(int orderId, string accountId)
        {
            var order = await _orderHeaderRepository.GetAsync(o => o.OrderID == orderId, true, x => x.OrderDetails);
            if (order == null)
                return ServiceResult.ErrorResult("Order not found", ErrorType.NotFound);

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return ServiceResult.ErrorResult("Pharmacy not found", ErrorType.NotFound);

            if (order.PharmacyId != pharmacy.PharmacyID)
                return ServiceResult.ErrorResult("You are not authorized to update this order.", ErrorType.Authorization);

            if (order.Status != SD.StatusOutForDelivery)
                return ServiceResult.ErrorResult("Only orders out for delivery can be updated.", ErrorType.Validation);

            order.Status = SD.StatusDelivered;
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

            if (order.Status != SD.StatusUnderReview && order.Status != SD.StatusReviewing && order.Status != SD.StatusPending)
                return ServiceResult.ErrorResult("Delivred, out for delivery, rejected or canceled orders cannot be rejected.", ErrorType.Validation);

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

        /// <summary>
        /// Searches pharmacy orders for the specified account using a query string.
        /// The query matches patient names (case-insensitive, partial match) or order IDs.
        /// Returns a list of matching PharmacyOrderDTOs.
        /// </summary>
        public async Task<ServiceResult<List<PharmacyOrderDTO>>> SearchOrdersAsync(string accountId, string query)
        {
            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return new ServiceResult<List<PharmacyOrderDTO>>();

            query = query.ToLower().Trim();

            var orders = await _orderHeaderRepository.GetAllAsync(
                o => o.PharmacyId == pharmacy.PharmacyID &&
                     (EF.Functions.Like(o.Patient.Name, $"%{query}%") || o.OrderID.ToString().Contains(query)),
                x => x.OrderDetails,
                x => x.Patient,
                x => x.Pharmacy
            );
            if (orders == null || !orders.Any())
                return new ServiceResult<List<PharmacyOrderDTO>>();
            var mappedOrders = _mapper.Map<List<PharmacyOrderDTO>>(orders);
            return ServiceResult<List<PharmacyOrderDTO>>.SuccessResult(mappedOrders);
        }

        /// <summary>
        /// Filters pharmacy orders for the specified account by order status.
        /// Returns a list of PharmacyOrderDTOs with the given status.
        /// </summary>
        public async Task<ServiceResult<List<PharmacyOrderDTO>>> FilterOrdersByStatusAsync(string accountId, string status)
        {
            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return ServiceResult<List<PharmacyOrderDTO>>.ErrorResult("Pharmacy not found", ErrorType.NotFound);
            var orders = await _orderHeaderRepository.GetAllAsync(
                o => o.PharmacyId == pharmacy.PharmacyID && o.Status == status,
                x => x.OrderDetails,
                x => x.Patient,
                x => x.Pharmacy
            );
            if (orders == null || !orders.Any())
                return ServiceResult<List<PharmacyOrderDTO>>.ErrorResult("No orders found for this pharmacy with the specified status.", ErrorType.NotFound);
            var result = _mapper.Map<List<PharmacyOrderDTO>>(orders);
            return ServiceResult<List<PharmacyOrderDTO>>.SuccessResult(result);
        }

        public async Task<ServiceResult<PharmacyAnalysisDTO>> GetPharmacyAnalysisAsync(string accountId)
        {
            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return ServiceResult<PharmacyAnalysisDTO>.ErrorResult("Pharmacy not found", ErrorType.NotFound);

            // Fix for CS1061: Replace the incorrect property access `.Drug` with the correct navigation path to access the `Drug` entity.
            // The `OrderDetail` class does not have a direct `Drug` property, but it has a `PharmacyProduct` property, which in turn has a `Drug` property.

            var orders = await _orderHeaderRepository.GetAllWithDetailsAsync(
                filter: o => o.PharmacyId == pharmacy.PharmacyID && o.Status == "Completed",
                include: query => query
                    .Include(ph => ph.Patient)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.PharmacyProduct) 
                    .ThenInclude(pp => pp.Drug)           
            );

            if (orders == null || !orders.Any())
                return ServiceResult<PharmacyAnalysisDTO>.ErrorResult("No completed orders found for this pharmacy.", ErrorType.NotFound);

            // Calculate overall statistics
            var totalRevenue = orders.Sum(o => o.TotalPrice);
            var totalOrders = orders.Count;
            var uniqueCustomers = orders.Select(o => o.PatientId).Distinct().Count();

            // Calculate monthly statistics
            var monthlyStats = orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new MonthlyOrderStats
                {
                    MonthYear = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(x => x.MonthYear)
                .ToList();

            // Calculate top selling products
            var topProducts = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.DrugId)
                .Select(g => new TopSellingProduct
                {
                    DrugId = g.Key,
                    DrugName = g.Select(od => od.PharmacyProduct?.Drug?.CommonName).FirstOrDefault() ?? "Unknown",
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.Price)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10) 
                .ToList();
            var topCustomer = orders
                .GroupBy(o => o.PatientId)
                .Select(g => new TopCustomers
                {
                    CustomerId = g.Key,
                    TotalSpent = g.Sum(o => o.TotalPrice),
                    CustomerName = g.Select(o => o.Patient.Name).FirstOrDefault() ?? "Unknown",
                    TotalOrders = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(10)
                .ToList();

            var result = new PharmacyAnalysisDTO
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalUniqueCustomers = uniqueCustomers,
                MonthlyStats = monthlyStats,
                TopSellingProducts = topProducts,
                TopCustomers = topCustomer
            };

            return ServiceResult<PharmacyAnalysisDTO>.SuccessResult(result);
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
                Status = SD.StatusUnderReview,
                PaymentMethod = paymentMethod
            };

            await _orderHeaderRepository.CreateAndSaveAsync(order);
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

                await _orderDetailRepository.CreateAndSaveAsync(orderDetail);
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

        /// <summary>
        /// Builds an OrderDetailsDTO object from the provided Order entity.
        /// Maps order and patient details, payment information, status, and a list of medicines with quantities.
        /// </summary>
        private OrderDetailsDTO BuildOrderDetailsDto(Order order)
        {
            return new OrderDetailsDTO
            {
                OrderId = order.OrderID,
                OrderDate = order.OrderDate,
                PatientName = order.Patient?.Name ?? "Unknown",
                PatientPhone = order.Patient?.Account?.PhoneNumber ?? "Unknown",
                PatientAddress = order.Patient?.Address ?? "Unknown",
                PaymentMethod = order.PaymentMethod,
                Total = order.TotalPrice,
                CurrentStatus = order.Status,
                EstimatedCompletionMinutes = 10,
                Medicines = order.OrderDetails.Select(od => $"{od.PharmacyProduct?.Drug?.CommonName} (Qty: {od.Quantity})").ToList()
            };
        }

        
    }
}
