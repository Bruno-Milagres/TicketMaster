using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using TicketMaster.IntegrationTests.WebApplicationFactory;

namespace TicketMaster.IntegrationTests.ControllerTests;

public sealed class HomeControllerTests
{
    [Fact]
    public async Task Index_QuandoNaoAutenticado_DeveRetornarRedirectParaLogin()
    {
        // Arrange
        await using var factory = new TicketMasterWebFactory($"HomeCtrl_{Guid.NewGuid()}");
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Index_QuandoAutenticado_DeveRetornarPaginaComEventos()
    {
        // Arrange
        await using var factory = new TicketMasterWebFactory($"HomeCtrl_Auth_{Guid.NewGuid()}");
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(factory);

        // Act
        var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("TicketMaster", body);
    }
}
