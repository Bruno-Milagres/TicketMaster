using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TicketMaster.IntegrationTests.WebApplicationFactory;

namespace TicketMaster.IntegrationTests.ControllerTests;

/// <summary>
/// Helper para criar usuários de teste e obter HttpClient autenticado.
/// Faz o fluxo completo: criar usuário → GET login → extrair anti-forgery → POST login.
/// </summary>
internal static class AuthenticationHelper
{
    public const string TestEmail = "teste@ticketmaster.com";
    public const string TestPassword = "Teste@123";

    internal static async Task<HttpClient> CreateAuthenticatedClientAsync(TicketMasterWebFactory factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        // 1. Cria o usuário de teste via UserManager
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = new IdentityUser(TestEmail);
            var result = await userManager.CreateAsync(user, TestPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Falha ao criar usuário: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // 2. GET na página de login para obter o anti-forgery token
        var loginPageResponse = await client.GetAsync("/Identity/Account/Login");
        var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();

        // 3. Extrai o __RequestVerificationToken do HTML
        var tokenMatch = Regex.Match(loginPageHtml,
            @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]+)""",
            RegexOptions.IgnoreCase);

        if (!tokenMatch.Success)
            throw new InvalidOperationException("Não foi possível extrair o anti-forgery token da página de login.");

        var antiForgeryToken = tokenMatch.Groups[1].Value;

        // 4. POST no login com o token + credenciais
        var loginData = new Dictionary<string, string>
        {
            { "Input.Email", TestEmail },
            { "Input.Password", TestPassword },
            { "__RequestVerificationToken", antiForgeryToken }
        };

        var loginResponse = await client.PostAsync("/Identity/Account/Login",
            new FormUrlEncodedContent(loginData));

        // Verifica se o login foi bem-sucedido (redirect esperado)
        if (loginResponse.StatusCode != System.Net.HttpStatusCode.Redirect &&
            loginResponse.StatusCode != System.Net.HttpStatusCode.Found)
        {
            var errorBody = await loginResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Falha no login. Status: {loginResponse.StatusCode}. Body: {errorBody[..Math.Min(500, errorBody.Length)]}");
        }

        return client;
    }
}
