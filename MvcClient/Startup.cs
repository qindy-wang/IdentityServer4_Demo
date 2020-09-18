using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MvcClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            JwtSecurityTokenHandler.DefaultMapInboundClaims=false;

            /*add the authentication services to DI.
             we are using a cookie to locally sign-in the user(via "Cookies" as the default schema)
            , and we set the DefaultChanllegeSchema to OIDC because when we need the user to login, we
            will be using the OpenID Connect protocol.

            we then use AddCookie to add the handler that can process cookies.

            Finally, AddOpenIdConnect is used to configure the handler that performs the OpenId Connnect protocol.
            The Authority indicates where the trusted token service is located, we
            then identify this client via the ClientId and the ClientSecret.
            SaveToken is used to persist the token from IdentityServer in the cookie
            (as they will be needed later)
             */
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            }).AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
             {
                 options.Authority = "https://localhost:6001";
                 options.ClientId = "mvc";
                 options.ClientSecret = "secret";
                 options.ResponseType = "code";

                 //since SaveToken is enabled, ASP.NET Core will automatically store the resulting access and refresh token 
                 //in the authentication session. you should be able to inspect
                 //the data on the page that prints out the contents of the session
                 //that you created earlier.
                 options.SaveTokens = true;

                 options.Scope.Add("api1");
                 options.Scope.Add("offline_access");
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            /*And then ensure the excution of the authentication services on each request,
             * add UseAuthentication to Configure in Startup
             */
            app.UseAuthentication();
            app.UseAuthorization();

            //the RequireAuthentication method disables annoymous acccess for the entire application
            // you can use the [Authorize] attribute, if you want to specify authorization on a per controller of action basic
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}")
                endpoints.MapDefaultControllerRoute().RequireAuthorization();
            });
        }
    }
}
