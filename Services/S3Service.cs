using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"]; // Asegúrate de que este valor esté en appsettings.json
    }

    public async Task<List<string>> ListImagesAsync()
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName
        };

        var response = await _s3Client.ListObjectsV2Async(request);
        var imageUrls = new List<string>();

        foreach (var obj in response.S3Objects)
        {
            string url = $"https://{_bucketName}.s3.amazonaws.com/{obj.Key}";
            imageUrls.Add(url);
        }

        return imageUrls;
    }

public async Task<List<string>> ListImagesByCategoryAsync(string category)
{
    var request = new ListObjectsV2Request
    {
        BucketName = _bucketName,
        Prefix = $"{category}/"
    };

    var response = await _s3Client.ListObjectsV2Async(request);
    var imageUrls = response.S3Objects
        .Where(obj => !obj.Key.EndsWith("/")) // Filtra la carpeta (que termina con "/")
        .Select(obj => $"https://{_bucketName}.s3.amazonaws.com/{obj.Key}")
        .ToList();

    return imageUrls;
}

public async Task<List<string>> ListUserImagesByCategoryAsync(string userId, string category)
{
    var request = new ListObjectsV2Request
    {
        BucketName = _bucketName,
        Prefix = $"private/{userId}/{category}/" // Ruta para las imágenes privadas del usuario
    };

    var response = await _s3Client.ListObjectsV2Async(request);
    var imageUrls = response.S3Objects
        .Where(obj => !obj.Key.EndsWith("/")) // Excluye carpetas
        .Select(obj => $"https://{_bucketName}.s3.amazonaws.com/{obj.Key}")
        .ToList();

    return imageUrls;
}




public async Task<string> UploadImageAsync(string key, Stream imageStream)
{
    var request = new PutObjectRequest
    {
        BucketName = _bucketName,
        Key = key,
        InputStream = imageStream,
        ContentType = "image/jpeg",
        CannedACL = S3CannedACL.Private // Almacenamos la imagen como privada
    };

    await _s3Client.PutObjectAsync(request);
    return $"https://{_bucketName}.s3.amazonaws.com/{key}";
}



    public async Task DeleteImageAsync(string key)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(request);
    }
}
