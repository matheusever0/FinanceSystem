using Equilibrium.Domain.Interfaces.Services;

namespace Equilibrium.API.Controllers
{
    public abstract class DefaultController<TService>(IUnitOfWork unitOfWork, TService service) : BaseController(unitOfWork)
        where TService : class
    {
        protected readonly TService _service = service;
    }
}