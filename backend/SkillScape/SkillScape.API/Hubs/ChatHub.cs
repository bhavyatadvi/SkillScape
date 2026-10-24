using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Data;
using System.Security.Claims;

namespace SkillScape.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Group name helper
    private string GetChatRoomName(string userA, string userB)
    {
        var sortedIds = new[] { userA, userB }.OrderBy(id => id).ToArray();
        return $"{sortedIds[0]}-{sortedIds[1]}";
    }

    public async Task SyncRoomChat(string circleId, object chatMessage)
    {
        await Clients.Group(circleId).SendAsync("ReceiveRoomChat", chatMessage);
    }

    public async Task JoinChat(string otherUserId)
    {
        var currentUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null) return;

        var roomName = GetChatRoomName(currentUserId, otherUserId);
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
    }

    public async Task LeaveChat(string otherUserId)
    {
        var currentUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null) return;

        var roomName = GetChatRoomName(currentUserId, otherUserId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
    }

    public async Task SendMessage(string receiverId, string content, string? imageUrl = null)
    {
        var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chat_debug_log.txt");
        
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        System.IO.File.AppendAllText(logFile, $"\n[{DateTime.UtcNow}] SendMessage Triggered! sender={senderId}, receiver={receiverId}, content={content}");

        if (senderId == null) 
        {
            System.IO.File.AppendAllText(logFile, " - FAILED: senderId is null");
            return;
        }

        try
        {
            // Persist to DB
            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                ImageUrl = imageUrl,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            System.IO.File.AppendAllText(logFile, " - DB Save Success");

            var sender = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == senderId);

            var senderName = sender?.FullName ?? "Someone";
            var notificationMessage = !string.IsNullOrWhiteSpace(content)
                ? content
                : "Sent an image";

            var dbNotification = new Notification
            {
                UserId = receiverId,
                Type = "Message",
                Title = $"New message from {senderName}",
                Message = notificationMessage,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(dbNotification);
            await _context.SaveChangesAsync();
            System.IO.File.AppendAllText(logFile, " - Notification Save Success");

            // Broadcast to the room
            var roomName = GetChatRoomName(senderId, receiverId);
            await Clients.Group(roomName).SendAsync("ReceiveMessage", message);
            System.IO.File.AppendAllText(logFile, " - Broadcast Success");
        }
        catch (Exception ex)
        {
            var errorLog = $"SendMessage Error: {ex.Message}\nStack: {ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorLog += $"\nInner: {ex.InnerException.Message}";
            }
            System.IO.File.WriteAllText("chat_error_log.txt", errorLog);
            throw;
        }
        
        // Also fire a general notification if the user is online but not in the specific chat room
        // Assuming we map ConnectionIds to UserIds in a concurrent dictionary (simplified approach here is group by ID)
        await Clients.Group(receiverId).SendAsync("ReceiveNotification", new 
        {
            Title = "New Message",
            Message = content,
            ImageUrl = imageUrl,
            Type = "chat",
            SenderId = senderId,
            DbNotificationType = "Message"
        });
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }
    
    // WebRTC Signaling
    public async Task JoinVideoRoom(string circleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"video_{circleId}");
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
        // Notify others in room
        await Clients.Group($"video_{circleId}").SendAsync("UserJoinedVideo", Context.ConnectionId, userId, userName);
    }
    
    public async Task LeaveVideoRoom(string circleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video_{circleId}");
        await Clients.Group($"video_{circleId}").SendAsync("UserLeftVideo", Context.ConnectionId);
    }
    
    public async Task SendWebRTCSignal(string targetConnectionId, string signalData)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveWebRTCSignal", Context.ConnectionId, signalData);
    }
    
    public async Task EndVideoSession(string circleId)
    {
        await Clients.Group($"video_{circleId}").SendAsync("SessionEnded");
        await Clients.Group(circleId).SendAsync("SessionEnded");
    }

    public async Task JoinCircleGroup(string circleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, circleId);
    }

    public async Task LeaveCircleGroup(string circleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, circleId);
    }

    public async Task SyncWhiteboard(string circleId, string content)
    {
        await Clients.Group(circleId).SendAsync("ReceiveWhiteboard", content, Context.ConnectionId);
    }
}
