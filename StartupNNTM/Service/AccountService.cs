using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using StartupNNTM.Models;
using StartupNNTM.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StartupNNTM.Service
{
    public class AccountService : IAccountService
    {
        public readonly ISendMailService _sendmailservice;
        //public readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        private readonly NntmContext _dataContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;


        public AccountService(NntmContext context,
                UserManager<User> userManager,
                SignInManager<User> signInManager,
                IConfiguration configuration,
                ISendMailService sendmailservice
            )
        {
            _dataContext = context;
            //_logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = configuration;
            _sendmailservice = sendmailservice;
        }
        public async Task<ApiResult<string>> Authenticate(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            //if (emailGetCode != null && emailGetCode.IsDeleted)
            //{
            //    emailGetCode = null;
            //}

            var errorMessages = new Dictionary<Func<bool>, string>
            {
                { () => user == null, "Tài khoản không tồn tại" },
                { () => user.LockoutEnabled && user.AccessFailedCount == -1, "Tài khoản bị khóa vĩnh viễn" },
                { () => !user.LockoutEnabled, "Tài khoản bị khóa" },
                { () => true, "ok" },
            };
            var errorMessage = errorMessages.First(kv => kv.Key()).Value;
            if (!errorMessage.Equals("ok"))
            {
                return new ApiErrorResult<string>(errorMessage);
            }

            var accessFailedCount = user.AccessFailedCount;
            var result = await _signInManager.PasswordSignInAsync(user, request.Password, true, true);

            if (!result.Succeeded)
            {
                //if (accessFailedCount is 4)
                //{
                //    await LockAccount(emailGetCode);
                //    return new ApiErrorResult<string>("Bạn đã nhập sai mật khẩu liên tục 5 lần! Tài khoản của bạn đã bị khóa. Để lấy lại tài khoản vui lòng thực hiện quên mật khẩu");
                //}
                return new ApiErrorResult<string>("Sai mật khẩu");
            }
            return new ApiSuccessResult<string>(await GetToken(user));
        }

        public async Task<string> GetToken(User user)
        {
            Encoding encoding = Encoding.UTF8;
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role, string.Join(";",roles)),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Surname, user.Fullname),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Tokens:Issuer"],
                _config["Tokens:Issuer"],
                claims,
                expires: DateTime.Now.AddDays(15),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // Confirm Code to Reset Password
        public async Task<ApiResult<ResetPassDto>> ConfirmCode(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = new ResetPassDto()
            {
                Email = email,
                Token = token
            };
            return new ApiSuccessResult<ResetPassDto>(result);
        }

        public async Task<ApiResult<string>> EmailConfirm(string numberConfirm, string email)
        {
            var emailGetCode = await _dataContext.EmailGetCodes.Where(email => email.EmailName.Equals(email)).FirstOrDefaultAsync();
            if (emailGetCode is null) return new ApiErrorResult<string>("Lỗi xác nhận email");
            if (emailGetCode.Code.Equals(numberConfirm))
            {
                return new ApiSuccessResult<string>();
            }
            return new ApiErrorResult<string>("Mã code không chính xác");
        }

        public async Task<ApiResult<LoginRequest>> ForgetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return new ApiErrorResult<LoginRequest>("Email chưa được đăng ký tài khoản");
            }
            if (user.AccessFailedCount is -1)
            {
                return new ApiErrorResult<LoginRequest>("Tài khoản bị khóa vĩnh viễn");
            }

            var confirmNumber = GetConfirmCode();
            await SaveCodeConfirm(confirmNumber, email);


            await SendConfirmCodeToEmail(user.Email, confirmNumber);

            var result = new LoginRequest()
            {
                Email = email,
                Password = confirmNumber
            };

            return new ApiSuccessResult<LoginRequest>(result);
        }
        public async Task<ApiResult<string>> Register(RegisterRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null) return new ApiErrorResult<string>("Email đã tồn tại");

                user = new User()
                {
                    UserName = request.Email,
                    Fullname = request.Email,
                    Email = request.Email,
                    EmailConfirmed = true,
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    var role = "user";
                    var getUser = await _userManager.FindByEmailAsync(request.Email);
                    if (getUser != null)
                    {
                        await _userManager.AddToRoleAsync(getUser, role);
                        return new ApiSuccessResult<string>();
                    }
                }

                return new ApiErrorResult<string>("Đăng ký không thành công : Mật khẩu không hợp lệ, yêu cầu gồm có ít 6 ký tự bao gồm ký tự: Hoa, thường, số, ký tự đặc biệt ");
            }
            catch (Exception ex)
            {
              //  _logger.LogError("Xảy ra lỗi trong quá trình đăng ký | ", ex.Message);
                return new ApiErrorResult<string>("Lỗi đăng ký! " + ex.Message);
            }
        }

        private async Task SendConfirmCodeToEmail(string email, string confirmNumber)
        {
            MailContent content = new()
            {
                To = email,
                Subject = "Yêu cầu xác nhận email từ [Nông nghiệp thông minh & thực phẩm sạch]",
                Body = $@"<!DOCTYPE html>
                <html lang=""en"">
                <head>
                  <meta charset=""UTF-8"">
                  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                  <style>
                    :root {{
                      --panel-color: rgba(0, 0, 0, 0.135);
                      --title-color: rgba(37, 47, 61, 1);
                      --panel-border-width: 0.1em;
                      --panel-padding: 0.75em;
                    }}

                    .panel {{
                      background: var(--panel-color);
                      border-radius: var(--panel-border-width);
                      padding: var(--panel-border-width);
                    }}
                    .panel__header {{
                        background: var(--title-color);
                    }}
                    .panel__header, 
                    .panel__content {{
                      padding: var(--panel-padding);
                    }}

                    .panel__title {{
                      line-height: 1;
                      font-family: Montserrat;
                    }}

                    .panel__content {{
                      padding: 12px 22px;
                      background: #fff;
                    }}

                    .example {{
                      display: flex;
                      flex-grow: 1;
                      padding: 1em max(1em, calc(50vw - 60ch));
                      place-items: center;
                    }}

                    .example > * {{
                      flex-grow: 1;
                    }}

                    .bg-1 {{
                      background-image: linear-gradient( 135deg, #81FBB8 10%, #28C76F 100%);
                    }}
                  </style>
                </head>
                <body>
                  <div class=""example bg-1"">
                    <section class=""panel"">
                      <header class=""panel__header"">
                        <h1 class=""panel__title"" style=""text-align: center"">
                          Nông nghiệp thông minh & thực phẩm sạch
                        </h1>
                      </header>
                      <div class=""panel__content"" style=""font-family: Montserrat; font-size: 14px;"">
                        Xin chào! Chúng tôi đã nhận yêu cầu xác thực tài khoản web <strong>Tên website</strong> của bạn. Mã dùng một lần của bạn là:
                        <br>
                        <div style=""font-size: 4em; text-align: center"">
                          {confirmNumber}
                        </div>
                        <hr>
                        <div style=""padding-top: 12px; text-align: center; font-family: Montserrat"">
                        Chúng tôi sẽ không bao giờ gửi email cho bạn và yêu cầu bạn tiết lộ hoặc xác minh mật khẩu, thẻ tín dụng hoặc số tài khoản ngân hàng của bạn.
                        </div>
                        </div>

                        <div style=""padding: 12px 0px; text-align: center; font-family: Montserrat; font-size: 10px"">
                        Thông báo này được tạo và phân phối bởi ***, Inc.., *** là thương hiệu đã đăng ký của ***, Inc. Xem chính sách quyền riêng tư của chúng tôi.
                        </div>
                    </section>
                  </div>
                </body>
                </html>"
            };


            await _sendmailservice.SendMail(content);
        }

        private string GetConfirmCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public ClaimsPrincipal ValidateToken(string jwtToken)
        {
            IdentityModelEventSource.ShowPII = true;
            TokenValidationParameters validationParameters = new()
            {
                ValidateLifetime = true,
                ValidAudience = _config["Tokens:Issuer"],
                ValidIssuer = _config["Tokens:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]))
            };

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);

            return principal;
        }

        public async Task<ApiResult<string>> ResetPassword(ResetPassDto resetPass)
        {
            var user = await _userManager.FindByEmailAsync(resetPass.Email);
            if (user is null) return new ApiErrorResult<string>("Lỗi");
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, resetPass.Token, resetPass.Password);
            if (!resetPasswordResult.Succeeded)
            {
              //  _logger.LogError("Xảy ra lỗi trong quá trình xử lý | ", resetPasswordResult.Errors.Select(e => e.Description));
                return new ApiErrorResult<string>("Lỗi! "+ resetPasswordResult.Errors.Select(e => e.Description));
            }
            if (user.AccessFailedCount is 5)
            {
                user.LockoutEnabled = false;
                user.AccessFailedCount = 0;
                _dataContext.User.Update(user);
                await _dataContext.SaveChangesAsync();
            }
            return new ApiSuccessResult<string>();
        }

        public async Task LockAccount(User user)
        {
            user.LockoutEnabled = true;
            user.AccessFailedCount = 5;
            _dataContext.User.Update(user);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<ApiResult<string>> ChangePassword(ChangePasswordDto changePasswodDto)
        {
            var user = await _userManager.FindByEmailAsync(changePasswodDto.Email);
            var passwordCheckResult = await _userManager.CheckPasswordAsync(user, changePasswodDto.Password);
            if (!passwordCheckResult)
            {
                return new ApiErrorResult<string>("Mật khẩu hiện tại không đúng");
            }
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswodDto.Password, changePasswodDto.NewPassword);
            if (changePasswordResult.Succeeded)
            {
                return new ApiSuccessResult<string>();
            }
            return new ApiErrorResult<string>("Đổi mật khẩu không thành công");
        }


        public async Task<ApiResult<string>> ChangeEmail(string currentEmail, string email)
        {
            var user = await _userManager.FindByEmailAsync(currentEmail);
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, email);
            user.UserName = email;
            user.NormalizedEmail = email;
            user.NormalizedUserName = email;
            var changeEmailResult = await _userManager.ChangeEmailAsync(user, email, token);
            await SendConfirmCodeToEmail(email, GetConfirmCode());
            if (changeEmailResult.Succeeded)
            {
                return new ApiSuccessResult<string>(await GetToken(user));
            }
            return new ApiErrorResult<string>("Đổi email không thành công");
        }

        public async Task<ApiResult<string>> GetCode(string email)
        {
            var code = new EmailGetCode()
            {
                Id = Guid.NewGuid(),
                Code = GetConfirmCode(),
                EmailName = email,
                CreatedAt = DateTime.Now.AddMinutes(10)
            };

            await SendConfirmCodeToEmail(email, code.Code);
            await _dataContext.EmailGetCodes.AddAsync(code);
            await _dataContext.SaveChangesAsync();

            return new ApiSuccessResult<string>(code.Code);
        }

        private async Task SaveCodeConfirm(string confirmNumber, string email)
        {
            var code = await _dataContext.EmailGetCodes.Where(email => email.EmailName.Equals(email)).FirstOrDefaultAsync();
            code.Code = confirmNumber;
            code.CreatedAt = DateTime.Now.AddMinutes(10);

            _dataContext.EmailGetCodes.Update(code);
            await _dataContext.SaveChangesAsync();
        }
    }
}
