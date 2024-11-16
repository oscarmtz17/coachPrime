using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _bucketName = configuration["AWS:BucketName"];
    }

    // Método para generar una URL firmada
    public string GetPresignedUrl(string key, int expirationInSeconds = 3600)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddSeconds(expirationInSeconds)
        };

        return _s3Client.GetPreSignedURL(request);
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
            .Where(obj => !obj.Key.EndsWith("/"))
            .Select(obj => GetPresignedUrl(obj.Key)) // Generamos una URL firmada
            .ToList();

        return imageUrls;
    }

    public async Task<List<string>> ListImagesAsync()
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName
        };

        var response = await _s3Client.ListObjectsV2Async(request);
        var imageUrls = response.S3Objects
            .Where(obj => !obj.Key.EndsWith("/"))
            .Select(obj => GetPresignedUrl(obj.Key)) // Generamos una URL firmada
            .ToList();

        return imageUrls;
    }


    public async Task<List<string>> ListUserImagesByCategoryAsync(string userId, string category)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = $"private/{userId}/{category}/"
        };

        var response = await _s3Client.ListObjectsV2Async(request);
        var imageUrls = response.S3Objects
            .Where(obj => !obj.Key.EndsWith("/"))
            .Select(obj => GetPresignedUrl(obj.Key)) // Generamos una URL firmada
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
            CannedACL = S3CannedACL.Private
        };

        await _s3Client.PutObjectAsync(request);
        return GetPresignedUrl(key); // Retornamos la URL firmada de la imagen subida
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

    public async Task<List<string>> ListUserProgressImagesAsync(string userId, string progressDate)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = $"private/{userId}/progress/{progressDate}/"
        };

        var response = await _s3Client.ListObjectsV2Async(request);
        var imageUrls = response.S3Objects
            .Where(obj => !obj.Key.EndsWith("/"))
            .Select(obj => GetPresignedUrl(obj.Key))
            .ToList();

        return imageUrls;
    }

    public async Task DeleteFolderAsync(string folderKey)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = folderKey
        };

        var response = await _s3Client.ListObjectsV2Async(request);
        if (response.S3Objects.Count > 0)
        {
            var deleteObjectsRequest = new DeleteObjectsRequest
            {
                BucketName = _bucketName,
                Objects = response.S3Objects.Select(obj => new KeyVersion { Key = obj.Key }).ToList()
            };

            await _s3Client.DeleteObjectsAsync(deleteObjectsRequest);
        }
    }

    public async Task<List<string>> ListImagesInFolderAsync(string folderKey)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = _bucketName,
            Prefix = folderKey
        };

        var response = await _s3Client.ListObjectsV2Async(request);
        var imageUrls = response.S3Objects
            .Where(obj => !obj.Key.EndsWith("/")) // Ignorar las carpetas
            .Select(obj => GetPresignedUrl(obj.Key)) // Generar URLs firmadas
            .ToList();

        return imageUrls;
    }




}
