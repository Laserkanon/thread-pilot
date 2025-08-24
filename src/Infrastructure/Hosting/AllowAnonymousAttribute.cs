using System;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Hosting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AllowAnonymousAttribute : Attribute, IAllowAnonymous
    {
    }
}
