using BillIssue.Api.ActionFilters;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.User;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using Microsoft.AspNetCore.Mvc;
using BillIssue.Shared.Models.Response.User.Dto;
using BillIssue.Shared.Models.Response.User;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly IUserFacade _userFacade;
        public UserController(IUserFacade userFacade, ILogger<UserController> logger) : base(logger)
        {
            _userFacade = userFacade;
        }

        /*[HttpGet("GetCurrentSessionUserData")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<IActionResult> GetCurrentSessionUserData(Guid WorkspaceId)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            SessionUserDto result = await _userFacade.GetCurrentSessionUserData(sessionId);

            return Ok(new GetCurrentSessionUserDataResponse { SessionUserDto = result });
        }*/
    }
}
