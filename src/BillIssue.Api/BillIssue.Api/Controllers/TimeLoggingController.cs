using BillIssue.Api.ActionFilters;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.TimeLogEntry;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using Microsoft.AspNetCore.Mvc;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry.Dto;
using BillIssue.Shared.Models.Response.TimeLogEntry;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeLoggingController : BaseController
    {
        private readonly ITimeLogEntryFacade _timeLogEntryFacade;
        private readonly ITimeLogEntrySearchFacade _timeLogEntrySearchFacade;

        public TimeLoggingController(ITimeLogEntryFacade timeLogEntryFacade, ITimeLogEntrySearchFacade timeLogEntrySearchFacade)
        {
            _timeLogEntryFacade = timeLogEntryFacade;
            _timeLogEntrySearchFacade = timeLogEntrySearchFacade;
        }

        #region Time Log Entry CRUD

        [HttpGet("GetTimeLogEntry/{timeLogEntryId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetTimeLogEntry(Guid timeLogEntryId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            TimeLogEntryDto result = await _timeLogEntryFacade.GetTimeLogEntry(sessionId, new GetTimeLogEntryRequest { TimeLogEntryId = timeLogEntryId });

            return Ok(new GetTimeLogEntryResponse { TimeLogEntryDto = result });
        }

        [HttpPost("CreateTimeLogEntry")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> CreateTimeLogEntry([FromBody] CreateTimeLogEntryRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            TimeLogEntryDto result = await _timeLogEntryFacade.CreateTimeLogEntry(sessionId, request);

            return Ok(new CreateTimeLogEntryResponse { TimeLogEntryDto = result });
        }

        [HttpPatch("ModifyTimeLogEntry")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> ModifyTimeLogEntry([FromBody] ModifyTimeLogEntryRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            TimeLogEntryDto result = await _timeLogEntryFacade.ModifyTimeLogEntry(sessionId, request);

            return Ok(new ModifyTimeLogEntryResponse { TimeLogEntryDto = result });
        }

        [HttpDelete("RemoveTimeLogEntry/{timeLogEntryId}")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> RemoveProject(Guid timeLogEntryId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            await _timeLogEntryFacade.RemoveTimeLogEntry(sessionId, new RemoveTimeLogEntryRequest
            {
                TimeLogEntryId = timeLogEntryId
            });

            return Ok();
        }

        #endregion

        #region Time Log Entry Search

        [HttpPost("SearchTimeLogEntries")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> SearchTimeLogEntries([FromBody] SearchTimeLogEntriesRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<TimeLogEntryDto> result = await _timeLogEntrySearchFacade.SearchTimeLogEntries(sessionId, request);

            return Ok(new SearchTimeLogEntriesResponse { TimeLogEntryDtos = result });
        }

        [HttpPost("GetWeekOfTimeEntries")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetWeekOfTimeEntries([FromBody] GetWeekOfTimeEntriesRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            List<TimeLogEntriesForDay> result = await _timeLogEntrySearchFacade.GetWeekOfTimeEntries(sessionId, request);

            return Ok(new GetWeekOfTimeEntriesResponse { TimeLogEntriesForWeek = result });
        }

        #endregion
    }
}
