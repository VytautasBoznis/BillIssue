using BillIssue.Api.Business.Alerts;
using BillIssue.Api.Business.Auth;
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
    }
}
