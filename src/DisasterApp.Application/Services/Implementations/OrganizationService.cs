using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services.Implementations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repository;

        public OrganizationService(IOrganizationRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateOrganizationAsync(Guid userId, CreateOrganizationDto dto)
        {
            var org = new Organization
            {
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                LogoUrl = dto.LogoUrl,
                WebsiteUrl = dto.WebsiteUrl,
                ContactEmail = dto.ContactEmail
            };
            return await _repository.AddAsync(org);
        }

        public async Task<bool> UpdateOrganizationAsync(int id, Guid userId, UpdateOrganizationDto dto)
        {
            var org = await _repository.GetByIdAsync(id);
            if (org == null || org.UserId != userId) return false;

            org.Name = dto.Name;
            org.Description = dto.Description;
            org.LogoUrl = dto.LogoUrl;
            org.WebsiteUrl = dto.WebsiteUrl;
            org.ContactEmail = dto.ContactEmail;

            return await _repository.UpdateAsync(org);
        }

        public async Task<bool> DeleteOrganizationAsync(int id, Guid userId)
        {
            var org = await _repository.GetByIdAsync(id);
            if (org == null || org.UserId != userId) return false;

            return await _repository.DeleteAsync(id);
        }

        public async Task<OrganizationDto?> GetOrganizationByIdAsync(int id)
        {
            var org = await _repository.GetByIdAsync(id);
            if (org == null) return null;

            return new OrganizationDto
            {
                Id = org.Id,
                Name = org.Name,
                Description = org.Description,
                LogoUrl = org.LogoUrl,
                WebsiteUrl = org.WebsiteUrl,
                ContactEmail = org.ContactEmail,
                CreatedAt = org.CreatedAt
            };
        }

        public async Task<List<OrganizationDto>> GetOrganizationsAsync()
        {
            var orgs = await _repository.GetAllAsync();
            return orgs.Select(org => new OrganizationDto
            {
                Id = org.Id,
                Name = org.Name,
                Description = org.Description,
                LogoUrl = org.LogoUrl,
                WebsiteUrl = org.WebsiteUrl,
                ContactEmail = org.ContactEmail,
                CreatedAt = org.CreatedAt
            }).ToList();
        }
    }
}
