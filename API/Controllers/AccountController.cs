using API.Data.Template;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IMailService _mailService;
        private readonly ICaptchaService _captchaService;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper, IMailService mailService, ICaptchaService captchaService)
        {
            _captchaService = captchaService;
            _mailService = mailService;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (!await _captchaService.VerifyCaptcha(registerDto.Captcha)) return BadRequest("Invalid ReCapcha");
            if (await UserExists(registerDto.Username)) return BadRequest("Username already in use");
            if (await EmailExists(registerDto.Email)) return BadRequest("Email already in use");

            var user = _mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username.ToLower();
            user.Email = registerDto.Email.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            loginDto.Username = loginDto.Username.ToLower();
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            if (user == null) user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.Email == loginDto.Username);

            if (user == null) return BadRequest("Invalid Username or Email");

            if (user.IsBlocked) return Unauthorized("This account has been block. Please contact admin for more information.");

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return BadRequest("Invalid Password");

            if (user.IsDeleted)
            {
                user.IsDeleted = false;
                await _userManager.UpdateAsync(user);
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        private async Task<bool> EmailExists(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email.ToLower());

            if (user == null) return BadRequest("User not found.");

            //if (user.EmailConfirmed == false) return BadRequest("This email is not confirmed yet. Please confirm your email.");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = HttpUtility.UrlEncode(resetToken);
            var encodedEmail = HttpUtility.UrlEncode(user.Email);

            var resetUrl = $"https://{forgotPasswordDto.Hostname}/reset-password?email={encodedEmail}&token={encodedToken}";

            var emailContent = ResetPasswordTemplate.ResetPassword(resetUrl, user.FirstName);

            await _mailService.SendEmailAsync(user.Email, "Reset Password", emailContent);

            return Ok("Password reset link sent to your email.");
        }

        [HttpPut("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email.ToLower());

            if (user == null) return BadRequest("User not found.");

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (result.Succeeded)
            {
                return Ok("Password reset successfully. Please login again.");
            }
            else
            {
                return BadRequest("Problem when resetting password.");
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.GetUserAsync(User);

            var checkPassword = await _userManager.CheckPasswordAsync(user, changePasswordDto.OldPassword);

            if (!checkPassword) return BadRequest("Invalid Password");

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);

            if (result.Succeeded)
            {
                return Ok("Password changed successfully");
            }
            else
            {
                return BadRequest("Problem when changing password.");
            }
        }
    }
}