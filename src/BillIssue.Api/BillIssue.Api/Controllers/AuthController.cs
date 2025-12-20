using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Auth;
using BillIssue.Api.Controllers.Base;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Response.Auth;
using Microsoft.AspNetCore.Mvc;

namespace BillIssue.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly OperationFactory _operationFactory;

        public AuthController(OperationFactory operationFactory, ILogger<AuthController> logger): base(logger)
        {
            _operationFactory = operationFactory;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            LoginResponse response = await _operationFactory
                                                .Get<LoginOperation>(typeof(LoginOperation))
                                                .Run(request);

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            RegisterResponse response = await _operationFactory
                                               .Get<RegisterOperation>(typeof(RegisterOperation))
                                               .Run(request);

            return Ok(response);
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
            RemindPasswordResponse response = await _operationFactory
                                                       .Get<RemindPasswordOperation>(typeof(RemindPasswordOperation))
                                                       .Run(request);

            return Ok();
        }

        [HttpPost("remindPasswordConfirmation")]
        public async Task<OkResult> RemindPasswordConfirmation(RemindPasswordConfirmRequest request)
        {
            RemindPasswordConfirmResponse response = await _operationFactory
                                                               .Get<RemindPasswordConfirmOperation>(typeof(RemindPasswordConfirmOperation))
                                                               .Run(request);
            return Ok();
        }
    }
}
