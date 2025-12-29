using BillIssue.Api.ActionFilters;
using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.TimeLogEntry;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Authentication;
using BillIssue.Shared.Models.Request.TimeLogEntry;
using BillIssue.Shared.Models.Response.TimeLogEntry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeLoggingController : BaseController
    {
        private readonly OperationFactory _operationFactory;

        public TimeLoggingController(
            ILogger<TimeLoggingController> logger,
            OperationFactory operationFactory
        ) : base(logger)
        {
            _operationFactory = operationFactory;
        }

        #region Time Log Entry CRUD

        [HttpGet("GetTimeLogEntry/{timeLogEntryId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> GetTimeLogEntry(Guid timeLogEntryId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            GetTimeLogEntryResponse response = await _operationFactory
                                                        .Get<GetTimeLogEntryOperation>(typeof(GetTimeLogEntryOperation))
                                                        .Run(new GetTimeLogEntryRequest { 
                                                            SessionUserData = session, 
                                                            TimeLogEntryId = timeLogEntryId 
                                                        });

            return Ok(response);
        }

        [HttpPost("CreateTimeLogEntry")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> CreateTimeLogEntry([FromBody] CreateTimeLogEntryRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            CreateTimeLogEntryResponse response = await _operationFactory
                                                            .Get<CreateTimeLogEntryOperation>(typeof(CreateTimeLogEntryOperation))
                                                            .Run(request);

            return Ok(response);
        }

        [HttpPatch("ModifyTimeLogEntry")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> ModifyTimeLogEntry([FromBody] ModifyTimeLogEntryRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            ModifyTimeLogEntryResponse response = await _operationFactory
                                                            .Get<ModifyTimeLogEntryOperation>(typeof(ModifyTimeLogEntryOperation))
                                                            .Run(request);

            return Ok(response);
        }

        [HttpDelete("RemoveTimeLogEntry/{timeLogEntryId}")]
        [Authorize(Policy = AuthConstants.UserRequiredPolicyName)]
        public async Task<IActionResult> RemoveTimeLogEntry(Guid timeLogEntryId)
        {
            SessionUserData session = GetSessionModelFromJwt();

            RemoveTimeLogEntryResponse response = await _operationFactory
                                                            .Get<RemoveTimeLogEntryOperation>(typeof(RemoveTimeLogEntryOperation))
                                                            .Run(new RemoveTimeLogEntryRequest
                                                            {
                                                                TimeLogEntryId = timeLogEntryId,
                                                                SessionUserData = session
                                                            });

            return Ok(response);
        }

        #endregion

        #region Time Log Entry Search

        [HttpPost("SearchTimeLogEntries")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> SearchTimeLogEntries([FromBody] SearchTimeLogEntriesRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            SearchTimeLogEntriesResponse response = await _operationFactory
                                                            .Get<SearchTimeLogEntriesOperation>(typeof(SearchTimeLogEntriesOperation))
                                                            .Run(request);

            return Ok(response);
        }

        [HttpPost("GetWeekOfTimeEntries")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetWeekOfTimeEntries([FromBody] GetWeekOfTimeEntriesRequest request)
        {
            SessionUserData session = GetSessionModelFromJwt();
            request.SessionUserData = session;

            GetWeekOfTimeEntriesResponse response = await _operationFactory
                                                            .Get<GetWeekOfTimeEntriesOperation>(typeof(GetWeekOfTimeEntriesOperation))
                                                            .Run(request);

            return Ok(response);
        }

        #endregion
    }
}
