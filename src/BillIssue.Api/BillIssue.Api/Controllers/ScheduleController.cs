using BillIssue.Api.ActionFilters;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.Schedule;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using Microsoft.AspNetCore.Mvc;
using BillIssue.Shared.Models.Request.Schedule;
using BillIssue.Shared.Models.Response.Schedule.Dto;
using BillIssue.Shared.Models.Response.Schedule;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : BaseController
    {
        private readonly IScheduleFacade _scheduleFacade;
        public ScheduleController(IScheduleFacade scheduleFacade, ILogger<ScheduleController> logger) : base(logger)
        {
            _scheduleFacade = scheduleFacade;
        }

        /*[HttpGet("GetUserSchedule")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public IActionResult GetUserSchedule()
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            UserScheduleDto result = _scheduleFacade.GetUserSchedule(sessionId, new GetUserScheduleRequest());

            return Ok(new GetUserScheduleResponse { UserScheduleDto = result });
        }*/
    }
}
