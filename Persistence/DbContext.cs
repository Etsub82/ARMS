using Domain;
using Domain.common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Persistence
{
    public class ARMSDbContext : DbContext
    {
        public readonly DbContextOptions<ARMSDbContext> _context;
        

        public ARMSDbContext(DbContextOptions<ARMSDbContext> options) : base(options)
        {
            _context = options;
        }

        public DbSet<ApplicationModel> ApplicationModels { get; set; }
        public DbSet<ApplicationGroup> ApplicationGroups { get; set; }
        public DbSet<RoleModel> RoleModels { get; set; }
        public DbSet<GroupRole> GroupRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseDomainEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseDomainEntity.Id))
                        .ValueGeneratedOnAdd();

                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseDomainEntity.DateCreated))
                        .HasDefaultValue(DateTime.UtcNow);

                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseDomainEntity.LastModifiedDate))
                        .HasDefaultValue(DateTime.UtcNow);
                }
            }
            modelBuilder.Entity<GroupRole>()
                .HasKey(gr => new { gr.ApplicationGroupId, gr.RoleModelId });

            modelBuilder.Entity<GroupRole>()
                .HasOne(gr => gr.ApplicationGroup)
                .WithMany(ag => ag.GroupRoles)
                .HasForeignKey(gr => gr.ApplicationGroupId);

            modelBuilder.Entity<GroupRole>()
                .HasOne(gr => gr.RoleModel)
                .WithMany(rm => rm.GroupRoles)
                .HasForeignKey(gr => gr.RoleModelId);
        }


        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            foreach (var entity in ChangeTracker.Entries<BaseDomainEntity>())
            {
                entity.Entity.LastModifiedDate = DateTime.Now;
                if (entity.State == EntityState.Added)
                {
                    entity.Entity.DateCreated = DateTime.Now;
                }
            }
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }



    }


}