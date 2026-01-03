using BillIssue.Api.ActionFilters;
using BillIssue.Api.Extensions;
using BillIssue.Api.Models.ConfigurationOptions;
using BillIssue.Api.Models.Constants;
using BillIssue.Api.OperationFilters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Text;

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

            // Swagger JWT support
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer {token}'"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] { }
                }
            });
        });

        IConfiguration configuration = new ConfigurationBuilder()
                                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                            .AddEnvironmentVariables()
                                            .Build();

        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
        builder.Host.UseSerilog();

        var jwtOptions = configuration.GetSection(AppSettingKeys.JwtSection).Get<JwtOptions>();

        if (jwtOptions == null)
        {
            throw new InvalidOperationException("Jwt configuration is missing or invalid in appsettings.json.");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = !string.IsNullOrEmpty(jwtOptions.Issuer),
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = !string.IsNullOrEmpty(jwtOptions.Audience),
                ValidAudience = jwtOptions.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
            };
        });

        builder.Services.AddAuthorization();

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        var redisOptions = configuration.GetSection(AppSettingKeys.RedisSection).Get<RedisOptions>();

        if(redisOptions == null)
        {
            throw new InvalidOperationException("Redis configuration is missing or invalid in appsettings.json.");
        }

        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(new ConfigurationOptions
                {
                    EndPoints = { $"{redisOptions.Host}:{redisOptions.Port}" },
                    Ssl = redisOptions.SslEnabled,
                    AbortOnConnectFail = false,
                }
            )
        );

        builder.Services
            .AddConfig(configuration)
            .AddRepositories()
            .RegisterServices()
            .AddValidators()
            .AddOperations()
            .AddAuthorizationPolicies();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors(AllowedCorsOrigins);

        // Enable authentication middleware
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}