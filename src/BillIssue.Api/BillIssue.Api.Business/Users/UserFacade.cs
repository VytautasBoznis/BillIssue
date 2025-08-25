using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.User;
using BillIssue.Api.Models.Exceptions;
using BillIssue.Api.Models.Models.Auth;
using BillIssue.Shared.Models.Constants;
using BillIssue.Shared.Models.Response.User.Dto;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BillIssue.Api.Business.Users
{
    public class UserFacade : IUserFacade
    {
        private readonly NpgsqlConnection _dbConnection;
        private readonly ILogger<UserFacade> _logger;

        private readonly ISessionFacade _sessionFacade;

        public UserFacade(
            ISessionFacade sessionFacade,
            NpgsqlConnection dbConnection,
            ILogger<UserFacade> logger)
        {
            _sessionFacade = sessionFacade;
            _dbConnection = dbConnection;
            _logger = logger;
        }

        #region User Facade
        
        public async Task<SessionUserDto> GetCurrentSessionUserData(string sessionId)
        {
            SessionModel sessionModel = await _sessionFacade.GetSessionModel(sessionId);

            SessionUserDto sessionUserDto = GetUserDataById(sessionModel.Id);

            if (sessionUserDto == null)
            {
                _logger.LogError($"Failed to find user in the database for this session, user id: {sessionModel.Id}, email: {sessionModel.Email}");
                throw new UserException("User not found", ExceptionCodes.USER_NOT_FOUND);
            }

            return sessionUserDto;
        }

        #endregion

        #region Private functions

        private SessionUserDto GetUserDataById(Guid userId)
        {
            var dictionary = new Dictionary<string, object> { { "@ui", userId } };

            SessionUserDto sessionUserDto = _dbConnection.Query<SessionUserDto>(
                @"SELECT
                    id as UserId,
                    email,
                    first_name as FirstName,
                    last_name as LastName,
                    role
	            FROM user_users uu
                WHERE
	                uu.id = @ui", dictionary).FirstOrDefault();

            return sessionUserDto != null ? sessionUserDto : new SessionUserDto();
        }

        #endregion
    }
}
