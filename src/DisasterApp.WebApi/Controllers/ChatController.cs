using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DisasterApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly DisasterDbContext _context;

    public ChatController(DisasterDbContext context)
    {
        _context = context;
    }

    // User sends message to CJ
    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromForm] SendChatWithFileDto dto)
    {
        try
        {
            if (dto.SenderId == Guid.Empty || dto.ReceiverId == Guid.Empty)
                return BadRequest("Invalid data");
            // Message (text) မရှိ၊ attachment (image) မရှိ - error
            if (string.IsNullOrWhiteSpace(dto.Message) && (dto.File == null || dto.File.Length == 0))
                return BadRequest("Message or image is required");

            string? attachmentUrl = null;
            if (dto.File != null && dto.File.Length > 0)
            {
                // Save file to wwwroot/uploads/chat
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }
                // Save relative path for client access
                attachmentUrl = $"/uploads/chat/{fileName}";
            }

            var chat = new Chat
            {
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                Message = dto.Message ?? string.Empty,
                SentAt = DateTime.UtcNow,
                IsRead = false,
                AttachmentUrl = attachmentUrl
            };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, attachmentUrl });
        }
        catch (Exception ex)
        {
            var errorMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return StatusCode(500, new
            {
                error = errorMsg,
                senderId = dto.SenderId,
                receiverId = dto.ReceiverId,
                message = dto.Message
            });
        }
    }

    // CJ gets received messages
    [HttpGet("received/{cjId}")]
    [Authorize(Roles = "cj")]
    public async Task<IActionResult> GetReceivedMessages(Guid cjId)
    {
        var messages = await _context.Chats
            .Where(c => c.ReceiverId == cjId)
            .Include(c => c.Sender)
            .OrderByDescending(c => c.SentAt)
            .Select(c => new
            {
                c.ChatId,
                c.SenderId,
                c.ReceiverId,
                c.Message,
                c.SentAt,
                c.IsRead,
                SenderName = c.Sender.Name,
                SenderEmail = c.Sender.Email,
                SenderPhoto = c.Sender.PhotoUrl
            })
            .ToListAsync();
        return Ok(messages);
    }

    // Mark message as read
    [HttpPost("mark-read/{chatId}")]
    [Authorize(Roles = "cj")]
    public async Task<IActionResult> MarkAsRead(int chatId)
    {
        var chat = await _context.Chats.FindAsync(chatId);
        if (chat == null) return NotFound();
        chat.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    // Get conversation between user and CJ
    [HttpGet("conversation")]
    [Authorize]
    public async Task<IActionResult> GetConversation([FromQuery] Guid userId, [FromQuery] Guid cjId)
    {
        var messages = await _context.Chats
            .Where(c =>
                (c.SenderId == userId && c.ReceiverId == cjId) ||
                (c.SenderId == cjId && c.ReceiverId == userId))
            .OrderBy(c => c.SentAt)
            .Select(c => new ChatMessageDto
            {
                Id = Guid.NewGuid(), // Generate new Guid since ChatId is int
                SenderId = c.SenderId,
                ReceiverId = c.ReceiverId,
                Message = c.Message ?? string.Empty,
                SentAt = c.SentAt ?? DateTime.UtcNow,
                AttachmentUrl = c.AttachmentUrl,
                IsRead = c.IsRead ?? false
            })
            .ToListAsync();
        return Ok(messages);
    }
}
