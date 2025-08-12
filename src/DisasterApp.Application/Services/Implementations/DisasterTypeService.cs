using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public class DisasterTypeService : IDisasterTypeService
    {
        private readonly IDisasterTypeRepository _repository;

        public DisasterTypeService(IDisasterTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DisasterTypeDto>> GetAllAsync()
        {
            var disasterTypes = await _repository.GetAllAsync();
            return disasterTypes.Select(d => new DisasterTypeDto
            {
                Id = d.Id,
                Name = d.Name,
                Category = d.Category
            }).ToList();
        }

        public async Task<DisasterTypeDto?> GetByIdAsync(int id)
        {
            var disasterType = await _repository.GetByIdAsync(id);
            if (disasterType == null) return null;

            return new DisasterTypeDto
            {
                Id = disasterType.Id,
                Name = disasterType.Name,
                Category = disasterType.Category
            };
        }

        public async Task AddAsync(CreateDisasterTypeDto dto)
        {
            var entity = new DisasterType
            {
                Name = dto.Name,
                Category = dto.Category
            };

            await _repository.AddAsync(entity);
        }

        public async Task UpdateAsync(int id, UpdateDisasterTypeDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"DisasterType with ID {id} not found.");
            }

            existing.Name = dto.Name;
            existing.Category = dto.Category;

            await _repository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            await _repository.DeleteAsync(id);
            return true;
        }
    }
}
