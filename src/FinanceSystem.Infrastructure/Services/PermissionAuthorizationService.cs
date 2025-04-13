using FinanceSystem.Domain.Interfaces.Services;
using System.Security.Claims;

namespace FinanceSystem.Infrastructure.Services
{
    public class PermissionAuthorizationService : IPermissionAuthorizationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionAuthorizationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permissionSystemName)
        {
            // Verificar se o usuário está autenticado
            if (!user.Identity.IsAuthenticated)
                return false;

            // Admin tem todas as permissões
            if (user.IsInRole("Admin"))
                return true;

            // Obter o ID do usuário
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                return false;

            // Obter as roles do usuário
            var userRoles = user.FindAll(ClaimTypes.Role)?.Select(c => c.Value).ToList();
            if (userRoles == null || !userRoles.Any())
                return false;

            // Obter as roles do banco de dados
            var roles = await _unitOfWork.Roles.FindAsync(r => userRoles.Contains(r.Name));
            if (roles == null || !roles.Any())
                return false;

            // Verificar se alguma das roles tem a permissão
            foreach (var role in roles)
            {
                var roleWithPermissions = await _unitOfWork.Roles.GetRoleWithPermissionsAsync(role.Id);
                if (roleWithPermissions != null && roleWithPermissions.HasPermission(permissionSystemName))
                    return true;
            }

            return false;
        }
    }
}