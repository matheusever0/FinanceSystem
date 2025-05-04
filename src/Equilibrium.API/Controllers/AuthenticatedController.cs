using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public abstract class AuthenticatedController<TService> : DefaultController<TService>
        where TService : class
    {
        public AuthenticatedController(IUnitOfWork unitOfWork, TService service) : base(unitOfWork, service)
        {
        }
    }
}