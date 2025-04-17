using FinanceSystem.Web.Configuration;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebConfiguration();
builder.Services.AddHttpConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfiguration();
builder.Services.AddServices();

var app = builder.Build();


var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseWebConfiguration(app.Environment);

app.Run();