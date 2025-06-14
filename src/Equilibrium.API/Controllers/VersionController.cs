using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Equilibrium.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class VersionController(IUnitOfWork unitOfWork) : BaseController(unitOfWork)
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