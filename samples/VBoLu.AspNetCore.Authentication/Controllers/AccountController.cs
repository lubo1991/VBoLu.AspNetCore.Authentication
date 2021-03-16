using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace VBoLu.AspNetCore.Authentication.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ExternalLogin(string provider)
        {
            string callbackUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { provider }, Request.Scheme);


            return new ChallengeResult(provider, new AuthenticationProperties()
            {
                RedirectUri = callbackUrl
            });
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string provider)
        {
            if (string.IsNullOrEmpty(provider)) return BadRequest();

            var result = await HttpContext.AuthenticateAsync(provider);

            if (!result.Succeeded)
            {
                return Content("登录失败");
            }
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                _logger.LogDebug("External claims: {@claims}", externalClaims);
            }
            string unionid = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            string name = result.Principal.FindFirstValue(ClaimTypes.Name);


            return Content($"name:{name},unionid:{unionid}");
        }
    }
}
