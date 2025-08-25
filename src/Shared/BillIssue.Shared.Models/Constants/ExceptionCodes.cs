namespace BillIssue.Shared.Models.Constants
{
    public static class ExceptionCodes
    {
        #region Generic

        public const string UNEXPECTED_EXCEPTION = "UNEXPECTED_EXCEPTION";

        #endregion

        #region Auth

        public const string AUTH_UNOTHORIZED_EXCEPTION = "AUTH_UNOTHORIZED_EXCEPTION";
        public const string AUTH_SESSION_EXPIRED = "AUTH_SESSION_EXPIRED";
        public const string AUTH_REGISTRATION_EMAIL_ALREADY_IN_USE = "AUTH_REGISTRATION_EMAIL_ALREADY_IN_USE";
        public const string AUTH_EMAIL_AND_PASSWORD_MISSMATCH = "AUTH_EMAIL_AND_PASSWORD_MISSMATCH";
        public const string AUTH_CONTACT_SUPPORT = "AUTH_CONTACT_SUPPORT";
        public const string AUTH_FAILED_TO_UPDATE_USER = "AUTH_FAILED_TO_UPDATE_USER";

        #endregion

        #region Company Workspace

        public const string WORKSPACE_NOT_FOUND = "WORKSPACE_NOT_FOUND";
        public const string WORKSPACES_NOT_FOUND = "WORKSPACES_NOT_FOUND";
        public const string WORKSPACE_FAILED_TO_CREATE = "WORKSPACE_FAILED_TO_CREATE";
        public const string WORKSPACE_CAN_NOT_CREATE_ROLE_ASSINGMENTS_HIGHER_THAN_CURRENT_ROLE = "WORKSPACE_CAN_NOT_CREATE_ROLE_ASSINGMENTS_HIGHER_THAN_CURRENT_ROLE";
        public const string WORKSPACE_USER_ASSIGNMENT_FAILED_TO_CREATE = "WORKSPACE_USER_ASSIGNMENT_FAILED_TO_CREATE";
        public const string WORKSPACE_USER_ASSIGNMENT_NOT_FOUND = "WORKSPACE_USER_ASSIGNMENT_NOT_FOUND";
        public const string WORKSPACE_USER_CAN_NOT_REMOVE_A_WORKSPACE_ASSIGNMENT_THAT_IS_OF_HIGHER_ROLE = "WORKSPACE_USER_CAN_NOT_REMOVE_A_WORKSPACE_ASSIGNMENT_THAT_IS_OF_HIGHER_ROLE";
        public const string WORKSPACE_USER_NOT_FOUND_IN_WORKSPACE = "WORKSPACE_USER_NOT_FOUND_IN_WORKSPACE";

        #endregion

        #region Project

        public const string PROJECT_FAILED_TO_CREATE = "PROJECT_FAILED_TO_CREATE";
        public const string PROJECT_NOT_FOUND = "PROJECT_NOT_FOUND";
        public const string PROJECT_FAILED_TO_MODIFY = "PROJECT_FAILED_TO_MODIFY";

        #endregion

        #region Project Worktypes

        public const string PROJECT_WORKTYPE_FAILED_TO_CREATE = "PROJECT_WORKTYPE_FAILED_TO_CREATE";
        public const string PROJECT_WORKTYPE_NOT_FOUND = "PROJECT_WORKTYPE_NOT_FOUND";

        #endregion

        #region Time log entry

        public const string TIME_LOG_ENTRY_FAILED_TO_CREATE = "TIME_LOG_ENTRY_FAILED_TO_CREATE";
        public const string TIME_LOG_ENTRY_FAILED_TO_MODIFY = "TIME_LOG_ENTRY_FAILED_TO_MODIFY";
        public const string TIME_LOG_ENTRY_NOT_FOUND = "TIME_LOG_ENTRY_NOT_FOUND";

        #endregion

        #region User

        public const string USER_NOT_FOUND = "USER_NOT_FOUND";

        #endregion
    }
}
