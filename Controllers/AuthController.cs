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
            var user = _usuarioService.ValidateUser(loginRequest.Email, loginRequest.Contraseña);
            if (user == null)
            {
                return Unauthorized("Credenciales incorrectas");
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            // Guarda el refresh token en la base de datos
            _usuarioService.SaveRefreshToken(user.UsuarioId, refreshToken);

            return Ok(new { token, refreshToken });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] TokenRequest tokenRequest)
        {
            var principal = GetPrincipalFromExpiredToken(tokenRequest.AccessToken);
            if (principal == null) return BadRequest("Token inválido.");

            var usuarioId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
            var savedRefreshToken = _usuarioService.GetRefreshToken(usuarioId); // Obtén el refresh token de la base de datos

            if (savedRefreshToken != tokenRequest.RefreshToken || savedRefreshToken == null || _usuarioService.IsRefreshTokenRevoked(usuarioId))
            {
                return Unauthorized("Refresh token inválido.");
            }

            // Genera nuevo AccessToken y RefreshToken
            var newJwtToken = GenerateJwtToken(_usuarioService.GetUserById(usuarioId));
            var newRefreshToken = GenerateRefreshToken();

            // Actualiza el refresh token en la base de datos
            _usuarioService.RevokeRefreshToken(usuarioId); // Revoca el anterior
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

            // Generar el token de recuperación
            var resetToken = GenerateResetToken();

            // Guardar el token en la base de datos o en un campo de usuario
            _usuarioService.SavePasswordResetToken(user.UsuarioId, resetToken);

            // Enviar el token al correo electrónico
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

    // Validar la seguridad de la nueva contraseña
    if (!_usuarioService.IsPasswordSecure(request.NewPassword))
    {
        return BadRequest("La nueva contraseña no cumple con los requisitos de seguridad. Debe tener al menos 8 caracteres, una letra mayúscula, una letra minúscula, un número y un carácter especial.");
    }

    // Restablecer la contraseña del usuario usando un método específico
    try
    {
        _usuarioService.UpdatePassword(user.UsuarioId, request.NewPassword);  // Usar el método específico para actualizar la contraseña
        return Ok("Su contraseña ha sido actualizada exitosamente.");
    }
    catch (Exception ex)
    {
        return StatusCode(500, "Ocurrió un error al actualizar la contraseña.");
    }
}


        private string GenerateJwtToken(Usuario user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UsuarioId.ToString()), // Usa UsuarioId como Sub
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UsuarioId.ToString()), // Usa el UsuarioId como NameIdentifier
                new Claim(ClaimTypes.Role, user.Rol)  // Agregar el rol del usuario como claim
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

        private string GenerateRefreshToken()
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
                ValidateAudience = false, // Ignorar validación de audiencia
                ValidateIssuer = false, // Ignorar validación de emisor
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false // Ignorar que el token esté expirado
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Token inválido.");
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private void SendPasswordResetEmail(string email, string resetToken)
        {
            // Aquí se puede integrar un servicio de envío de correos electrónicos (SendGrid, SMTP, etc.)
            var resetLink = $"https://tudominio.com/reset-password?token={resetToken}";
            var message = $"Use el siguiente enlace para restablecer su contraseña: {resetLink}";
            
            // Lógica para enviar el correo electrónico
            // emailService.SendEmail(email, "Recuperación de contraseña", message);
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
}
