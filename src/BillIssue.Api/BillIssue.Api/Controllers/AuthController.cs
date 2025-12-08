using BillIssue.Api.ActionFilters;
using BillIssue.Api.Business.Auth;
using BillIssue.Api.Business.Base;
using BillIssue.Api.Controllers.Base;
using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Response.Auth;
using BillIssue.Shared.Models.Response.Auth.Dto;
using Microsoft.AspNetCore.Mvc;
using SendGrid;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthFacade _authFacade;
        private readonly OperationFactory _operationFactory;

        public AuthController(IAuthFacade authFacade, OperationFactory operationFactory, ILogger<AuthController> logger): base(logger)
        {
            _operationFactory = operationFactory;
            _authFacade = authFacade;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            LoginResponse response = _operationFactory.Get<LoginOperation>(typeof(LoginOperation)).Run(request);

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            SessionDto sessionDto = await _authFacade.Register(request);

            return Ok(new LoginResponse
            {
                Session = sessionDto,
            });
        }

        /*[HttpPost("validateEmail")]
        [TypeFilter(typeof(AuthorizationFilter), Arguments = [UserRole.User])]
        public async Task<OkResult> ValidateEmail(ValidateEmailRequest request)
        {
            string sessionId = Request.Headers[AuthConstants.AuthTokenHeaderName];
            return Ok();
        }

        [HttpPost("validateEmailConfirmation")]
        public async Task<OkResult> ValidateEmailConfirmation(ValidateEmailConfirmRequest request)
        {
            return Ok();
        }*/

        [HttpPost("remindPassword")]
        public async Task<OkResult> RemindPassword(RemindPasswordRequest request)
        {
            //TODO add critical error handlers (E.G sendgrid is down so we handle the 500 but we need to say to the user to comeback later)
            await _authFacade.RemindPassword(request);
            return Ok();
        }

        [HttpPost("remindPasswordConfirmation")]
        public async Task<OkResult> RemindPasswordConfirmation(RemindPasswordConfirmRequest request)
        {
            await _authFacade.RemindPasswordConfirm(request);
            return Ok();
        }
    }
}
