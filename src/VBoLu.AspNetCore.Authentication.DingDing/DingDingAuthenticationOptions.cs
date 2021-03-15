using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VBoLu.IdentityServer.Extensions.DingDing
{
    public class DingDingAuthenticationOptions : OAuthOptions
    {
        public string UserInformationByCodeEndpoint
        {
            get;
            set;
        }

        public string UserIdByUnionidEndpoint
        {
            get;
            set;
        }
        public bool IsEmployee
        {
            get;
            set;
        }
        public string AppID
        {
            get;
            set;
        }
        public string AppSecret
        {
            get;
            set;
        }
        public DingDingAuthenticationOptions()
        {
            base.ClaimsIssuer = "DingDing";
            base.CallbackPath = "/signin-dingding";
            IsEmployee = false;
          //  base.AuthorizationEndpoint = "https://oapi.dingtalk.com/connect/oauth2/sns_authorize";
            base.AuthorizationEndpoint = "https://oapi.dingtalk.com/connect/qrconnect";
            base.TokenEndpoint = "https://oapi.dingtalk.com/gettoken";
            base.UserInformationEndpoint = "https://oapi.dingtalk.com/topapi/v2/user/get";
            UserInformationByCodeEndpoint = "https://oapi.dingtalk.com/sns/getuserinfo_bycode";
            UserIdByUnionidEndpoint = "https://oapi.dingtalk.com/topapi/user/getbyunionid";
            base.Scope.Add("snsapi_login");
            base.ClaimActions.MapJsonKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", "unionid");
            base.ClaimActions.MapJsonKey("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "nick");
            base.ClaimActions.MapJsonKey("urn:dingding:unionid", "unionid");
        }
    }
}
