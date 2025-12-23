using BillIssue.Api.Authorization;
using BillIssue.Api.Business.Auth;
using BillIssue.Api.Business.Base;
using BillIssue.Api.Business.Operations.Auth;
using BillIssue.Api.Business.Operations.Email;
using BillIssue.Api.Business.Operations.Multilanguage;
using BillIssue.Api.Business.Operations.Notifications;
using BillIssue.Api.Business.Operations.Project;
using BillIssue.Api.Business.Operations.Workspace;
using BillIssue.Api.Business.Project;
using BillIssue.Api.Business.Repositories.Auth;
using BillIssue.Api.Business.Repositories.Confirmations;
using BillIssue.Api.Business.Repositories.Multilanguage;
using BillIssue.Api.Business.Repositories.Project;
using BillIssue.Api.Business.Repositories.Workspace;
using BillIssue.Api.Business.Schedule;
using BillIssue.Api.Business.TimeLogEntry;
using BillIssue.Api.Business.Users;
using BillIssue.Api.Business.Workspace;
using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Base;
using BillIssue.Api.Interfaces.Project;
using BillIssue.Api.Interfaces.Repositories.Auth;
using BillIssue.Api.Interfaces.Repositories.Confirmations;
using BillIssue.Api.Interfaces.Repositories.Multilanguage;
using BillIssue.Api.Interfaces.Repositories.Project;
using BillIssue.Api.Interfaces.Repositories.Workspace;
using BillIssue.Api.Interfaces.Schedule;
using BillIssue.Api.Interfaces.TimeLogEntry;
using BillIssue.Api.Interfaces.User;
using BillIssue.Api.Interfaces.Workspace;
using BillIssue.Api.Models.ConfigurationOptions;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.Models.Enums.Auth;
using BillIssue.Shared.Models.Request.Auth;
using BillIssue.Shared.Models.Request.Email;
using BillIssue.Shared.Models.Request.Multilanguage;
using BillIssue.Shared.Models.Request.Notifications;
using BillIssue.Shared.Models.Request.Project;
using BillIssue.Shared.Models.Request.Workspace;
using BillIssue.Shared.Models.Validators.Auth;
using BillIssue.Shared.Models.Validators.Email;
using BillIssue.Shared.Models.Validators.Multilanguage;
using BillIssue.Shared.Models.Validators.Notifications;
using BillIssue.Shared.Models.Validators.Project;
using BillIssue.Shared.Models.Validators.Workspace;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace BillIssue.Api.Extensions
{
    public static class ServiceCollectionExtenstions
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<SendgridOptions>(config.GetSection(AppSettingKeys.SendgridSection));
            services.Configure<JwtOptions>(config.GetSection(AppSettingKeys.JwtSection));
            services.Configure<DatabaseOptions>(config.GetSection(AppSettingKeys.DatabaseSection));

            return services;
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, RoleRequirementHandler>();
            services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();

            services.AddSingleton<ISessionFacade, SessionFacade>();

            services.AddScoped<IUserFacade, UserFacade>();
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

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IConfirmationRepository, ConfirmationRepository>();
            services.AddScoped<IMultilanguageRepository, MultilanguageRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Auth validators
            services.AddSingleton<IValidator<LoginRequest>, LoginRequestValidator>();
            services.AddSingleton<IValidator<RegisterRequest>, RegisterRequestValidator>();
            services.AddSingleton<IValidator<RemindPasswordRequest>, RemindPasswordRequestValidator>();
            services.AddSingleton<IValidator<RemindPasswordConfirmRequest>, RemindPasswordConfirmRequestValidator>();

            // Multilanguage validators
            services.AddSingleton<IValidator<ClearMultilanguageCachesRequest>, ClearMultilanguageCachesRequestValidator>();
            services.AddSingleton<IValidator<CreateMultilanguageItemRequest>, CreateMultilanguageItemRequestValidator>();
            services.AddSingleton<IValidator<GetAllMultilanguageItemsRequest>, GetAllMultilanguageItemsRequestValidator>();
            services.AddSingleton<IValidator<GetAllTranslationsInCsvRequest>, GetAllTranslationsInCsvRequestValidator>();
            services.AddSingleton<IValidator<ImportMultilanguageCsvRequest>, ImportMultilanguageCsvRequestValidator>();
            services.AddSingleton<IValidator<ModifyMultilanguageItemRequest>, ModifyMultilanguageItemRequestValidator>();

            // Email validators
            services.AddSingleton<IValidator<SendPasswordReminderEmailRequest>, SendPasswordReminderEmailRequestValidator>();

            // Notification validators
            services.AddSingleton<IValidator<GetNotificationsRequest>, GetNotificationsRequestValidator>();
            services.AddSingleton<IValidator<CreateWorkspaceNotificationRequest>, CreateWorkspaceNotificationRequestValidator>();
            services.AddSingleton<IValidator<DoNotificationDecisionRequest>, DoNotificationDecisionRequestValidator>();

            // Project validators
            services.AddSingleton<IValidator<CreateProjectRequest>, CreateProjectRequestValidator>();
            services.AddSingleton<IValidator<GetProjectRequest>, GetProjectRequestValidator>();
            services.AddSingleton<IValidator<GetProjectSelectionsForWorkspacesRequest>, GetProjectSelectionsForWorkspacesRequestValidator>();

            // Workspace validators
            services.AddSingleton<IValidator<GetWorkspaceRequest>, GetWorkspaceRequestValidator>();
            services.AddSingleton<IValidator<CreateWorkspaceRequest>, CreateWorkspaceRequestValidator>();
            services.AddSingleton<IValidator<GetWorkspaceSelectionsForUserRequest>, GetWorkspaceSelectionsForUserRequestValidator>();
            services.AddSingleton<IValidator<GetAllWorkspacesForUserRequest>, GetAllWorkspacesForUserRequestValidator>();
            services.AddSingleton<IValidator<ModifyWorkspaceRequest>, ModifyWorkspaceRequestValidator>();
            services.AddSingleton<IValidator<RemoveWorkspaceRequest>, RemoveWorkspaceRequestValidator>();

            // Workspace User validators
            services.AddSingleton<IValidator<GetAllWorkspaceUsersRequest>, GetAllWorkspaceUsersRequestValidator>();
            services.AddSingleton<IValidator<ModifyUserInWorkspaceRequest>, ModifyUserInWorkspaceRequestValidator>();

            return services;
        }

        public static IServiceCollection AddOperations(this IServiceCollection services)
        {
            services.AddScoped<OperationFactory>();

            // Auth Operations
            services.AddScoped<LoginOperation>();
            services.AddScoped<RegisterOperation>();
            services.AddScoped<RemindPasswordOperation>();
            services.AddScoped<RemindPasswordConfirmOperation>();

            // Multilanguage Operations
            services.AddScoped<ClearMultilanguageCachesOperation>();
            services.AddScoped<CreateMultilanguageItemOperation>();
            services.AddScoped<GetAllMultilanguageItemsOperation>();
            services.AddScoped<GetAllTranslationsInCsvOperation>();
            services.AddScoped<ImportMultilanguageCsvOperation>();
            services.AddScoped<ModifyMultilanguageItemOperation>();

            // Email Operations
            services.AddScoped<SendPasswordReminderEmailOperation>();

            // Notification Operations
            services.AddScoped<GetNotificationsOperation>();
            services.AddScoped<CreateWorkspaceNotificationOperation>();
            services.AddScoped<DoNotificationDecisionOperation>();

            // Project Operations
            services.AddScoped<CreateProjectOperation>();
            services.AddScoped<GetProjectOperation>();
            services.AddScoped<GetProjectSelectionsForWorkspacesOperation>();

            // Workspace Operations
            services.AddScoped<CreateWorkspaceOperation>();
            services.AddScoped<GetWorkspaceOperation>();
            services.AddScoped<GetWorkspaceSelectionsForUserOperation>();
            services.AddScoped<GetAllWorkspacesForUserOperation>();
            services.AddScoped<ModifyWorkspaceOperation>();
            services.AddScoped<RemoveWorkspaceOperation>();

            // Workspace User Operations
            services.AddScoped<GetAllWorkspaceUsersOperation>();
            services.AddScoped<ModifyUserInWorkspaceOperation>();

            return services;
        }
    }
}
