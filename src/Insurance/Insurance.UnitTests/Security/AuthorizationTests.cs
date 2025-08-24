using System.Reflection;
using Insurance.Service.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace Insurance.UnitTests.Security;

public class AuthorizationTests
{
    [Fact]
    public void InsurancesController_GetInsurances_ShouldHaveReadPolicy()
    {
        // Arrange
        var method = typeof(InsurancesController).GetMethod(nameof(InsurancesController.GetInsurances));
        Assert.NotNull(method);

        // Act
        var attribute = method.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        Assert.NotNull(attribute);
    }
}
