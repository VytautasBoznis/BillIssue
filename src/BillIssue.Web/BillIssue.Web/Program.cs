using BlazorStrap;
using Soenneker.Blazor.TomSelect.Registrars;
using BillIssue.Web.Initialization;
using BillIssue.Web.Business.RestClient;
using BillIssue.Web.Domain.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add core services to the app.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorStrap();
builder.Services.AddWMBOS();
builder.Services.AddTomSelect();

builder.Services.AddMemoryCache(x => x.SizeLimit = 256);

builder.Services.AddHttpClient<BillIssueApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>(ConfigurationKeys.ApiUrl));
});

ServiceConfiguration.ConfigureServices(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
