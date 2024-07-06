using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace IdentityCore;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource()
            {
                Name = "verification",
                UserClaims = new List<string>
                {
                    JwtClaimTypes.Email,
                    JwtClaimTypes.EmailVerified,
                }
            }
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            //new ApiScope(name: "api", displayName: "My API")
            new ApiScope(name: "api1", displayName: "My API")
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client()
            {
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = { "api1" },
            },
            new Client()
            {
                ClientId = "web",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,

                // where to redirect to after login
                RedirectUris = { "https://localhost:5002/signin-oidc" },

                // where to redirect to after logout
                PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes=
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "verification",
                    "api1"
                }
            }
        };
}