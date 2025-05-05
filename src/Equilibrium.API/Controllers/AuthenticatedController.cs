using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public abstract class AuthenticatedController<TService>
        (IUnitOfWork unitOfWork, TService service) : DefaultController<TService>(unitOfWork, service)
        where TService : class
    {
    }
}