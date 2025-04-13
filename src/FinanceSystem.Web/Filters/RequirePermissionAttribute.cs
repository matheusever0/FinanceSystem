﻿using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permissionSystemName) : base(typeof(PermissionAuthorizationFilter))
        {
            Arguments = new object[] { permissionSystemName };
        }
    }
}
