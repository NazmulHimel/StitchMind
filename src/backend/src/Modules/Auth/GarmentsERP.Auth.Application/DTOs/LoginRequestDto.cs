namespace GarmentsERP.Auth.Application.DTOs;
public record LoginRequestDto(string UserNameOrEmail, string Password, bool RememberMe = false);
