using Equilibrium.Domain.Interfaces.Services;

namespace Equilibrium.API.Controllers
{
    public abstract class DefaultController<TService> : BaseController
        where TService : class
    {
        protected readonly TService _service;

        public DefaultController(IUnitOfWork unitOfWork, TService service) : base(unitOfWork)
        {
            _service = service;
        }
    }
}