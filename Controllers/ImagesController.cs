using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using webapi.Services;

[Route("api/[controller]")]
[ApiController]
public class ImagesController : ControllerBase
{
    private readonly S3Service _s3Service;

    public ImagesController(S3Service s3Service)
    {
        _s3Service = s3Service;
    }

    [HttpGet]
    public async Task<IActionResult> ListImages()
    {
        var images = await _s3Service.ListImagesAsync();
        return Ok(images);
    }

    [HttpPost("upload-private")]
    public async Task<IActionResult> UploadPrivateImage([FromForm] IFormFile file, [FromForm] string category, [FromForm] string userId, [FromForm] string customName)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(customName))
            return BadRequest("Category, userId, or customName is missing.");

        string uniqueIdentifier = Guid.NewGuid().ToString();
        var extension = file.FileName.Split('.').Last();
        var key = $"private/{userId}/{category}/{uniqueIdentifier}_{customName.Replace(" ", "_")}.{extension}";

        using var stream = file.OpenReadStream();
        var url = await _s3Service.UploadImageAsync(key, stream);
        return Ok(new { Url = url });
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> DeleteImage(string key)
    {
        await _s3Service.DeleteImageAsync(key);
        return NoContent();
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> ListImagesByCategory(string category)
    {
        var images = await _s3Service.ListImagesByCategoryAsync(category);
        return Ok(images);
    }

    [HttpGet("combined-images")]
    [Authorize]
    public async Task<IActionResult> ListCombinedImagesByCategory([FromQuery] string userId, [FromQuery] string category)
    {
        var publicImages = await _s3Service.ListImagesByCategoryAsync(category);
        var userImages = await _s3Service.ListUserImagesByCategoryAsync(userId, category);
        var combinedImages = publicImages.Concat(userImages).ToList();
        return Ok(combinedImages);
    }

    [HttpPost("upload-logo")]
    [Authorize]
    public async Task<IActionResult> UploadUserLogo([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found.");

        string uniqueIdentifier = Guid.NewGuid().ToString();
        var extension = file.FileName.Split('.').Last();
        var key = $"private/{userId}/logo/{uniqueIdentifier}_logo.{extension}";

        using var stream = file.OpenReadStream();
        var url = await _s3Service.UploadImageAsync(key, stream);
        return Ok(new { Url = url });
    }

    [HttpGet("user-logo")]
    [Authorize]
    public async Task<IActionResult> GetUserLogo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found.");

        // Prefijo exacto para la carpeta de logos del usuario
        var prefix = $"private/{userId}/logo/";

        // Obtén el logo específico usando ListUserImagesByCategoryAsync para obtener resultados más predecibles
        var logos = await _s3Service.ListUserImagesByCategoryAsync(userId, "logo");

        if (logos.Count == 0)
            return NotFound("No logo found for the user.");

        return Ok(new { url = logos[0] });
    }

    [Authorize]
    [HttpPost("upload-progress-images/{clienteId}/{progresoId}")]
    public async Task<IActionResult> UploadProgressImages(
        int clienteId,
        int progresoId,
        [FromForm] List<IFormFile> files
    )
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User ID not found.");

        var urls = new List<string>();
        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                string uniqueIdentifier = Guid.NewGuid().ToString();
                var extension = file.FileName.Split('.').Last();
                var key = $"private/{userId}/progress/{progresoId}/{uniqueIdentifier}.{extension}";

                using var stream = file.OpenReadStream();
                var url = await _s3Service.UploadImageAsync(key, stream);
                urls.Add(url);
            }
        }

        return Ok(new { ProgresoId = progresoId, Urls = urls });
    }






}
