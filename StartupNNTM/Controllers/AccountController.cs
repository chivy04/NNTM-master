using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using StartupNNTM.Service;
using StartupNNTM.ViewModels;

namespace StartupNNTM.Controllers
{
    [ApiController]
    [Route("")]
    public class AccountController : Controller
    {
        private readonly IAccountService _account;
        private readonly IDistributedCache _cache;
        public AccountController(IAccountService account, IDistributedCache cache)
        {
            _account = account;
            _cache = cache;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {
            var result = await _account.Authenticate(request);
            if (!result.IsSuccessed)
            {
                return Ok(result);
            }
            var userPrincipal = _account.ValidateToken(result.ResultObj);
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(15),
                IsPersistent = true
            };

            HttpContext.Session.SetString(SystemConstants.Token, result.ResultObj);
            await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        userPrincipal,
                        authProperties);

            var cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12)
            };
            await _cache.SetStringAsync("my_token_key", SystemConstants.Token, cacheOptions);
            return Ok(result);
        }
        [HttpGet("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Remove("Token");
                return Ok(new { message = "Đăng xuất thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost("SignUp")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
        {
            var result = await _account.Register(request);
            return result.IsSuccessed ? Ok(result) : BadRequest(result);
        }

        [HttpPost("GetCode")]
        public async Task<IActionResult> GetCode(string email)
        {
            var result = await _account.GetCode(email);
            return result.IsSuccessed ? Ok(result) : BadRequest(result);
        }

        [HttpGet("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword([FromQuery] string email)
        {
            var result = await _account.ForgetPassword(email);
            return Ok(result);
        }

        [HttpGet("ForgetPassword/ConfirmCode")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmCode([FromQuery] string email)
        {
            var result = await _account.ConfirmCode(email);
            return result.IsSuccessed ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPassDto resetPass)
        {
            var result = await _account.ResetPassword(resetPass);
            return result.IsSuccessed ? Ok(result) : BadRequest(result);

        }

        [HttpPost("ChangeEmail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmail(string email)
        {
            //// var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var result = await _account.ChangeEmail(User.Identity.Name, email);
            return result.IsSuccessed ? Ok(result) : BadRequest(result);
        }

        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChanggPassword(ChangePasswordDto changePasswodDto)
        {
            var result = await _account.ChangePassword(changePasswodDto);
            return result.IsSuccessed ? Ok(result) : BadRequest(result);
        }
    }
}
