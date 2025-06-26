using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using SinalR.Areas.Identity.Data;
using SinalR.Models;

public class ChatHub : Hub
{
    private readonly SDbContext _context;

    public ChatHub(SDbContext context)
    {
        _context = context;
    }

    // Store active users: Email -> ConnectionId
    private static readonly Dictionary<string, string> _emailToConnectionMap = new();

    public override async Task OnConnectedAsync()
    {
        var email = Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(email))
        {
            _emailToConnectionMap[email] = Context.ConnectionId;
            await Clients.All.SendAsync("UserOnlineStatusChanged", email, true);
        }

        await base.OnConnectedAsync();
    }
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var email = Context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(email))
        {
            _emailToConnectionMap.Remove(email);
            await Clients.All.SendAsync("UserOnlineStatusChanged", email, false);
        }

        await base.OnDisconnectedAsync(exception);
    }
    public async Task CheckIfUserOnline(string? emailToCheck)
    {
        if (_emailToConnectionMap.ContainsKey(emailToCheck))
        {
            await Clients.Caller.SendAsync("UserOnlineStatus", emailToCheck, true);
        }
        else
        {
            await Clients.Caller.SendAsync("UserOnlineStatus", emailToCheck, false);
        }
    }
    public async Task MarkAsSeen(string senderEmail)
    {
        var receiverEmail = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(receiverEmail) || string.IsNullOrEmpty(senderEmail))
            return;

        // Step 1: Update IsSeen in the database
        var unseenMessages = _context.Messages
            .Where(m => m.SenderEmail == senderEmail && m.ReceiverEmail == receiverEmail && !m.IsSeen)
            .ToList();

        foreach (var msg in unseenMessages)
        {
            msg.IsSeen = true;
        }

        await _context.SaveChangesAsync();

        // Step 2: Notify sender in real-time
        if (_emailToConnectionMap.TryGetValue(senderEmail, out var senderConnId))
        {
            await Clients.Client(senderConnId).SendAsync("MessagesSeen", receiverEmail);
        }
    }

    public async Task Typing(string receiverEmail)
    {
        var senderEmail = Context.User?.Identity?.Name;
        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(receiverEmail)) return;

        if (_emailToConnectionMap.TryGetValue(receiverEmail, out var receiverConnId))
        {
            await Clients.Client(receiverConnId).SendAsync("UserTyping", senderEmail);
        }
    }

    public async Task StopTyping(string receiverEmail)
    {
        var senderEmail = Context.User?.Identity?.Name;
        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(receiverEmail)) return;

        if (_emailToConnectionMap.TryGetValue(receiverEmail, out var receiverConnId))
        {
            await Clients.Client(receiverConnId).SendAsync("UserStoppedTyping", senderEmail);
        }
    }






    public async Task SendMessageToEmail(string receiverEmail, string message, string messageId)
    {
        var senderEmail = Context.User?.Identity?.Name;

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(receiverEmail)) return;

        // 1. Save the message in the database
        var chatMessage = new Message
        {
            SenderEmail = senderEmail,
            ReceiverEmail = receiverEmail,
            Content = message,
            Timestamp = DateTime.Now,
            IsDelivered = true
        };

        _context.Messages.Add(chatMessage);

        // 2. Ensure receiver is in sender's contact list
        var contact = _context.Contacts.FirstOrDefault(c =>
            c.UserEmail == senderEmail && c.ContactEmail == receiverEmail);

        if (contact == null)
        {
            _context.Contacts.Add(new Contact
            {
                UserEmail = senderEmail,
                ContactEmail = receiverEmail,
                ContactName = receiverEmail.Split('@')[0]
            });
        }
        else if (contact.DeletedAt != null)
        {
            contact.DeletedAt = null;
            _context.Contacts.Update(contact);
        }

        // 3. Ensure sender is in receiver's contact list
        var reverseContact = _context.Contacts.FirstOrDefault(c =>
            c.UserEmail == receiverEmail && c.ContactEmail == senderEmail);

        if (reverseContact == null)
        {
            _context.Contacts.Add(new Contact
            {
                UserEmail = receiverEmail,
                ContactEmail = senderEmail,
                ContactName = senderEmail.Split('@')[0]
            });
        }
        else if (reverseContact.DeletedAt != null)
        {
            reverseContact.DeletedAt = null;
            _context.Contacts.Update(reverseContact);
        }

        await _context.SaveChangesAsync();

        // 4. Acknowledge delivery to sender
        await Clients.Caller.SendAsync("MessageDelivered", receiverEmail, message, messageId);

        // 5. Send to receiver if online
        if (_emailToConnectionMap.TryGetValue(receiverEmail, out var receiverConnectionId))
        {
            await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", senderEmail, message);
        }
    }


}

