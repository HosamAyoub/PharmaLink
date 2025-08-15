using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PharmaLink_API.Core.Enums;
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
        public NotificationsController(IOrderHeaderRepository orderHeaderRepository, IPatientRepository petientRepository, IPharmacyRepository pharmacyRepository)
        {
            _orderHeaderRepository = orderHeaderRepository;
            _petientRepository = petientRepository;
            _pharmacyRepository = pharmacyRepository;
        }

        [Authorize(Roles = "Patient")]
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized("Invalid token.");

            var patient = await _petientRepository.GetAsync(p => p.AccountId == accountId);
            if (patient == null)
                return NotFound("Patient not found.");

            var ordersWithNotifications = await _orderHeaderRepository.GetAllAsync(
                o => o.PatientId == patient.PatientId && o.Message != null
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
                o => o.PatientId == patient.PatientId && o.Message != null && o.IsRead == false
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

            var ordersWithNotifications = await _orderHeaderRepository.GetAllAsync(
                o => o.PharmacyId == pharmacy.PharmacyID && o.Status == SD.StatusUnderReview
            );

            var notifications = ordersWithNotifications
                .Select(o => new
                {
                    o.OrderID,
                    o.Status,
                    o.Message,
                    Timestamp = o.OrderDate
                })
                .OrderByDescending(n => n.Timestamp)
                .ToList();

            return Ok(notifications);
        }

    }
}
