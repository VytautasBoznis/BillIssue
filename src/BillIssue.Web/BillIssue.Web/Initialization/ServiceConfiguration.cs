using BillIssue.Interfaces.Multilanguage;
using BillIssue.Web.Business.Account;
using BillIssue.Web.Business.Multilanguage;
using BillIssue.Web.Data;
using BillIssue.Web.Interfaces.Account;
using BillIssue.Web.Services;

namespace BillIssue.Web.Initialization
{
    public static class ServiceConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            AddSingletons(services);
            AddScoped(services);
        }

        public static void AddSingletons(IServiceCollection services)
        {
            services.AddSingleton<IMultilanguageFacade, MultilanguageFacade>();
            services.AddSingleton<IAccountFacade, AccountFacade>();
            services.AddSingleton<WeatherForecastService>();
        }

        public static void AddScoped(IServiceCollection services)
        {
            services.AddScoped<NavScrollService>();
            services.AddScoped<StateService>();
            services.AddScoped<MenuDataService>();
            services.AddScoped<MultilanguageService>();
            services.AddScoped<AccountService>();
        }
    }
}
