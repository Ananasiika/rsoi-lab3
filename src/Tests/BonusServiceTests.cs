using BonusService.Dto;
using BonusService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using PrivilegeController = BonusService.Controllers.PrivilegeController;

namespace Tests;

public class BonusServiceTests
{
    private readonly Mock<IPrivilegeService> _mockPrivilegeService;
    private readonly PrivilegeController _privilegeController;

    public BonusServiceTests()
    {
        _mockPrivilegeService = new Mock<IPrivilegeService>();
        _privilegeController = new PrivilegeController(_mockPrivilegeService.Object);
    }

    [Fact]
    public async Task GetPrivilegeInfo_MissingUsername_ReturnsBadRequest()
    {
        // Act
        var response = (ObjectResult)await _privilegeController.GetPrivilegeInfo("");

        // Assert
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPurchase_ValidRequest_ReturnsOk()
    {
        // Arrange
        var username = "testuser";
        var request = new PurchaseUpdateRequest
        {
            TicketUid = Guid.NewGuid(),
            Price = 5000,
            PaidFromBalance = true
        };

        _mockPrivilegeService.Setup(x => x.ProcessPurchaseAsync(username, request))
            .Returns(Task.CompletedTask);

        // Act
        var response = (OkResult)await _privilegeController.UpdateAfterPurchase(username, request);

        // Assert
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPurchase_MissingUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new PurchaseUpdateRequest
        {
            TicketUid = Guid.NewGuid(),
            Price = 5000,
            PaidFromBalance = true
        };

        // Act
        var response = (ObjectResult)await _privilegeController.UpdateAfterPurchase("", request);

        // Assert
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task ProcessCancel_ValidRequest_ReturnsOk()
    {
        // Arrange
        var username = "testuser";
        var request = new CancelUpdateRequest
        {
            TicketUid = Guid.NewGuid()
        };

        _mockPrivilegeService.Setup(x => x.ProcessCancelAsync(username, request))
            .Returns(Task.CompletedTask);

        // Act
        var response = (OkResult)await _privilegeController.UpdateAfterCancel(username, request);

        // Assert
        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task ProcessCancel_MissingUsername_ReturnsBadRequest()
    {
        // Arrange
        var request = new CancelUpdateRequest
        {
            TicketUid = Guid.NewGuid()
        };

        // Act
        var response = (ObjectResult)await _privilegeController.UpdateAfterCancel("", request);

        // Assert
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task ProcessPurchase_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var username = "testuser";
        var request = new PurchaseUpdateRequest
        {
            TicketUid = Guid.NewGuid(),
            Price = 5000,
            PaidFromBalance = true
        };

        _mockPrivilegeService.Setup(x => x.ProcessPurchaseAsync(username, request))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var response = (ObjectResult)await _privilegeController.UpdateAfterPurchase(username, request);

        // Assert
        Assert.Equal(500, response.StatusCode);
    }
}