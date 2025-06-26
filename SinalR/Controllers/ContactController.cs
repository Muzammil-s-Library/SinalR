using Microsoft.AspNetCore.Mvc;
using SinalR.Areas.Identity.Data;
using SinalR.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace SinalR.Controllers
{
    public class ContactController : Controller
    {
        private readonly SDbContext _dbcontext;
        private readonly UserManager<AppUser> _userManager;

        // Constructor with dependency injection for UserManager and DbContext
        public ContactController(SDbContext dbContext, UserManager<AppUser> userManager)
        {
            _dbcontext = dbContext;
            _userManager = userManager;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddContact(Contact contact)
        {

            // Get the logged-in user's email
            var userEmail = User.Identity.Name;

            // Set the UserEmail field of the Contact
            contact.UserEmail = userEmail;
            var ExistingEmail = _dbcontext.Contacts.Where(e => e.ContactEmail == contact.ContactEmail && e.UserEmail==userEmail);
            if (ExistingEmail.IsNullOrEmpty())
            {
                _dbcontext.Contacts.Add(contact);
                await _dbcontext.SaveChangesAsync();
                return RedirectToAction("Index", "Home");

            }
            TempData["ErrorMessage"] = "This email is already in Contact.";
            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> DeleteContact(string sender, string receiver)
        {
            if (string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(receiver))
                return BadRequest("Invalid sender or receiver.");

            var contact = await _dbcontext.Contacts
                .FirstOrDefaultAsync(c => c.UserEmail == sender && c.ContactEmail == receiver);

            if (contact != null && contact.DeletedAt == null)
            {
                contact.DeletedAt = DateTime.UtcNow;
                _dbcontext.Contacts.Update(contact);
                await _dbcontext.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }



    }
}
