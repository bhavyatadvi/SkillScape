using System;

namespace SkillScape.Domain.Entities;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    
    // Virtual navigations (optional mapping)
    public virtual ApplicationUser? Sender { get; set; }
    public virtual ApplicationUser? Receiver { get; set; }
}
