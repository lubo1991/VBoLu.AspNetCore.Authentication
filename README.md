# VBoLu.AspNetCore.Authentication
基于identityServer4三方登录
调用方式
 # services.AddAuthentication()
                  .AddDingDing("DingDing", o =>
                  {
                       // var dingding = Configuration.GetSection("dingDingAuthority").Get<DingDingAuthenticationOptions>();
                       o.ClientId = "";//获取token的应用（可以不配但不能为空）
                       o.ClientSecret = "";//获取token的秘钥（可以不配但不能为空）
                       o.AppID = "";//登录的应用
                       o.AppSecret = "";//登录获取用户信息秘钥
                                                                                                        //IdentityServer4 需要使用
                       o.ClaimActions.MapJsonKey(JwtClaimTypes.Subject, "unionid");//将Unionid映射到Subject
                    //o.ClaimActions.MapJsonKey(JwtClaimTypes.PhoneNumber, "mobile");//企业内部员工才能获取
                      o.IsEmployee = false;//是否为企业内部员工 三方扫码登录请不要配置
                      //外部登录统一设置为ExternalCookieAuthenticationScheme
                       o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                  });
