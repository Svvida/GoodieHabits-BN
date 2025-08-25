using Api.Helpers;
using Application.Accounts.Commands.ChangePassword;
using Application.Accounts.Commands.DeleteAccount;
using Application.Accounts.Commands.RequestPasswordReset;
using Application.Accounts.Commands.ResetPassword;
using Application.Accounts.Commands.UpdateAccount;
using Application.Accounts.Commands.UploadAvatar;
using Application.Accounts.Commands.VerifyPasswordResetCode;
using Application.Accounts.Commands.WipeoutData;
using Application.Accounts.Queries.GetWithProfile;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class AccountController(ISender sender, IMapper mapper) : ControllerBase
    {
        [HttpGet("accounts/me")]
        public async Task<ActionResult<GetAccountWithProfileResponse>> GetAccount(CancellationToken cancellationToken = default)
        {
            var query = new GetAccountWithProfileQuery(JwtHelpers.GetCurrentUserId(User));
            var account = await sender.Send(query, cancellationToken);
            return Ok(account);
        }

        [HttpPut("accounts/me")]
        public async Task<IActionResult> UpdateAccount(
            UpdateAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<UpdateAccountCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("accounts/me/avatar")]
        public async Task<ActionResult<UploadAvatarResponse>> UploadAvatar(
            IFormFile avatarFile, CancellationToken cancellationToken = default)
        {
            var validationResult = ImageValidator.Validate(avatarFile);

            if (validationResult != Domain.Enums.ImageValidationResult.Valid)
            {
                return validationResult switch
                {
                    Domain.Enums.ImageValidationResult.IsEmpty => BadRequest(new { Message = "No file uploaded." }),
                    Domain.Enums.ImageValidationResult.IsTooLarge => BadRequest(new { Message = "The uploaded file exceeds the maximum allowed size of 5 MB." }),
                    Domain.Enums.ImageValidationResult.InvalidDimensions => BadRequest(new { Message = $"Image dimensions cannot exceed {ImageValidator.MaxDimension} pixels." }),
                    Domain.Enums.ImageValidationResult.UnsupportedFormat => BadRequest(new { Message = "Invalid or unsupported image format. Only JPEG, PNG, and WEBP are allowed." }),
                    _ => BadRequest(new { Message = "Invalid file." })
                };
            }

            var command = new UploadAvatarCommand(
                AccountId: JwtHelpers.GetCurrentUserId(User),
                FileStream: avatarFile.OpenReadStream(),
                FileName: avatarFile.FileName);

            var response = await sender.Send(command, cancellationToken);

            return Ok(response);
        }

        [HttpPut("accounts/me/password")]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<ChangePasswordCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("accounts/me")]
        public async Task<IActionResult> DeleteAccount(
            DeleteAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<DeleteAccountCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("accounts/me/wipeout-data")]
        public async Task<IActionResult> WipeoutAccountData(
            WipeoutDataRequest request,
            CancellationToken cancellationToken = default)
        {
            var command = mapper.Map<WipeoutDataCommand>(request) with { AccountId = JwtHelpers.GetCurrentUserId(User) };
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("accounts/forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
            RequestPasswordResetCommand command,
            CancellationToken cancellationToken = default)
        {
            await sender.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("accounts/verify-reset-code")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyResetCode(VerifyPasswordResetCodeCommand command, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(command, cancellationToken);
            if (!result)
                return BadRequest(new { Message = "Invalid email or reset code.", StatusCode = 400 });
            return NoContent();
        }

        [HttpPost("accounts/reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken = default)
        {
            await sender.Send(command, cancellationToken);
            return NoContent();
        }
    }
}