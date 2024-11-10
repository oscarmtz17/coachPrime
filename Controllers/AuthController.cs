using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webapi.Models;
using webapi.Services;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUsuarioService _usuarioService;

        public AuthController(IConfiguration configuration, IUsuarioService usuarioService)
        {
            _configuration = configuration;
            _usuarioService = usuarioService;
        }

[HttpPost("login")]
public IActionResult Login([FromBody] LoginRequest loginRequest)
{
    var user = _usuarioService.ValidateUser(loginRequest.Email, loginRequest.Password);
    if (user == null)
    {
        return Unauthorized("Credenciales incorrectas");
    }

    // if (!user.EmailVerificado)
    // {
    //     return BadRequest("Por favor, verifica tu correo electrónico antes de iniciar sesión.");
    // }

    var token = GenerateJwtToken(user);
    var refreshToken = GenerateRefreshToken();

    _usuarioService.SaveRefreshToken(user.UsuarioId, refreshToken);

    // Incluye el `userId` en la respuesta
    return Ok(new { userId = user.UsuarioId, token, refreshToken });
}


        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] TokenRequest tokenRequest)
        {
            var principal = GetPrincipalFromExpiredToken(tokenRequest.AccessToken);
            if (principal == null) return BadRequest("Token inválido.");

            var usuarioId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            var savedRefreshToken = _usuarioService.GetRefreshToken(usuarioId);

            if (savedRefreshToken != tokenRequest.RefreshToken || savedRefreshToken == null || _usuarioService.IsRefreshTokenRevoked(usuarioId))
            {
                return Unauthorized("Refresh token inválido.");
            }

            var newJwtToken = GenerateJwtToken(_usuarioService.GetUserById(usuarioId));
            var newRefreshToken = GenerateRefreshToken();

            _usuarioService.RevokeRefreshToken(usuarioId);
            _usuarioService.SaveRefreshToken(usuarioId, newRefreshToken);

            return Ok(new { token = newJwtToken, refreshToken = newRefreshToken });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _usuarioService.GetUserByEmail(request.Email);
            if (user == null)
            {
                return NotFound("El usuario con el correo proporcionado no fue encontrado.");
            }

            var resetToken = GenerateResetToken();
            _usuarioService.SavePasswordResetToken(user.UsuarioId, resetToken);
            SendPasswordResetEmail(user.Email, resetToken);

            return Ok(new { message = "Se ha enviado un enlace de recuperación de contraseña a su correo electrónico.", token = resetToken });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = _usuarioService.GetUserByPasswordResetToken(request.Token);
            if (user == null || user.TokenExpirationDate < DateTime.Now)
            {
                return BadRequest("El token de recuperación es inválido o ha expirado.");
            }

            if (!_usuarioService.IsPasswordSecure(request.NewPassword))
            {
                return BadRequest("La nueva contraseña no cumple con los requisitos de seguridad.");
            }

            _usuarioService.UpdatePassword(user.UsuarioId, request.NewPassword);
            return Ok("Su contraseña ha sido actualizada exitosamente.");
        }

[HttpPost("verify-email")]
public IActionResult VerifyEmail([FromBody] VerifyEmailRequest request)
{
    var user = _usuarioService.GetUserByVerificationToken(request.Token);
    if (user == null)
    {
        return BadRequest("Token inválido.");
    }

    user.EmailVerificado = true;
    user.EmailVerificationToken = null; // Limpiamos el token tras la verificación
    _usuarioService.Update(user.UsuarioId, user);

    return Ok("Correo verificado exitosamente.");
}


      [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    var userExistsEmail = _usuarioService.GetUserByEmail(request.Email);
    if (userExistsEmail != null)
    {
        return BadRequest("El correo ya está registrado.");
    }

    var userExistsPhone = _usuarioService.GetUserByPhone(request.Phone);
    if (userExistsPhone != null)
    {
        return BadRequest("El número de teléfono ya está registrado.");
    }

    // Crear nuevo usuario
    var newUser = new Usuario
    {
        Nombre = request.Nombre,
        Apellido = request.Apellido,
        Email = request.Email,
        Password = request.Password,
        Phone = request.Phone,
        Rol = "Usuario", // Asignamos un rol predeterminado
        EmailVerificationToken = GenerateVerificationToken(),
        EmailVerificado = false // El correo no está verificado al principio
    };

    await _usuarioService.Save(newUser);

    // Enviar correo de verificación
    SendVerificationEmail(newUser.Email, newUser.EmailVerificationToken);

    return Ok("Usuario registrado exitosamente. Por favor, verifique su correo electrónico.");
}




        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }


        private string GenerateJwtToken(Usuario user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UsuarioId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UsuarioId.ToString()),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateResetToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private string GenerateVerificationToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            return principal;
        }

        private void SendPasswordResetEmail(string email, string resetToken)
        {
            var resetLink = $"https://tudominio.com/reset-password?token={resetToken}";
            var message = $"Use el siguiente enlace para restablecer su contraseña: {resetLink}";
        }

        private void SendVerificationEmail(string email, string verificationToken)
        {
            var verificationLink = $"https://tudominio.com/verify-email?token={verificationToken}";
            var message = $"Haga clic en el siguiente enlace para verificar su correo electrónico: {verificationLink}";
        }
    }

    public class TokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

    public class VerifyEmailRequest
    {
        public string Token { get; set; }
    }

    public class RegisterRequest
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? Phone { get; set; }
    }
}
