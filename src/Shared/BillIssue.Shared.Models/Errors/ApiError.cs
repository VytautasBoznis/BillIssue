﻿namespace BillIssue.Shared.Models.Errors
{
    public class ApiError
    {
        public string Message { get; set; }
        public string Description { get; set; }
        public string ErrorCode { get; set; }
    }
}
