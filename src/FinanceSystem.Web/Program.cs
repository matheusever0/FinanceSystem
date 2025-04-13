using FinanceSystem.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebConfiguration();
builder.Services.AddHttpConfiguration(builder.Configuration);
builder.Services.AddAuthenticationConfiguration();
builder.Services.AddServices();

var app = builder.Build();

app.UseWebConfiguration(app.Environment);

app.Run();