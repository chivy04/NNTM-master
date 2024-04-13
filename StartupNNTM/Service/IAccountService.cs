using Microsoft.AspNetCore.Identity.Data;
using StartupNNTM.Models;
using StartupNNTM.ViewModels;
using System.Security.Claims;

namespace StartupNNTM.Service
{
    public interface IAccountService
    {
        Task<ApiResult<string>> Authenticate(LoginRequest request);
        Task<ApiResult<string>> Register(RegisterRequest request);
        Task<ApiResult<string>> EmailConfirm(string numberConfirm, string email);
        ClaimsPrincipal ValidateToken(string jwtToken);
        Task<ApiResult<LoginRequest>> ForgetPassword(string email);
        Task<ApiResult<string>> GetCode(string email);
        Task<ApiResult<ResetPassDto>> ConfirmCode(string email);
        Task<ApiResult<string>> ResetPassword(ResetPassDto resetPass);
        Task LockAccount(User user);
        Task<ApiResult<string>> ChangePassword(ChangePasswordDto changePasswodDto);
        Task<ApiResult<string>> ChangeEmail(string currentEmail, string email);
    }
}
