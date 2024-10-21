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

        private string GenerateJwtToken(Usuario user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UsuarioId.ToString()), // Usa UsuarioId como Sub
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UsuarioId.ToString()) // Usa el UsuarioId como NameIdentifier
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
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
    }

    public class TokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
