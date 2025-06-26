using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SinalR.Areas.Identity.Data;
using SinalR.Models;

namespace SinalR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SDbContext _dbcontext;
        private readonly UserManager<AppUser> _userManager;

        // Constructor with dependency injection for UserManager and DbContext
   
        public HomeController(ILogger<HomeController> logger, SDbContext dbContext, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _dbcontext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? sender, string? receiver)
        {
            var userEmail = User.Identity.Name;

            // Get all contacts saved by the user
            var contacts = await _dbcontext.Contacts
      .Where(c => c.UserEmail == userEmail && c.DeletedAt == null)
      .ToListAsync();


            // Get all unique emails that messaged the user (and aren't already in contacts)
            // First, get all deleted contacts
            // 1. Load deleted contacts to memory
            var deletedContacts = await _dbcontext.Contacts
                .Where(c => c.UserEmail == userEmail && c.DeletedAt != null)
                .ToListAsync();

            // 2. Get all messages to the user (filtered in-memory based on deleted contacts)
            var allMessages = await _dbcontext.Messages
                .Where(m => m.ReceiverEmail == userEmail)
                .ToListAsync();

            // 3. Filter messages: either from new senders or sent after DeletedAt
            var filteredSenderEmails = allMessages
                .Where(m =>
                {
                    var deleted = deletedContacts.FirstOrDefault(dc => dc.ContactEmail == m.SenderEmail);
                    return deleted == null || m.Timestamp > deleted.DeletedAt;
                })
                .Select(m => m.SenderEmail)
                .Distinct()
                .ToList();


            // Add implicit contacts (not already saved) from incoming messages
            var existingContacts = await _dbcontext.Contacts
         .Where(c => c.UserEmail == userEmail)
         .ToListAsync();

            var implicitContacts = new List<Contact>();

            foreach (var email in filteredSenderEmails)
            {
                var existing = existingContacts.FirstOrDefault(c => c.ContactEmail == email);

                if (existing == null)
                {
                    // New contact, add and save to database
                    var newContact = new Contact
                    {
                        ContactName = email.Split('@')[0],
                        ContactEmail = email,
                        UserEmail = userEmail
                    };

                    _dbcontext.Contacts.Add(newContact);
                    implicitContacts.Add(newContact);
                }
               
            }


            await _dbcontext.SaveChangesAsync();


            // Combine both contact lists
            var allContacts = contacts.Concat(implicitContacts).ToList();

            // Get last messages for any conversation involving the user
            var lastMessages = await _dbcontext.Messages
                .Where(m => m.SenderEmail == userEmail || m.ReceiverEmail == userEmail)
                .GroupBy(m => m.SenderEmail == userEmail ? m.ReceiverEmail : m.SenderEmail)
                .Select(g => g.OrderByDescending(m => m.Timestamp).FirstOrDefault())
                .ToListAsync();
            // Get count of unseen messages grouped by sender
            var unseenCounts = await _dbcontext.Messages
                .Where(m => m.ReceiverEmail == userEmail && !m.IsSeen)
                .GroupBy(m => m.SenderEmail)
                .Select(g => new { SenderEmail = g.Key, Count = g.Count() })
                .ToListAsync();

            // Merge into ViewModel
            var contactViews = allContacts.Select(contact =>
            {
                var lastMsg = lastMessages.FirstOrDefault(m =>
                    (m.SenderEmail == userEmail && m.ReceiverEmail == contact.ContactEmail) ||
                    (m.ReceiverEmail == userEmail && m.SenderEmail == contact.ContactEmail));

                var unseenCount = unseenCounts
                    .FirstOrDefault(u => u.SenderEmail == contact.ContactEmail)?.Count ?? 0;

                return new ContactViewModel
                {
                    ContactName = contact.ContactName,
                    ContactEmail = contact.ContactEmail,
                    UserEmail = contact.UserEmail,
                    LastMessage = lastMsg?.Content ?? "",
                    LastMessageTime = lastMsg?.Timestamp,
                    UnseenCount = unseenCount
                };
            }).ToList();


            var messages = new List<Message>();
            if (!string.IsNullOrEmpty(sender) && !string.IsNullOrEmpty(receiver))
            {
                var contact = await _dbcontext.Contacts
                    .FirstOrDefaultAsync(c =>
                        c.UserEmail == sender && c.ContactEmail == receiver);

                DateTime? deletedAt = contact?.DeletedAt;

                messages = await _dbcontext.Messages
                    .Where(m =>
                        (
                            (m.SenderEmail == sender && m.ReceiverEmail == receiver) ||
                            (m.SenderEmail == receiver && m.ReceiverEmail == sender)
                        ) &&
                        (deletedAt == null || m.Timestamp > deletedAt))
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }


            var viewModel = new ChatViewModel
            {
                Contacts = contactViews,
                Messages = messages,
                SenderEmail = sender,
                ReceiverEmail = receiver
            };

            return View(viewModel);
        }





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
