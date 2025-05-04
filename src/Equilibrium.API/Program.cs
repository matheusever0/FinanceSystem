using Equilibrium.API.Configuration;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue;
});
builder.Services.AddApiConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddLocalizationResources();

var app = builder.Build();

app.UseRequestLocalization();
app.UseApiConfiguration(app.Environment);

app.Run();