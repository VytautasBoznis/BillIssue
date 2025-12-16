using BillIssue.Api.ActionFilters;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.Notifications;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Response.Notifications;
using BillIssue.Shared.Models.Response.Notifications.Dto;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly INotificationFacade _notificationFacade;

        public NotificationController(INotificationFacade notificationFacade, ILogger<NotificationController> logger) : base(logger)
        {
            _notificationFacade = notificationFacade;
        }

        [HttpGet("GetNotifications")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<GetNotificationsResponse> GetNotifications()
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<NotificationDto> notificationDtos = await _notificationFacade.GetUserNotifications(sessionId, new GetNotificationsRequest());

            return new GetNotificationsResponse
            {
                NotificationDtos = notificationDtos,
            };
        }

        [HttpPost("DoNotificationDecision")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> DoNotificationDecision([FromBody] DoNotificationDecisionRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _notificationFacade.DoNotificationDecision(sessionId, request);

            return Ok();
        }
    }
}
