using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services.Interfaces
{
    public interface IDonationService
    {
        Task<int> CreateDonationAsync(Guid userId, CreateDonationDto dto);
        Task<bool> VerifyDonationAsync(int donationId, Guid adminUserId, VerifyDonationDto dto);
        Task<List<DonationDto>> GetDonationsByOrganizationIdAsync(int organizationId);
        Task<Donation?> GetDonationByIdAsync(int donationId);
        Task<List<DonationDto>> GetPendingDonationsAsync();
    }
}
