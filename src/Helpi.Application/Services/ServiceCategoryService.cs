
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Helpi.Application.Services;


public class ServiceCategoryService
{
        private readonly IServiceCategoryRepository _repository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public ServiceCategoryService(
                IServiceCategoryRepository repository,
                IMapper mapper,
                IWebHostEnvironment environment,
                IConfiguration configuration
        )
        {
                _repository = repository;
                _mapper = mapper;
                _environment = environment;
                _configuration = configuration;
        }

        public async Task<List<ServiceCategoryDto>> GetAllCategoriesAsync() =>
            _mapper.Map<List<ServiceCategoryDto>>(await _repository.GetAllAsync(excludeDeleted: true));

        public async Task<ServiceCategoryDto> CreateCategoryAsync(ServiceCategoryCreateDto dto)
        {
                var category = _mapper.Map<ServiceCategory>(dto);
                await _repository.AddAsync(category);
                return _mapper.Map<ServiceCategoryDto>(category);
        }

        public async Task<ServiceCategoryDto?> UpdateCategoryAsync(int id, ServiceCategoryUpdateDto dto)
        {
                var category = await _repository.GetByIdAsync(id);
                if (category == null)
                        return null;

                _mapper.Map(dto, category);

                await _repository.UpdateAsync(category);

                return _mapper.Map<ServiceCategoryDto>(category);
        }


        public async Task<ServiceCategoryIconDto> UploadIconAsync(int id, IFormFile iconFile)
        {


                var serviceCategory = await _repository.GetByIdAsync(id);
                if (serviceCategory == null)
                        throw new ArgumentException($"ServiceCategory with ID {id} not found");

                // Validate file
                if (iconFile == null || iconFile.Length == 0)
                        throw new ArgumentException("No file uploaded");

                // Validate file type
                var allowedExtensions = new[] { ".svg", ".png", ".jpg", ".jpeg", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(iconFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                        throw new ArgumentException("Invalid file type. Allowed types: " + string.Join(", ", allowedExtensions));

                // Validate file size (max 2MB)
                if (iconFile.Length > 2 * 1024 * 1024)
                        throw new ArgumentException("File size too large. Maximum size is 2MB");

                // Delete old icon if exists
                if (!string.IsNullOrEmpty(serviceCategory.Icon))
                {
                        await DeleteIconFile(serviceCategory.Icon);
                }

                // Generate unique filename
                var fileName = $"icon_{id}_{Guid.NewGuid()}{fileExtension}";
                var webRootPath = _environment.WebRootPath;


                if (string.IsNullOrEmpty(webRootPath))
                {
                        webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                        if (!Directory.Exists(webRootPath))
                        {
                                Directory.CreateDirectory(webRootPath);
                        }
                }
                var uploadsPath = Path.Combine(webRootPath, "uploads", "icons");

                // Ensure directory exists
                if (!Directory.Exists(uploadsPath))
                        Directory.CreateDirectory(uploadsPath);

                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                        await iconFile.CopyToAsync(stream);
                }

                // Update database with relative path
                var relativePath = $"/uploads/icons/{fileName}";
                serviceCategory.Icon = relativePath;

                await _repository.UpdateAsync(serviceCategory);

                return new ServiceCategoryIconDto
                {
                        Id = serviceCategory.Id,
                        Icon = relativePath
                };
        }

        public async Task<bool> DeleteIconAsync(int serviceCategoryId)
        {
                var serviceCategory = await _repository.GetByIdAsync(serviceCategoryId);
                if (serviceCategory == null || string.IsNullOrEmpty(serviceCategory.Icon))
                        return false;

                // Delete physical file
                await DeleteIconFile(serviceCategory.Icon);

                // Update database
                serviceCategory.Icon = null;
                await _repository.UpdateAsync(serviceCategory);

                return true;
        }

        private async Task DeleteIconFile(string iconPath)
        {
                try
                {
                        if (!string.IsNullOrEmpty(iconPath))
                        {
                                var fullPath = Path.Combine(_environment.WebRootPath, iconPath.TrimStart('/'));
                                if (File.Exists(fullPath))
                                {
                                        File.Delete(fullPath);
                                }
                        }
                }
                catch (Exception ex)
                {
                        // Log the exception but don't throw - we don't want file deletion failures to break the main operation
                        Console.WriteLine($"Error deleting icon file: {ex.Message}");
                }
        }

}

