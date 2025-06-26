using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SinalR.Areas.Identity.Data;
using SinalR.Models;

namespace SinalR.Areas.Identity.Data;

public class SDbContext : IdentityDbContext<AppUser>
{
    public SDbContext(DbContextOptions<SDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

    }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<DeletedContact> DeletedContacts { get; set; }



}
