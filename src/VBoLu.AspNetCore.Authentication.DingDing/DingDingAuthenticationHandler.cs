using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace VBoLu.IdentityServer.Extensions.DingDing
{
    public class DingDingAuthenticationHandler : OAuthHandler<DingDingAuthenticationOptions>
    {
        public DingDingAuthenticationHandler( IOptionsMonitor<DingDingAuthenticationOptions> options,  ILoggerFactory logger,  UrlEncoder encoder,  ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync( ClaimsIdentity identity,  AuthenticationProperties properties, OAuthTokenResponse tokens)
        {


            var userInfo = await GetUserInfoByCode(base.Context.Request.Query["code"]);
            if (userInfo == null)
            {
                throw new HttpRequestException($"未能检索钉钉的用户信息,请检查参数是否正确。");
            }
            string content = userInfo.RootElement.GetString("user_info");
          //  base.Logger.LogInformation("token：" + tokens.AccessToken);

            //JObject payload =  JObject.Parse(content);
            //payload.Add("unionid",userInfo.RootElement.GetString("unionid"));
            JsonDocument jsonDocument = JsonDocument.Parse(content);
            base.Logger.LogInformation("用户信息：" + jsonDocument.RootElement);
            #region 获取用户详细信息，暂时不用(只能获取内部员工)
            if (base.Options.IsEmployee) 
            {
                string uninoid = jsonDocument.RootElement.GetString("unionid");
                string userid = await GetUserId(uninoid, tokens.AccessToken);
                if (userid == null)
                {
                    throw new HttpRequestException($"未能检索钉钉的用户id信息,请检查参数是否正确。");
                }
                 jsonDocument = await GetUserInfoById(userid, tokens.AccessToken);
                base.Logger.LogInformation("用户信息：" + jsonDocument.RootElement);
            }

            #endregion


            OAuthCreatingTicketContext context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, base.Context, base.Scheme, (OAuthOptions)base.Options, base.Backchannel, tokens, jsonDocument.RootElement);
            context.RunClaimActions();
            await base.Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal, context.Properties, base.Scheme.Name);
        }

        protected override async Task<OAuthTokenResponse> ExchangeCodeAsync( OAuthCodeExchangeContext context)
        {
            if (base.Options.IsEmployee)
            {
                Exception ex = new Exception("换取access_token失败，content：");
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                {
                    ["appkey"] = base.Options.ClientId,
                    ["appsecret"] = base.Options.ClientSecret,

                };
                string endpoint = QueryHelpers.AddQueryString(base.Options.TokenEndpoint, dictionary);
                HttpResponseMessage response = await base.Backchannel.GetAsync(endpoint, base.Context.RequestAborted);

                string text = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    JsonDocument jsonDocument = JsonDocument.Parse(text);
                    if (jsonDocument.RootElement.GetString("errcode") != "0")
                    {
                        ex = new Exception("换取access_token失败，content：" + text);
                        base.Logger.LogError(ex, "DingDingHandler ExchangeCodeAsync");
                        return OAuthTokenResponse.Failed(ex);
                    }
                    return OAuthTokenResponse.Success(jsonDocument);
                }


                return OAuthTokenResponse.Failed(ex);
            }
            //如果不获取内部员工无需配置获取token
            Dictionary<string, string> dictionarytoken = new Dictionary<string, string>
            {
                ["access_token"] = "abcd",

            };
            return OAuthTokenResponse.Success( JsonDocument.Parse(JsonSerializer.Serialize(dictionarytoken)));
           

        }

        protected override string BuildChallengeUrl( AuthenticationProperties properties, [NotNull] string redirectUri)
        {
            string value = base.Options.StateDataFormat.Protect(properties);
            Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                ["appid"] = base.Options.AppID,
                ["scope"] = FormatScope(),
                ["response_type"] = "code",
                ["redirect_uri"] = redirectUri,
                ["state"] = value
            };
            string parameter = properties.GetParameter<string>("loginTmpCode");
            if (!string.IsNullOrWhiteSpace(parameter))
            {
                dictionary.Add("loginTmpCode", parameter);
            }

            redirectUri = QueryHelpers.AddQueryString(base.Options.AuthorizationEndpoint, dictionary);
            return redirectUri;
        }

        protected override string FormatScope()
        {
            return string.Join(",", base.Options.Scope);
        }
        /// <summary>
        /// 根据code获取用户信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private async Task<JsonDocument> GetUserInfoByCode(string code)
        {
            DateTimeOffset dto = new DateTimeOffset(DateTime.Now);
            var timestamp = dto.ToUnixTimeMilliseconds().ToString();

            Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                ["accessKey"] = base.Options.AppID,
                ["timestamp"] = timestamp,
                ["signature"] = EncryptWithSHA256(base.Options.AppSecret, timestamp),
            };
            Dictionary<string, string> content = new Dictionary<string, string>
            {
                ["tmp_auth_code"] = code,

            };
            string json = JsonSerializer.Serialize(content);
            StringContent stringContent = new StringContent(json);
            var requestUri = QueryHelpers.AddQueryString(base.Options.UserInformationByCodeEndpoint, dictionary);

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequestMessage.Content = stringContent;
            HttpResponseMessage response = await base.Backchannel.SendAsync(httpRequestMessage, base.Context.RequestAborted);
            string text = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                JsonDocument jsonDocument = JsonDocument.Parse(text);
                if (jsonDocument.RootElement.GetString("errcode") != "0")
                {
                    Exception ex = new Exception("使用code获取用户信息失败，content：" + text);
                    base.Logger.LogError(ex, "DingDingHandler ExchangeCodeAsync");
                    return null;
                }

                return jsonDocument;
            }
            return null;
        }
        /// <summary>
        /// 根据unionid获取userid
        /// </summary>
        /// <param name="unionid"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        private async Task<string> GetUserId(string unionid, string access_token)
        {
            DateTimeOffset dto = new DateTimeOffset(DateTime.Now);
            var timestamp = dto.ToUnixTimeMilliseconds().ToString();

            Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                ["access_token"] = access_token,

            };
            Dictionary<string, string> content = new Dictionary<string, string>
            {
                ["unionid"] = unionid,

            };
            string json = JsonSerializer.Serialize(content);
            StringContent stringContent = new StringContent(json);
            var requestUri = QueryHelpers.AddQueryString(base.Options.UserIdByUnionidEndpoint, dictionary);

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequestMessage.Content = stringContent;
            HttpResponseMessage response = await base.Backchannel.SendAsync(httpRequestMessage, base.Context.RequestAborted);
            string text = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                JsonDocument jsonDocument = JsonDocument.Parse(text);
                if (jsonDocument.RootElement.GetString("errcode") != "0")
                {
                    Exception ex = new Exception("使用unionid获取userid失败，content：" + text);
                    base.Logger.LogError(ex, "DingDingHandler GetUserId");
                    return null;
                }
                string result = jsonDocument.RootElement.GetString("result");

                JsonDocument resultDocument = JsonDocument.Parse(result);
                string userid = resultDocument.RootElement.GetString("userid");
                return userid;
            }
            else
            {
                Exception ex = new Exception("使用unionid获取userid失败，content：" + text);
                base.Logger.LogError(ex, "DingDingHandler GetUserId");
            }
            return null;
        }
        /// <summary>
        /// 更具userid或者用户信息
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        private async Task<JsonDocument> GetUserInfoById(string userid, string access_token)
        {
            DateTimeOffset dto = new DateTimeOffset(DateTime.Now);
            var timestamp = dto.ToUnixTimeMilliseconds().ToString();

            Dictionary<string, string> dictionary = new Dictionary<string, string>
            {
                ["access_token"] = access_token,

            };
            Dictionary<string, string> content = new Dictionary<string, string>
            {
                ["userid"] = userid,

            };
            string json = JsonSerializer.Serialize(content);
            StringContent stringContent = new StringContent(json);
            var requestUri = QueryHelpers.AddQueryString(base.Options.UserInformationEndpoint, dictionary);

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpRequestMessage.Content = stringContent;
            HttpResponseMessage response = await base.Backchannel.SendAsync(httpRequestMessage, base.Context.RequestAborted);
            string text = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                JsonDocument jsonDocument = JsonDocument.Parse(text);
                if (jsonDocument.RootElement.GetString("errcode") != "0")
                {
                    Exception ex = new Exception("使用userid获取用户信息失败，content：" + text);
                    base.Logger.LogError(ex, "DingDingHandler GetUserInfoById");
                    return null;
                }
                var result = jsonDocument.RootElement.GetString("result");

                JsonDocument resultDocument = JsonDocument.Parse(result);
                return resultDocument;
            }
            return null;
        }
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="accessKey"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        protected virtual string EncryptWithSHA256(string accessKey, string timestamp)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(accessKey);
            byte[] bytes2 = Encoding.UTF8.GetBytes(timestamp);
            using (HMACSHA256 hMACSHA = new HMACSHA256(bytes))
            {
                byte[] inArray = hMACSHA.ComputeHash(bytes2);
                return Convert.ToBase64String(inArray);
            }
        }
    }
}
