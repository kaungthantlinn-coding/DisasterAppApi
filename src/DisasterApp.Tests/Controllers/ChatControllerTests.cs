using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Xunit;
using DisasterApp.Infrastructure.Data;
using DisasterApp.Domain.Entities;
using DisasterApp.WebApi.Controllers;
using DisasterApp.Application.DTOs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;

namespace DisasterApp.Tests.Controllers
{
    public class ChatControllerTests2 : IDisposable
    {
        private readonly DisasterDbContext _context;
        private readonly ChatController _controller;
        private readonly Mock<ILogger<ChatController>> _mockLogger;
        private readonly string _testUserId = Guid.NewGuid().ToString();

        public ChatControllerTests2()
        {
            var options = new DbContextOptionsBuilder<DisasterDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new DisasterDbContext(options);
            _mockLogger = new Mock<ILogger<ChatController>>();
            _controller = new ChatController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task SendMessage_ValidMessageWithoutFile_ReturnsOk()
        {
            // Arrange
            var receiverId = Guid.NewGuid();
            var dto = new SendChatWithFileDto
            {
                SenderId = Guid.Parse(_testUserId),
                ReceiverId = receiverId,
                Message = "Test message"
            };

            // Act
            var result = await _controller.SendMessage(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // The controller returns an anonymous type with success and attachmentUrl properties
            var responseType = okResult.Value.GetType();
            var successProperty = responseType.GetProperty("success");
            Assert.NotNull(successProperty);
            
            var successValue = (bool)successProperty.GetValue(okResult.Value)!;
            Assert.True(successValue);
            
            // Verify the message was saved to the database
            Assert.True(await _context.Chats.AnyAsync(c => c.Message == "Test message"));
        }

        [Fact]
        public async Task SendMessage_WithValidFile_ReturnsOk()
        {
            // Arrange
            var receiverId = Guid.NewGuid();
            var mockFile = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            mockFile.Setup(_ => _.FileName).Returns(fileName);
            mockFile.Setup(_ => _.Length).Returns(ms.Length);

            var dto = new SendChatWithFileDto
            {
                SenderId = Guid.Parse(_testUserId),
                ReceiverId = receiverId,
                Message = "Test message with file",
                File = mockFile.Object
            };

            // Act
            var result = await _controller.SendMessage(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // The controller returns an anonymous type with success and attachmentUrl properties
            var responseType = okResult.Value.GetType();
            var successProperty = responseType.GetProperty("success");
            var attachmentUrlProperty = responseType.GetProperty("attachmentUrl");
            
            Assert.NotNull(successProperty);
            Assert.NotNull(attachmentUrlProperty);
            
            var successValue = (bool)successProperty.GetValue(okResult.Value)!;
            Assert.True(successValue);
            
            var attachmentUrlValue = attachmentUrlProperty.GetValue(okResult.Value) as string;
            Assert.NotNull(attachmentUrlValue);
            Assert.Contains(".pdf", attachmentUrlValue);
        }

        [Fact]
        public async Task GetReceivedMessages_ValidUserId_ReturnsMessages()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var receiverId = Guid.Parse(_testUserId);
            
            // Add User entities first
            _context.Users.Add(new User
            {
                UserId = senderId,
                Name = "Sender User",
                Email = "sender@test.com",
                AuthProvider = "local",
                AuthId = senderId.ToString()
            });
            
            _context.Users.Add(new User
            {
                UserId = receiverId,
                Name = "Receiver User",
                Email = "receiver@test.com",
                AuthProvider = "local",
                AuthId = receiverId.ToString()
            });
            
            _context.Chats.Add(new Chat { SenderId = senderId, ReceiverId = receiverId, Message = "hi" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReceivedMessages(receiverId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // The controller returns a list of anonymous objects, not ChatMessageDto
            var messagesList = okResult.Value as IEnumerable<object>;
            Assert.NotNull(messagesList);
            Assert.Single(messagesList);
            
            // Verify the structure of the returned anonymous object
            var firstMessage = messagesList.First();
            var messageType = firstMessage.GetType();
            var messageProperty = messageType.GetProperty("Message");
            Assert.NotNull(messageProperty);
            
            var messageValue = messageProperty.GetValue(firstMessage) as string;
            Assert.Equal("hi", messageValue);
        }

        [Fact]
        public async Task GetConversation_ValidUserIds_ReturnsConversation()
        {
            // Arrange
            var user1Id = Guid.Parse(_testUserId);
            var user2Id = Guid.NewGuid();
            _context.Chats.AddRange(
                new Chat { SenderId = user1Id, ReceiverId = user2Id, Message = "msg1" },
                new Chat { SenderId = user2Id, ReceiverId = user1Id, Message = "msg2" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetConversation(user1Id, user2Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var messages = Assert.IsAssignableFrom<IEnumerable<ChatMessageDto>>(okResult.Value);
            Assert.Equal(2, messages.Count());
        }

        [Fact]
        public async Task MarkAsRead_ValidChatId_ReturnsOk()
        {
            // Arrange
            var chat = new Chat { SenderId = Guid.NewGuid(), ReceiverId = Guid.Parse(_testUserId), Message = "read me", IsRead = false };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.MarkAsRead(chat.ChatId);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.True(await _context.Chats.AnyAsync(c => c.ChatId == chat.ChatId && c.IsRead == true));
        }

        [Fact]
        public async Task MarkAsRead_InvalidChatId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.MarkAsRead(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
