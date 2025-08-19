using DisasterApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.DTOs
{
    public class CreateDonationDto
    {
        public string DonorName { get; set; } = null!;
        public string? DonorContact { get; set; }
        public DonationType DonationType { get; set; }  // Enum directly
        public decimal? Amount { get; set; }
        public string Description { get; set; } = null!;
    }
    public class DonationDto
    {
        public int Id { get; set; }
        public string DonorName { get; set; } = null!;
        public string? DonorContact { get; set; }
        public string DonationType { get; set; } = null!;
        public decimal? Amount { get; set; }
        public string Description { get; set; } = null!;
        public DateTime ReceivedAt { get; set; }
        public string Status { get; set; } = null!;
        public string? TransactionPhotoUrl { get; set; }
    }
    public class VerifyDonationDto
    {
        //public string TransactionPhotoUrl { get; set; } = null!;
        public IFormFile TransactionPhoto { get; set; } = null!;
    }
}
