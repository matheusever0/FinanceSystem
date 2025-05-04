using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly IUnitOfWork _unitOfWork;

        public BaseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
    }
}