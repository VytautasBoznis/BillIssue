using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Notifications;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Response.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly OperationFactory _operationFactory;

        public NotificationController(OperationFactory operationFactory, ILogger<NotificationController> logger) : base(logger)
        {
            _operationFactory = operationFactory;
        }

        [HttpGet("GetNotifications")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetNotifications()
        {
            GetNotificationsResponse response = await _operationFactory
                                                        .Get<GetNotificationsOperation>()
                                                        .Run(new GetNotificationsRequest { SessionUserData = GetSessionModelFromJwt() });

            return Ok(response);
        }

        [HttpPost("DoNotificationDecision")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> DoNotificationDecision([FromBody] DoNotificationDecisionRequest request)
        {
            request.SessionUserData = GetSessionModelFromJwt();

            DoNotificationDecisionResponse response = await _operationFactory
                                                        .Get<DoNotificationDecisionOperation>()
                                                        .Run(request);

            return Ok(response);
        }
    }
}
