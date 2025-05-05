using Equilibrium.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.API.Controllers
{
    [ApiController]
    public abstract class BaseController(IUnitOfWork unitOfWork) : ControllerBase
    {
        protected readonly IUnitOfWork _unitOfWork = unitOfWork;
    }
}