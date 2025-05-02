using Equilibrium.API.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddLocalizationResources();

var app = builder.Build();

app.UseRequestLocalization();
app.UseApiConfiguration(app.Environment);

app.Run();