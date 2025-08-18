using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Core.Enums;
using PharmaLink_API.Models;
using PharmaLink_API.Repository;
using PharmaLink_API.Repository.Interfaces;
using System.Security.Claims;

namespace PharmaLink_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IOrderHeaderRepository _orderHeaderRepository;
        private readonly IPatientRepository _petientRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IDrugRepository _drugRepository;
        public NotificationsController(IOrderHeaderRepository orderHeaderRepository, IPatientRepository petientRepository, IPharmacyRepository pharmacyRepository , IDrugRepository drugRepository)
        {
            _orderHeaderRepository = orderHeaderRepository;
            _petientRepository = petientRepository;
            _pharmacyRepository = pharmacyRepository;
            _drugRepository = drugRepository;
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotificationsForPatient()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var patient = await _petientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            // Get orders with notifications for the patient
            var ordersWithNotifications = await _orderHeaderRepository.GetAllAsync(
                o => o.PatientId == patient.PatientId 
                && o.Message != null
                && (o.Status == SD.StatusOutForDelivery || o.Status == SD.StatusRejected)
            );

            var notifications = ordersWithNotifications
                .Select(o => new
                {
                    o.OrderID,
                    o.Status,
                    o.Message,
                    o.IsRead,
                    Timestamp = o.StatusLastUpdated
                })
                .OrderByDescending(n => n.Timestamp)
                .ToList();

            return Ok(notifications);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost("markAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var patient = await _petientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            var ordersWithNotifications = await _orderHeaderRepository.GetAllAsync(
                o => o.PatientId == patient.PatientId && o.IsRead == false
            );

            if (!ordersWithNotifications.Any())
                return Ok(new { message = "No unread notifications found." });

            foreach (var order in ordersWithNotifications)
            {
                order.IsRead = true;
            }

            await _orderHeaderRepository.SaveAsync();

            return Ok(new { message = "All notifications marked as read." });

        }

        [Authorize(Roles = "Pharmacy")]
        [HttpGet("pharmacyOrdersNotifications")]
        public async Task<IActionResult> GetNotificationsForPharmacy()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var pharmacy = await _pharmacyRepository.GetAsync(p => p.AccountId == accountId);
            if (pharmacy == null)
                return NotFound("pharmacy not found.");

            // Get orders with notifications for the pharmacy
            var ordersWithNotifications = await _orderHeaderRepository.GetAllAsync(
                o => o.PharmacyId == pharmacy.PharmacyID && (o.Status == SD.StatusUnderReview || o.Status == SD.StatusCancelled)
            );

            // Get drug requests notifications for 
            var DrugRequestesNotifications = await _drugRepository.GetAllAsync(
                d => d.CreatedByPharmacy == pharmacy.PharmacyID && d.IsRead == false && d.DrugStatus != Status.Pending  
            );

            // Replace the problematic code block in GetNotificationsForPharmacy with the following:

            var orders = ordersWithNotifications
                .Select(o => new
                {
                    o.OrderID,
                    o.Status,
                    o.Message,
                    Timestamp = o.OrderDate,
                    Type = o.Status == SD.StatusCancelled ? "cancelOrder" : o.Status == SD.StatusUnderReview ? "order" : "Unknown"
                })
                .OrderByDescending(n => n.Timestamp)
                .ToList();

            var Requests = DrugRequestesNotifications
                .Select(d => new
                {
                    d.DrugID,
                    d.CommonName,
                    d.DrugStatus,
                    d.IsRead,
                    Timestamp = d.CreatedAt
                })
                .OrderByDescending(n => n.Timestamp)
                .ToList();

            var notifications = new
            {
                OrderNotifications = orders,
                DrugRequestNotifications = Requests
            };

            return Ok(notifications);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("AdminNotifications")]
        public async Task<IActionResult> GetNotificationsForAdmin()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");
            // Get all drug requests from all pharmacies
            var drugRequests = await _drugRepository.GetAllAsync(d => d.DrugStatus == Status.Pending && d.IsRead == false && d.CreatedAt >= DateTime.Today );

            if (drugRequests == null || !drugRequests.Any())
                return Ok(new { message = "No drug requests found." });

            var Notifications = drugRequests
                .Select(d => new
                {
                    d.DrugID,
                    d.CommonName,
                    d.DrugStatus,
                    d.IsRead,
                    Timestamp = d.CreatedAt
                })
                .OrderByDescending(n => n.Timestamp)
                .ToList();
            return Ok(Notifications);
        }



    }
}
