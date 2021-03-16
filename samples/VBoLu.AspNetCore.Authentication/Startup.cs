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
                      o.ClientId = "";//��ȡtoken��Ӧ��(���Բ��ã�������Ϊ��)
                      o.ClientSecret = "";//��ȡtoken����Կ(���Բ��ã�������Ϊ��)
                      o.AppID = "";//��¼��Ӧ��
                      o.AppSecret = "";//��¼��ȡ�û���Ϣ��Կ
                                                                                                       //IdentityServer4 ��Ҫʹ��
                      o.ClaimActions.MapJsonKey(JwtClaimTypes.Subject, "unionid");//��Unionidӳ�䵽Subject
                                                                                   //o.ClaimActions.MapJsonKey(JwtClaimTypes.PhoneNumber, "mobile");//��ҵ�ڲ�Ա�����ܻ�ȡ
                      o.IsEmployee = false;//�Ƿ�Ϊ��ҵ�ڲ�Ա�� ����ɨ���¼�벻Ҫ����
                                                                                   //�ⲿ��¼ͳһ����ΪExternalCookieAuthenticationScheme
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
