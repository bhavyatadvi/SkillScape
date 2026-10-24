using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillScape.Application.DTOs;
using SkillScape.Infrastructure.Data;
using System.Security.Claims;
using System.Text;

namespace SkillScape.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    public class SendChatMessageRequest
    {
        public string ReceiverId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }

    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<object>>> SendMessage([FromBody] SendChatMessageRequest request)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            if (string.IsNullOrWhiteSpace(request.ReceiverId) ||
                (string.IsNullOrWhiteSpace(request.Message) && string.IsNullOrWhiteSpace(request.ImageUrl)))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("ReceiverId and message or image is required"));
            }

            var chatMessage = new SkillScape.Domain.Entities.ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                SenderId = currentUserId,
                ReceiverId = request.ReceiverId,
                Content = request.Message,
                ImageUrl = request.ImageUrl,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var responseData = new
            {
                chatMessage.Id,
                chatMessage.SenderId,
                chatMessage.ReceiverId,
                Message = chatMessage.Content,
                chatMessage.ImageUrl,
                Timestamp = chatMessage.SentAt,
                chatMessage.IsRead
            };

            return Ok(ApiResponse<object>.SuccessResponse(responseData, "Message sent"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("history/{otherUserId}")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetChatHistory(string otherUserId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var messages = await _context.ChatMessages
                .Where(m => 
                    (m.SenderId == currentUserId && m.ReceiverId == otherUserId) || 
                    (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(m => new {
                    m.Id,
                    m.SenderId,
                    m.ReceiverId,
                    m.Content,
                    m.ImageUrl,
                    m.SentAt,
                    m.IsRead
                })
                .ToListAsync();

            // Return in chronological order
            messages.Reverse();

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(messages, "Chat history retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("history/{otherUserId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteChatHistory(string otherUserId)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var messages = await _context.ChatMessages
                .Where(m => 
                    (m.SenderId == currentUserId && m.ReceiverId == otherUserId) || 
                    (m.SenderId == otherUserId && m.ReceiverId == currentUserId))
                .ToListAsync();

            if (!messages.Any())
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Chat history is already empty"));

            _context.ChatMessages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Chat history deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("mark-read/{otherUserId}")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(string otherUserId)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == currentUserId && !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Messages marked as read"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetConversations()
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // Fetch all messages involving the current user
            var messages = await _context.ChatMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .ToListAsync();

            // Group by the *other* user in the conversation
            var conversations = messages
                .GroupBy(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Select(g => {
                    var latestMessage = g.OrderByDescending(m => m.SentAt).First();
                    var otherUserId = g.Key;
                    var otherUser = latestMessage.SenderId == currentUserId ? latestMessage.Receiver : latestMessage.Sender;
                    
                    return new {
                        Id = otherUserId,
                        OtherUserId = otherUserId,
                        OtherUserName = otherUser?.FullName ?? "Unknown User",
                        OtherUserAvatar = otherUser?.ProfileImageUrl,
                        LastMessage = latestMessage.Content == "Sent an image" || !string.IsNullOrEmpty(latestMessage.ImageUrl) ? "📷 Image" : latestMessage.Content,
                        LastMessageTime = latestMessage.SentAt,
                        UnreadCount = g.Count(m => m.ReceiverId == currentUserId && !m.IsRead)
                    };
                })
                .OrderByDescending(c => c.LastMessageTime)
                .ToList<object>();

            return Ok(ApiResponse<List<object>>.SuccessResponse(conversations, "Conversations retrieved"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<List<object>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("conversation")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetConversationAlias()
    {
        return await GetConversations();
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded"));

            const long maxImageSize = 5 * 1024 * 1024;
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (file.Length > maxImageSize ||
                string.IsNullOrEmpty(extension) ||
                !allowedExtensions.Contains(extension) ||
                !allowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(
                    "Chat images must be JPG, PNG, WebP, or GIF files no larger than 5 MB."));
            }
            if (!await HasValidImageSignatureAsync(file, extension))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("The uploaded file is not a valid image."));
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            // Simple local upload for now (could connect to S3/Cloudinary later)
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/chat/{fileName}";
            return Ok(ApiResponse<string>.SuccessResponse(fileUrl, "Image uploaded successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Image upload failed."));
        }
    }

        private static async Task<bool> HasValidImageSignatureAsync(IFormFile file, string? extension)
        {
            await using var stream = file.OpenReadStream();
            var header = new byte[12];
            var read = await stream.ReadAsync(header);
            return extension switch
            {
                ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
                ".png" => read >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                ".webp" => read >= 12 && Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
                           Encoding.ASCII.GetString(header, 8, 4) == "WEBP",
                ".gif" => read >= 6 && (Encoding.ASCII.GetString(header, 0, 6) == "GIF87a" ||
                                        Encoding.ASCII.GetString(header, 0, 6) == "GIF89a"),
                _ => false
            };
        }
    }
