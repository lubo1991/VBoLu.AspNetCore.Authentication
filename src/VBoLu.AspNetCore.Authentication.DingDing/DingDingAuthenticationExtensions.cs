using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using VBoLu.IdentityServer.Extensions.DingDing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DingDingAuthenticationExtensions
    {
        public static AuthenticationBuilder AddDingDing( this AuthenticationBuilder builder)
        {
            return builder.AddDingDing("DingDing", delegate
            {
            });
        }

        public static AuthenticationBuilder AddDingDing( this AuthenticationBuilder builder,  Action<DingDingAuthenticationOptions> configuration)
        {
            return builder.AddDingDing("DingDing", configuration);
        }

        public static AuthenticationBuilder AddDingDing( this AuthenticationBuilder builder, string scheme,  Action<DingDingAuthenticationOptions> configuration)
        {
            return builder.AddDingDing(scheme, "钉钉", configuration);
        }

        public static AuthenticationBuilder AddDingDing( this AuthenticationBuilder builder,  string scheme, string caption,  Action<DingDingAuthenticationOptions> configuration)
        {
            return builder.AddOAuth<DingDingAuthenticationOptions, DingDingAuthenticationHandler>(scheme, caption, configuration);
        }
    }
}
