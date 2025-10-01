using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
namespace ECommerce.BLL.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadsPath;
        private readonly string _baseUrl;

        public FileService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _uploadsPath = Path.Combine(environment.WebRootPath, "uploads");
            _baseUrl = configuration["BaseUrl"] ?? "https://localhost:7000";

            // Ensure uploads directory exists
            if (!Directory.Exists(_uploadsPath))
            {
                Directory.CreateDirectory(_uploadsPath);
            }

        }

        public async Task<string> SaveFileAsync(IFormFile file, string subfolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var subfolderPath = Path.Combine(_uploadsPath, subfolder);
            if (!Directory.Exists(subfolderPath))
            {
                Directory.CreateDirectory(subfolderPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(subfolderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(subfolder, fileName).Replace("\\", "/");
        }

        public async Task<bool> DeleteFileAsync(string relativePath)
        {
            try
            {
                var fullPath = Path.Combine(_uploadsPath, relativePath.Replace("/", "\\"));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetFileUrl(string relativePath)
        {
            return $"{_baseUrl}/uploads/{relativePath}";
        }

    }
}