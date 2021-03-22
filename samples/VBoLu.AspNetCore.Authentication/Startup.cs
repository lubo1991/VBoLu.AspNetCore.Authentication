using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VBoLu.AspNetCore.Authentication
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
            services.AddMvc();

            services.AddControllersWithViews();
           
            services.AddAuthentication()
              
                  .AddDingDing("DingDing", o =>
                  {
                      o.ClientId = "";//获取token的应用(可以不用，但不能为空)
                      o.ClientSecret = "";//获取token的秘钥(可以不用，但不能为空)
                      o.AppID = "";//登录的应用
                      o.AppSecret = "";//登录获取用户信息秘钥
                                                                                                       //IdentityServer4 需要使用
                      o.ClaimActions.MapJsonKey(JwtClaimTypes.Subject, "unionid");//将Unionid映射到Subject
                                                                                   //o.ClaimActions.MapJsonKey(JwtClaimTypes.PhoneNumber, "mobile");//企业内部员工才能获取
                      o.IsEmployee = false;//是否为企业内部员工 三方扫码登录请不要配置
                                                                                   //外部登录统一设置为ExternalCookieAuthenticationScheme
                       o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                  }).AddCookie(IdentityServerConstants.ExternalCookieAuthenticationScheme);

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
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                                   name: "default",
                                   pattern: "{controller=Account}/{action=Index}/{id?}");
            });
        }
    }
}
