using DisasterApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services.Interfaces
{
    public interface IOrganizationService
    {
        Task<int> CreateOrganizationAsync(Guid userId, CreateOrganizationDto dto);
        Task<bool> UpdateOrganizationAsync(int id, Guid userId, UpdateOrganizationDto dto);
        Task<bool> DeleteOrganizationAsync(int id, Guid userId);
        Task<OrganizationDto?> GetOrganizationByIdAsync(int id);
        Task<List<OrganizationDto>> GetOrganizationsAsync();
    }
}
