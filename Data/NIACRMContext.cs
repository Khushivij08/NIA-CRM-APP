using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NIA_CRM.Models;
using NIA_CRM.ViewModels;

namespace NIA_CRM.Data
{
    public class NIACRMContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string UserName { get; private set; }

        public NIACRMContext(DbContextOptions<NIACRMContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            UserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
        }

        public NIACRMContext(DbContextOptions<NIACRMContext> options)
            : base(options)
        {
            UserName = "Seed Data";
        }

        // DbSets for entities
        public DbSet<Member> Members { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Interaction> Interactions { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<Cancellation> Cancellations { get; set; }
        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<MemberMembershipType> MemberMembershipTypes { get; set; }
        public DbSet<ProductionEmail> ProductionEmails { get; set; }
        public DbSet<MemberLogo> MemberLogos { get; set; }
        public DbSet<MemberThumbnail> MemebrThumbnails { get; set; }
        public DbSet<ContactLogo> ContactLogos { get; set; }
        public DbSet<ContactThumbnail> ContactThumbnails { get; set; }
        public DbSet<IndustryNAICSCode> IndustryNAICSCodes { get; set; }
        public DbSet<NAICSCode> NAICSCodes { get; set; }
        public DbSet<MemberContact> MemberContacts { get; set; }
        public DbSet<MemberEvent> MemberEvents { get; set; }
        public DbSet<MEvent> MEvents { get; set; }
        public DbSet<MemberTag> MemberTags { get; set; }
        public DbSet<MTag> MTag { get; set; }
        public DbSet<AnnualAction> AnnualAction { get; set; }
        public DbSet<Strategy> Strategys { get; set; }
        public DbSet<MemberSector> MemberSectors { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<ContactCancellation> ContactCancellations { get; set; }
        public DbSet<WidgetLayout> WidgetLayouts { get; set; }
        public DbSet<AnnualAction> AnnualActions { get; set; }
        public DbSet<Strategy> Strategies  { get; set; }
        public DbSet<MTag> MTags { get; set; }

        public DbSet<DashboardLayout> DashboardLayouts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships and constraints

            // Member -> MembershipType (Many-to-Many)
            modelBuilder.Entity<MemberMembershipType>()
                .HasKey(mmt => new { mmt.MemberId, mmt.MembershipTypeId });

            modelBuilder.Entity<MemberMembershipType>()
                .HasOne(mmt => mmt.Member)
                .WithMany(m => m.MemberMembershipTypes)
                .HasForeignKey(mmt => mmt.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MemberMembershipType>()
                .HasOne(mmt => mmt.MembershipType)
                .WithMany(mt => mt.MemberMembershipTypes)
                .HasForeignKey(mmt => mmt.MembershipTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contact -> Member (Many-to-Many)
            modelBuilder.Entity<MemberContact>()
                .HasKey(mmt => new { mmt.MemberId, mmt.ContactId });

            modelBuilder.Entity<MemberContact>()
                .HasOne(mmt => mmt.Member)
                .WithMany(m => m.MemberContacts)
                .HasForeignKey(mmt => mmt.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MemberContact>()
                .HasOne(mmt => mmt.Contact)
                .WithMany(m => m.MemberContacts)
                .HasForeignKey(mmt => mmt.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

            // NAICSCode -> Member (Many-to-Many)
            modelBuilder.Entity<IndustryNAICSCode>()
                .HasKey(mmt => new { mmt.MemberId, mmt.NAICSCodeId });

            modelBuilder.Entity<IndustryNAICSCode>()
                .HasOne(mmt => mmt.Member)
                .WithMany(m => m.IndustryNAICSCodes)
                .HasForeignKey(mmt => mmt.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IndustryNAICSCode>()
                .HasOne(mmt => mmt.NAICSCode)
                .WithMany(m => m.IndustryNAICSCodes)
                .HasForeignKey(mmt => mmt.NAICSCodeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Address -> Member (One-to-Many)
            modelBuilder.Entity<Address>()
                .HasOne(a => a.Member)
                .WithOne(m => m.Address) // Changed from WithMany to WithOne
                .HasForeignKey<Address>(a => a.MemberId) // Foreign key remains the same
                .OnDelete(DeleteBehavior.Restrict); // Restricts deletion if referenced

            // Member -> Cancellation (One-to-Many)
            modelBuilder.Entity<Cancellation>()
                .HasOne(c => c.Member)
                .WithMany(m => m.Cancellations)
                .HasForeignKey(c => c.MemberID)
                .OnDelete(DeleteBehavior.Restrict);

            // Contact -> ContactCancellation (One-to-Many)
            modelBuilder.Entity<ContactCancellation>()
                .HasOne(c => c.Contact)
                .WithMany(m => m.ContactCancellations)
                .HasForeignKey(c => c.ContactID)
                .OnDelete(DeleteBehavior.Restrict);

            // Member -> Event (Many-to-Many)
            modelBuilder.Entity<MemberEvent>()
                .HasKey(mmt => new { mmt.MemberId, mmt.MEventID });

            modelBuilder.Entity<MemberEvent>()
                .HasOne(mmt => mmt.Member)
                .WithMany(m => m.MemberEvents)
                .HasForeignKey(mmt => mmt.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MemberEvent>()
                .HasOne(mmt => mmt.MEvent)
                .WithMany(m => m.MemberEvents)
                .HasForeignKey(mmt => mmt.MEventID)
                .OnDelete(DeleteBehavior.Cascade);

            // Member -> Sector (Many-to-Many)
            modelBuilder.Entity<MemberSector>()
                .HasKey(mmt => new { mmt.MemberId, mmt.SectorId });

            modelBuilder.Entity<MemberSector>()
                .HasOne(mmt => mmt.Member)
                .WithMany(m => m.MemberSectors)
                .HasForeignKey(mmt => mmt.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MemberSector>()
                .HasOne(mmt => mmt.Sector)
                .WithMany(m => m.MemberSectors)
                .HasForeignKey(mmt => mmt.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Member -> Tag (Many-to-Many)
            modelBuilder.Entity<MemberTag>()
                .HasKey(mmt => new { mmt.MemberId, mmt.MTagID });

            modelBuilder.Entity<MemberTag>()
                .HasOne(mmt => mmt.Member)
                .WithMany(m => m.MemberTags)
                .HasForeignKey(mmt => mmt.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MemberTag>()
                .HasOne(mmt => mmt.MTag)
                .WithMany(m => m.MemberTags)
                .HasForeignKey(mmt => mmt.MTagID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure the many-to-many relationship between Member and Contact via MemberContact
            modelBuilder.Entity<MemberContact>()
                .HasOne(mc => mc.Member)
                .WithMany(m => m.MemberContacts)
                .HasForeignKey(mc => mc.MemberId)
                .OnDelete(DeleteBehavior.Cascade); // or another delete behavior like Restrict

            modelBuilder.Entity<MemberContact>()
                .HasOne(mc => mc.Contact)
                .WithMany(c => c.MemberContacts)
                .HasForeignKey(mc => mc.ContactId)
                .OnDelete(DeleteBehavior.Cascade); // or another delete behavior

            //Member Name is unique
            modelBuilder.Entity<Member>()
            .HasIndex(p => p.MemberName)
            .IsUnique();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }
        
       
    }
}
