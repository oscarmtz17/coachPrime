using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using webapi.Models;
using webapi.Services;
using Stripe;
using Stripe.Checkout;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUsuarioService _usuarioService;
        private readonly EmailService _emailService;
        private readonly ISuscripcionService _suscripcionService;

        public AuthController(IConfiguration configuration, IUsuarioService usuarioService, EmailService emailService, ISuscripcionService suscripcionService)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            _usuarioService = usuarioService;
            _emailService = emailService;
            _suscripcionService = suscripcionService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                Console.WriteLine($"Login attempt for email: {loginRequest.Email}");

                var user = _usuarioService.ValidateUser(loginRequest.Email, loginRequest.Password);
                if (user == null)
                {
                    Console.WriteLine("Invalid credentials");
                    return Unauthorized("Credenciales incorrectas");
                }

                Console.WriteLine($"User validated: {user.UsuarioId}");

                // if (!user.EmailVerificado)
                // {
                //     return BadRequest("Por favor, verifica tu correo electrónico antes de iniciar sesión.");
                // }

                var token = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                Console.WriteLine($"Generated tokens - JWT: {!string.IsNullOrEmpty(token)}, Refresh: {!string.IsNullOrEmpty(refreshToken)}");

                _usuarioService.SaveRefreshToken(user.UsuarioId, refreshToken);
                Console.WriteLine($"Refresh token saved for user {user.UsuarioId}");

                // Incluye el `userId` en la respuesta
                return Ok(new { userId = user.UsuarioId, token, refreshToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in login endpoint: {ex.Message}");
                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] TokenRequest tokenRequest)
        {
            try
            {
                Console.WriteLine($"Refresh request received. AccessToken: {!string.IsNullOrEmpty(tokenRequest.AccessToken)}, RefreshToken: {!string.IsNullOrEmpty(tokenRequest.RefreshToken)}");

                var principal = GetPrincipalFromExpiredToken(tokenRequest.AccessToken);
                if (principal == null)
                {
                    Console.WriteLine("Invalid access token");
                    return BadRequest("Token inválido.");
                }

                var usuarioId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier));
                Console.WriteLine($"User ID from token: {usuarioId}");

                var savedRefreshToken = _usuarioService.GetRefreshToken(usuarioId);
                Console.WriteLine($"Saved refresh token: {!string.IsNullOrEmpty(savedRefreshToken)}");
                Console.WriteLine($"Token match: {savedRefreshToken == tokenRequest.RefreshToken}");
                Console.WriteLine($"Is revoked: {_usuarioService.IsRefreshTokenRevoked(usuarioId)}");

                if (savedRefreshToken != tokenRequest.RefreshToken || savedRefreshToken == null || _usuarioService.IsRefreshTokenRevoked(usuarioId))
                {
                    Console.WriteLine("Refresh token validation failed");
                    return Unauthorized("Refresh token inválido.");
                }

                var user = _usuarioService.GetUserById(usuarioId);
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return NotFound("Usuario no encontrado.");
                }

                var newJwtToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken();

                _usuarioService.RevokeRefreshToken(usuarioId);
                _usuarioService.SaveRefreshToken(usuarioId, newRefreshToken);

                Console.WriteLine("Token refresh successful");
                return Ok(new { token = newJwtToken, refreshToken = newRefreshToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in refresh endpoint: {ex.Message}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                int userId = int.Parse(userIdClaim.Value);
                _usuarioService.RevokeRefreshToken(userId);
            }

            return Ok(new { message = "Sesión cerrada exitosamente" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = _usuarioService.GetUserByEmail(request.Email);
            if (user == null)
            {
                return NotFound("El usuario con el correo proporcionado no fue encontrado.");
            }

            var resetToken = GenerateResetToken();
            _usuarioService.SavePasswordResetToken(user.UsuarioId, resetToken);

            await _emailService.SendPasswordResetEmail(user.Email, resetToken);

            return Ok(new { message = "Se ha enviado un enlace de recuperación de contraseña a su correo electrónico." });
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
                return BadRequest("La nueva contraseña no cumple con los requisitos de seguridad. Asegúrate de que:\n" +
                  "- Tenga al menos 8 caracteres.\n" +
                  "- Contenga al menos una letra mayúscula.\n" +
                  "- Contenga al menos una letra minúscula.\n" +
                  "- Contenga al menos un número.\n" +
                  "- Incluya al menos un carácter especial (por ejemplo: !@#$%^&*).");

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
            // Validar si el email ya está registrado
            var userExistsEmail = _usuarioService.GetUserByEmail(request.Email);
            if (userExistsEmail != null)
            {
                return BadRequest("El correo ya está registrado.");
            }

            // Validar si el teléfono ya está registrado
            var userExistsPhone = _usuarioService.GetUserByPhone(request.Phone);
            if (userExistsPhone != null)
            {
                return BadRequest("El número de teléfono ya está registrado.");
            }

            // Crear un nuevo usuario
            var newUser = new Usuario
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone,
                Rol = "Usuario", // Rol predeterminado
                EmailVerificationToken = GenerateVerificationToken(),
                EmailVerificado = false // El correo no está verificado al inicio
            };

            await _usuarioService.Save(newUser);
            await _emailService.SendRegistrationEmail(newUser.Email, $"{newUser.Nombre} {newUser.Apellido}");


            // Crear la suscripción en la base de datos
            var newSubscription = new Suscripcion
            {
                UsuarioId = newUser.UsuarioId,
                PlanId = request.PlanId,
                FechaInicio = DateTime.Now,
                EstadoSuscripcionId = (request.PlanId == 1)
                    ? 7 // Estado Prueba (en este caso para planes gratuitos)
                    : 1 // Estado Pendiente para planes de pago
            };

            await _suscripcionService.Save(newSubscription);

            // Si es un plan gratuito, registrar y devolver éxito
            if (request.PlanId == 1)
            {
                SendVerificationEmail(newUser.Email, newUser.EmailVerificationToken);
                return Ok(new { message = "Usuario registrado exitosamente." });
            }

            // Para planes de pago, generar sesión de Stripe Checkout
            var domain = "http://localhost:3000"; // Cambia al dominio de tu frontend
            var planStripePriceId = GetStripePriceId(request.PlanId);

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new SessionLineItemOptions
            {
                Price = planStripePriceId, // ID del plan en Stripe
                Quantity = 1,
            },
        },
                Mode = "subscription",
                SuccessUrl = $"{domain}/success",
                CancelUrl = $"{domain}/cancel",
                Metadata = new Dictionary<string, string>
        {
            { "userId", newUser.UsuarioId.ToString() },
            { "subscriptionId", newSubscription.SuscripcionId.ToString() },
            { "planId", request.PlanId.ToString() }
        }
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new { checkoutUrl = session.Url });
        }


        // Método auxiliar para obtener el ID del precio de Stripe
        private string GetStripePriceId(int planId)
        {
            return planId switch
            {
                2 => "price_1QR94gBZAdKqouiVj2rqIAq1",
                3 => "price_1QQKUQBZAdKqouiVDK0jLr25",
                4 => "price_1QR97hBZAdKqouiVKf5WRxMl",
                _ => throw new ArgumentException("Plan no válido."),
            };
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

        // Endpoint para obtener el usuario actual
        [HttpGet("current-user")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            // Obtiene el ID del usuario actual desde el token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);
            var user = _usuarioService.GetUserById(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Devolver solo los datos necesarios del usuario
            return Ok(new
            {
                user.UsuarioId,
                user.Nombre,
                user.Apellido,
                user.Email,
                user.Phone
            });
        }

        [HttpPost("change-password")]
        [Authorize]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            // Obtener el ID del usuario desde el token de autenticación
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);
            var user = _usuarioService.GetUserById(userId);

            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            // Verificar que la contraseña actual sea correcta
            if (!_usuarioService.ValidateUserPassword(user.UsuarioId, request.CurrentPassword))

            {
                return BadRequest("La contraseña actual es incorrecta.");
            }

            // Verificar que la nueva contraseña cumpla con los requisitos de seguridad
            if (!_usuarioService.IsPasswordSecure(request.NewPassword))
            {
                return BadRequest("La nueva contraseña no cumple con los requisitos de seguridad. Asegúrate de que:\n" +
                          "- Tenga al menos 8 caracteres.\n" +
                          "- Contenga al menos una letra mayúscula.\n" +
                          "- Contenga al menos una letra minúscula.\n" +
                          "- Contenga al menos un número.\n" +
                          "- Incluya al menos un carácter especial (por ejemplo: !@#$%^&*).");

            }

            // Actualizar la contraseña
            _usuarioService.UpdatePassword(user.UsuarioId, request.NewPassword);

            return Ok("Su contraseña ha sido actualizada exitosamente.");
        }

    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
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

        public int PlanId { get; set; }
    }
}
