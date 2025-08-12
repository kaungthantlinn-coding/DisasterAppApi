using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DisasterApp.Application.DTOs;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Message { get; set; } = null!;
    public string? AttachmentUrl { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class SendChatWithFileDto
{
    [Required]
    public Guid SenderId { get; set; }
    
    [Required]
    public Guid ReceiverId { get; set; }
    
    public string? Message { get; set; }
    
    public IFormFile? File { get; set; }
}