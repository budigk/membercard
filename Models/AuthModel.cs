namespace MemberCard.Models;

public class LoginRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
public class LoginResponse { public string AccessToken { get; set; } = string.Empty; public Member? Member { get; set; } }
public class RegisterRequest { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; }