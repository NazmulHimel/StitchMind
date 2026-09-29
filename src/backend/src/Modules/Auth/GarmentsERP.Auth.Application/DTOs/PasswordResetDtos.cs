namespace GarmentsERP.Auth.Application.DTOs;

public record ForgotPasswordRequestDto(string Email);

public record VerifyOtpRequestDto(string Email, string Otp);

public record ResetPasswordRequestDto(
    string Email,
    string Otp,
    string NewPassword,
    string ConfirmPassword);
