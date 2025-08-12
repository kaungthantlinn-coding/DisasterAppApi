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
    public class ImpactTypeService : IImpactTypeService
    {
        private readonly IImpactTypeRepository _impactTypeRepository;
        public ImpactTypeService(IImpactTypeRepository impactTypeRepository)
        {
            _impactTypeRepository = impactTypeRepository;
        }
        public async Task<IEnumerable<ImpactTypeDto>> GetAllAsync()
        {
           
                
                var entities=await _impactTypeRepository.GetAllAsync();
            return entities.Select(e=> new ImpactTypeDto
            {
                Id = e.Id,
                Name = e.Name
            }).ToList();
        }
        
        public async Task<ImpactTypeDto?> GetByIdAsync(int id)
        {
            var impactType = await _impactTypeRepository.GetByIdAsync(id);
            if (impactType == null) return null;
            return new ImpactTypeDto
            {
                Id = impactType.Id,
                Name = impactType.Name
            };
        }
        public async Task<ImpactTypeDto> CreateAsync(ImpactTypeCreateDto createDto)
        {
            var all=await _impactTypeRepository.GetAllAsync();
            if ( all.Any(e => e.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Impact type with name '{createDto.Name}' already exists.");
            }
            var impactType = new ImpactType
            {
                Name = createDto.Name
            };
            var createdImpactType = await _impactTypeRepository.CreateAsync(impactType);
            return new ImpactTypeDto
            {
                Id = createdImpactType.Id,
                Name = createdImpactType.Name
            };
        }
        public async Task<ImpactTypeDto> UpdateAsync(int id, ImpactTypeUpdateDto dto)
        {
            var entity = await _impactTypeRepository.GetByIdAsync(id);
            if (entity == null)
                throw new Exception("ImpactType not found");

            // Check duplicate name except current
            var all = await _impactTypeRepository.GetAllAsync();
            if (all.Any(e => e.Id != id && e.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("ImpactType name already exists");

            entity.Name = dto.Name ?? entity.Name;

            var updated = await _impactTypeRepository.UpdateAsync(entity);

            return new ImpactTypeDto
            {
                Id = updated.Id,
                Name = updated.Name
            };
        }
        public async Task DeleteAsync(int id)
        {
            var impactType = await _impactTypeRepository.GetByIdAsync(id);
            if (impactType == null) 
                throw new KeyNotFoundException($"Impact type with ID {id} not found.");
            await _impactTypeRepository.DeleteAsync(id);
        }
    }

}
