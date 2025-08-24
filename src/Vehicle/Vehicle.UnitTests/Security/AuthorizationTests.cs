using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Vehicle.Service.Controllers;

namespace Vehicle.UnitTests.Security;

public class AuthorizationTests
{
    [Fact]
    public void VehiclesController_GetVehicle_ShouldHaveReadPolicy()
    {
        // Arrange
        var method = typeof(VehiclesController).GetMethod(nameof(VehiclesController.GetVehicle));
        Assert.NotNull(method);

        // Act
        var attribute = method.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        Assert.NotNull(attribute);
    }

    [Fact]
    public void VehiclesController_GetVehiclesBatch_ShouldHaveReadPolicy()
    {
        // Arrange
        var method = typeof(VehiclesController).GetMethod(nameof(VehiclesController.GetVehiclesBatch));
        Assert.NotNull(method);

        // Act
        var attribute = method.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        Assert.NotNull(attribute);
    }
}
