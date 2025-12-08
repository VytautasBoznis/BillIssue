using BillIssue.Api.Authorization;
using BillIssue.Api.Business.Alerts;
using BillIssue.Api.Business.Auth;
using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Email;
using BillIssue.Api.Business.Multilanguage;
using BillIssue.Api.Business.Project;
using BillIssue.Api.Business.Schedule;
using BillIssue.Api.Business.TimeLogEntry;
using BillIssue.Api.Business.Users;
using BillIssue.Api.Business.Workspace;
using BillIssue.Api.Interfaces.Alerts;
using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Email;
using BillIssue.Api.Interfaces.Multilanguage;
using BillIssue.Api.Interfaces.Project;
using BillIssue.Api.Interfaces.Schedule;
using BillIssue.Api.Interfaces.TimeLogEntry;
using BillIssue.Api.Interfaces.User;
using BillIssue.Api.Interfaces.Workspace;
using BillIssue.Api.Models.ConfigurationOptions;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Validators.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace BillIssue.Api.Extensions
{
    public static class ServiceCollectionExtenstions
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SendgridOptions>(config.GetSection(AppSettingKeys.SendgridSection));
            services.Configure<JwtOptions>(config.GetSection(AppSettingKeys.JwtSection));

            return services;
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, RoleRequirementHandler>();

            services.AddSingleton<IEmailFacade, EmailFacade>();
            services.AddSingleton<IMultilanguageFacade, MultilanguageFacade>();
            services.AddSingleton<ISessionFacade, SessionFacade>();

            services.AddScoped<IAuthFacade, AuthFacade>();
            services.AddScoped<IUserFacade, UserFacade>();
            services.AddScoped<INotificationFacade, NotificationFacade>();
            services.AddScoped<IWorkspaceFacade, WorkspaceFacade>();
            services.AddScoped<IProjectFacade, ProjectFacade>();
            services.AddScoped<IScheduleFacade, ScheduleFacade>();
            services.AddScoped<ITimeLogEntryFacade, TimeLogEntryFacade>();
            services.AddScoped<ITimeLogEntrySearchFacade, TimeLogEntrySearchFacade>();

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthConstants.UserRequiredPolicyName, policy =>
                    policy.Requirements.Add(new RoleRequirement(UserRole.User)));
                options.AddPolicy(AuthConstants.AdminRequiredPolicyName, policy =>
                    policy.Requirements.Add(new RoleRequirement(UserRole.Admin)));
            });

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddSingleton<IValidator<LoginRequest>, LoginRequestValidator>();

            return services;
        }

        public static IServiceCollection AddOperations(this IServiceCollection services)
        {
            services.AddScoped<OperationFactory>();

            services.AddScoped<LoginOperation>();

            return services;
        }
    }
}
