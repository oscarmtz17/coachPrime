public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public string Token { get; set; }  // Token generado
    public DateTime ExpirationDate { get; set; }  // Fecha de expiración
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }
}
