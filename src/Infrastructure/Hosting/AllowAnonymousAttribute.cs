using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Hosting
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowAnonymousAttribute : Attribute, IAllowAnonymous
    {
    }
}
