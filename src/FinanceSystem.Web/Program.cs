using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
    });

builder.Services.AddHttpClient("FinanceAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Garante que os arquivos estáticos podem ser acessados
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true
});

// Mantenha também a chamada padrão
app.UseStaticFiles();

// Loga os arquivos na pasta wwwroot para depuração
if (app.Environment.IsDevelopment())
{
    var wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
    Console.WriteLine($"Verificando arquivos em: {wwwrootPath}");

    if (Directory.Exists(wwwrootPath))
    {
        var cssPath = Path.Combine(wwwrootPath, "css");
        if (Directory.Exists(cssPath))
        {
            Console.WriteLine("Arquivos CSS encontrados:");
            foreach (var file in Directory.GetFiles(cssPath))
            {
                Console.WriteLine($" - {file}");
            }
        }
        else
        {
            Console.WriteLine("AVISO: Pasta CSS não encontrada!");
        }
    }
    else
    {
        Console.WriteLine("AVISO: Pasta wwwroot não encontrada!");
    }
}

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapControllers();
app.MapRazorPages();

await app.RunAsync();