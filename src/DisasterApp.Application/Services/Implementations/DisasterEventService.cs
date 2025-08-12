using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public class DisasterEventService:IDisasterEventService
    {
        private readonly IDisasterEventRepository _repository;

        public DisasterEventService(IDisasterEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DisasterEventDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return list.Select(e => new DisasterEventDto
            {
                Id = e.Id,
                Name = e.Name,
                DisasterTypeId = e.DisasterTypeId
            }).ToList();
        }

        public async Task<DisasterEventDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;

            return new DisasterEventDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DisasterTypeId = entity.DisasterTypeId
            };
        }

        public async Task AddAsync(CreateDisasterEventDto dto)
        {
            var entity = new DisasterEvent
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                DisasterTypeId = dto.DisasterTypeId
            };

            await _repository.AddAsync(entity);
        }

        public async Task UpdateAsync(Guid id, UpdateDisasterEventDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("DisasterEvent not found.");

            existing.Name = dto.Name;
            existing.DisasterTypeId = dto.DisasterTypeId;

            await _repository.UpdateAsync(existing);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return false;

            await _repository.DeleteAsync(entity);
            return true;
        }
    }
}
