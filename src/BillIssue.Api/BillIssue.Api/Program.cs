using BillIssue.Api.ActionFilters;
using BillIssue.Api.Business.Auth;
using BillIssue.Api.Business.Workspace;
using BillIssue.Api.Business.Email;
using BillIssue.Api.Business.Multilanguage;
using BillIssue.Api.Business.Project;
using BillIssue.Api.Business.Schedule;
using BillIssue.Api.Business.TimeLogEntry;
using BillIssue.Api.Business.Users;
using BillIssue.Api.Interfaces.Auth;
using BillIssue.Api.Interfaces.Workspace;
using BillIssue.Api.Interfaces.Email;
using BillIssue.Api.Interfaces.Multilanguage;
using BillIssue.Api.Interfaces.Project;
using BillIssue.Api.Interfaces.Schedule;
using BillIssue.Api.Interfaces.TimeLogEntry;
using BillIssue.Api.Interfaces.User;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.OperationFilters;
using Npgsql;
using Serilog;
using StackExchange.Redis;
using System.Data;
using BillIssue.Api.Interfaces.Alerts;
using BillIssue.Api.Business.Alerts;

internal class Program
{
    private static void Main(string[] args)
    {
        var AllowedCorsOrigins = "allOrigins";

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: AllowedCorsOrigins,
                policy =>
                {
                    policy.WithOrigins("*")
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        builder.Services.AddControllers( options => 
        {
            options.Filters.Add(new ErrorHandlingFilterAttribute());
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.OperationFilter<AddCustomHeaderParameter>();
        });

        IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
        builder.Host.UseSerilog();

        builder.Services.AddSingleton<NpgsqlConnection>(sp =>
        {
            NpgsqlConnection connection = new NpgsqlConnection(builder.Configuration.GetValue<string>(AppSettingKeys.DatabaseConnectionKey));
            connection.Open();

            return connection;
        }
        );

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(new ConfigurationOptions
                {
                    EndPoints = { $"{builder.Configuration.GetValue<string>(AppSettingKeys.RedisHostKey)}:{builder.Configuration.GetValue<string>(AppSettingKeys.RedisPortKey)}" },
                    Ssl = builder.Configuration.GetValue<bool>(AppSettingKeys.RedisSslEnabledKey),
                    AbortOnConnectFail = false,
                }
            )
        );

        RegisterServices(builder.Services);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors(AllowedCorsOrigins);

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }

    private static void RegisterServices(IServiceCollection services)
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
    }
}