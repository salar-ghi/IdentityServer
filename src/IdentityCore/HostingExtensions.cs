using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;



using Duende.IdentityServer;
using Microsoft.IdentityModel.Tokens;



namespace IdentityCore;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        var migrationsAssembly = typeof(Program).Assembly.GetName().Name;
        //const string connectionString = @"Data Source=Duende.IdentityServer.Quickstart.EntityFramework.db";
        const string connectionString = @"Server=.;Database=Identity;User ID=sa;Password=Nitro912*;MultipleActiveResultSets=true;TrustServerCertificate=True";

        builder.Services.AddIdentityServer(options =>
            {
                //https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            })
            .AddOperationalStore(options =>
            {
                options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                    sql => sql.MigrationsAssembly(migrationsAssembly));
            });

            //.AddInMemoryIdentityResources(Config.IdentityResources)
            //.AddInMemoryApiScopes(Config.ApiScopes)
            //.AddInMemoryClients(Config.Clients)
            //.AddTestUsers(TestUsers.Users);

        var authenticationBuilder = builder.Services.AddAuthentication();

        var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
        var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        if (googleClientId != null && googleClientSecret != null) 
        {
            //authenticationBuilder.AddGoogle("Google", options =>
            //{
            //options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //options.ClientId = googleClientId;
            //options.ClientSecret = googleClientSecret;
            //});

        }
        //builder.Services.AddAuthentication(options =>
        //{
        //    options.DefaultScheme = "Cookies";
        //    options.DefaultChallengeScheme = "oidc";
        //})
        authenticationBuilder
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                options.SaveTokens = true;

                //options.ClientId = "web";

                options.Authority = "https://localhost:5001";
                options.ClientSecret = "secret";
                options.ResponseType = "code";
                //options.Scope.Clear();
                //options.Scope.Add("openid");
                //options.Scope.Add("profile");
                //options.Scope.Add("api1");
                //options.Scope.Add("offline_access");
                //options.Scope.Add("verification");
                //options.ClaimActions.MapJsonKey("email_verified", "email_verified");
                //options.GetClaimsFromUserInfoEndpoint = true;
                //options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

            });
        //.AddOpenIdConnect("oidc", "Demo IdentityServer", options =>
        //{
        //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        //    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

        //    options.Authority = "https://demo.duendesoftware.com";
        //    options.ClientId = "interactive.confidential";
        //    options.ResponseType = "code";

        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        NameClaimType = "name",
        //        RoleClaimType = "role"
        //    };
        //});
        //    .AddGoogle("Google", options =>
        //    {
        //        options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
        //        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        //        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        //    });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        InitializeDatabase(app);
        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }

    private static void InitializeDatabase(IApplicationBuilder app)
    {
        using(var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope())
        {
            serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            context.Database.Migrate();
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            if (!context.IdentityResources.Any())
            {
                foreach(var resource in Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            if (context.ApiScopes.Any())
            {
                foreach (var resource in Config.ApiScopes)
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}
