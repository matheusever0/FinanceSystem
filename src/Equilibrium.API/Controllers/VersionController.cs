using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Equilibrium.API.Controllers
{
    public class VersionController(IUnitOfWork unitOfWork) : AuthenticatedController<object>(unitOfWork, null)
    {
        [HttpGet]
        public ActionResult<object> Get()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
            var buildDate = System.IO.File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location).ToString("yyyy-MM-dd HH:mm:ss");

            return Ok(new
            {
                Version = version,
                BuildDate = buildDate,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            });
        }
    }
}