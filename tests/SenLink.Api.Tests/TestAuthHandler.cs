using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SenLink.Api.Tests;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var accountId = Context.Request.Headers["X-Test-AccountId"].ToString();
        var role = Context.Request.Headers["X-Test-Role"].ToString();

        if (string.IsNullOrEmpty(accountId))
        {
            return Task.FromResult(AuthenticateResult.Fail("No account id provided for test."));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, accountId),
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.Role, role ?? "Student")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
