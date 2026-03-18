using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Services
{
    public interface ICloudinaryService
    {
        Task<bool> TestConnectionAsync();
        Task<UploadResult> UploadImageAsync(IFormFile file);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> options)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                // Try to get a resource - will fail with auth error if credentials are invalid
                await _cloudinary.GetResourceAsync("test");
                return true;
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("403") || ex.Message.Contains("Unauthorized"))
            {
                // Invalid credentials
                return false;
            }
            catch
            {
                // If resource doesn't exist or other errors, connection is working
                return true;
            }
        }

        public async Task<UploadResult> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = "SarEquipEnterprise"
                };

                return await _cloudinary.UploadAsync(uploadParams);
            }
        }
    }
}
