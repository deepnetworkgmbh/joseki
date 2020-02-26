using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using joseki.db.entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace joseki.db
{
    /// <summary>
    /// Joseki database EF context.
    /// </summary>
    public class JosekiDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JosekiDbContext"/> class.
        /// </summary>
        /// <param name="options">Context options.</param>
        public JosekiDbContext(DbContextOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Audit table.
        /// </summary>
        public DbSet<AuditEntity> Audit { get; set; }

        /// <summary>
        /// Check table.
        /// </summary>
        public DbSet<CheckEntity> Check { get; set; }

        /// <summary>
        /// Check Result table.
        /// </summary>
        public DbSet<CheckResultEntity> CheckResult { get; set; }

        /// <summary>
        /// Azure Metadata table.
        /// </summary>
        public DbSet<MetadataAzureEntity> MetadataAzure { get; set; }

        /// <summary>
        /// Kubernetes Metadata table.
        /// </summary>
        public DbSet<MetadataKubeEntity> MetadataKube { get; set; }

        /// <summary>
        /// CVE table.
        /// </summary>
        public DbSet<CveEntity> Cve { get; set; }

        /// <summary>
        /// Image Scan Result table.
        /// </summary>
        public DbSet<ImageScanResultEntity> ImageScanResult { get; set; }

        /// <summary>
        /// Image Scan Result to Cve table.
        /// </summary>
        public DbSet<ImageScanToCveEntity> ImageScanResultToCve { get; set; }

        /// <inheritdoc />
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            this.SetCreatedDate();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Audit

            modelBuilder.Entity<AuditEntity>().HasKey(audit => audit.Id);
            modelBuilder.Entity<AuditEntity>().Property(audit => audit.ScannerId).IsRequired();
            modelBuilder.Entity<AuditEntity>().Property(audit => audit.AuditId).IsRequired();
            modelBuilder.Entity<AuditEntity>()
                .HasIndex(audit => new { audit.Date, audit.ScannerId })
                .IncludeProperties(audit => audit.AuditId)
                .IsUnique();

            #endregion

            #region Check

            modelBuilder.Entity<CheckEntity>().HasKey(check => check.Id);
            modelBuilder.Entity<CheckEntity>().Property(check => check.CheckId).IsRequired();
            modelBuilder.Entity<CheckEntity>().Property(check => check.Severity).HasConversion(new EnumToStringConverter<CheckSeverity>());
            modelBuilder.Entity<CheckEntity>().HasIndex(check => check.CheckId).IsUnique();

            #endregion

            #region Check Result

            modelBuilder.Entity<CheckResultEntity>().HasKey(checkResult => checkResult.Id);
            modelBuilder.Entity<CheckResultEntity>().Property(checkResult => checkResult.ComponentId).IsRequired();
            modelBuilder.Entity<CheckResultEntity>().Property(checkResult => checkResult.Value).HasConversion(new EnumToStringConverter<CheckValue>());
            modelBuilder.Entity<CheckResultEntity>()
                .HasOne(checkResult => checkResult.Audit)
                .WithMany(audit => audit.CheckResults)
                .HasForeignKey(checkResult => checkResult.AuditId)
                .IsRequired();

            // TODO: maybe add a computed column which indicates if a record is image-scan? It might make index even faster.
            modelBuilder.Entity<CheckResultEntity>()
                .HasIndex(checkResult => new { checkResult.ComponentId, checkResult.Value })
                .HasFilter("[VALUE] = 'InProgress'");

            #endregion

            #region Azure Metadata

            modelBuilder.Entity<MetadataAzureEntity>().HasKey(metadata => metadata.Id);
            modelBuilder.Entity<MetadataAzureEntity>().Property(metadata => metadata.JSON).IsRequired();
            modelBuilder.Entity<MetadataAzureEntity>()
                .HasOne(metadata => metadata.Audit)
                .WithOne(audit => audit.MetadataAzure)
                .HasForeignKey<MetadataAzureEntity>(metadata => metadata.AuditId)
                .IsRequired(false);

            #endregion

            #region Kube Metadata

            modelBuilder.Entity<MetadataKubeEntity>().HasKey(metadata => metadata.Id);
            modelBuilder.Entity<MetadataKubeEntity>().Property(metadata => metadata.JSON).IsRequired();
            modelBuilder.Entity<MetadataKubeEntity>()
                .HasOne(metadata => metadata.Audit)
                .WithOne(audit => audit.MetadataKube)
                .HasForeignKey<MetadataKubeEntity>(metadata => metadata.AuditId)
                .IsRequired(false);

            #endregion

            #region CVE

            modelBuilder.Entity<CveEntity>().HasKey(cve => cve.Id);
            modelBuilder.Entity<CveEntity>().Property(cve => cve.CveId).IsRequired();
            modelBuilder.Entity<CveEntity>().Property(cve => cve.PackageName).IsRequired();
            modelBuilder.Entity<CveEntity>().Property(cve => cve.Severity).HasConversion(new EnumToStringConverter<CveSeverity>()).IsRequired();
            modelBuilder.Entity<CveEntity>().HasIndex(cve => cve.CveId).IsUnique();

            #endregion

            #region Image Scan Result

            modelBuilder.Entity<ImageScanResultEntity>().HasKey(scan => scan.Id);
            modelBuilder.Entity<ImageScanResultEntity>().Property(scan => scan.ExternalId).IsRequired();
            modelBuilder.Entity<ImageScanResultEntity>().Property(scan => scan.ImageTag).IsRequired();
            modelBuilder.Entity<ImageScanResultEntity>().Property(scan => scan.Status).HasConversion(new EnumToStringConverter<ImageScanStatus>());
            modelBuilder.Entity<ImageScanResultEntity>()
                .HasIndex(scan => new { scan.ImageTag, scan.Date })
                .IsUnique();

            #endregion

            #region Image Scan to CVE

            modelBuilder.Entity<ImageScanToCveEntity>().HasKey(scan2cve => scan2cve.Id);
            modelBuilder.Entity<ImageScanToCveEntity>().Property(scan2cve => scan2cve.Target).IsRequired();
            modelBuilder.Entity<ImageScanToCveEntity>().Property(scan2cve => scan2cve.UsedPackageVersion).IsRequired();
            modelBuilder.Entity<ImageScanToCveEntity>()
                .HasOne(scan2cve => scan2cve.ImageScan)
                .WithMany(scan => scan.FoundCVEs)
                .HasForeignKey(scan2cve => scan2cve.ScanId)
                .IsRequired();
            modelBuilder.Entity<ImageScanToCveEntity>()
                .HasOne(scan2cve => scan2cve.CVE)
                .WithMany()
                .HasForeignKey(scan2cve => scan2cve.CveId)
                .IsRequired();

            #endregion
        }

        private void SetCreatedDate()
        {
            var items = this.ChangeTracker
                .Entries()
                .Where(x => x.Entity is IJosekiBaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified))
                .Select(x => (state: x.State, entity: (IJosekiBaseEntity)x.Entity))
                .ToArray();

            var now = DateTime.UtcNow;
            foreach (var (state, entity) in items)
            {
                if (state == EntityState.Added)
                {
                    entity.DateCreated = now;
                }

                entity.DateUpdated = now;
            }
        }

        /// <summary>
        /// JosekiDbContextFactory used to update the database with the entity framework during design time
        /// (update-database).
        /// </summary>
        public class JosekiDbContextFactory : IDesignTimeDbContextFactory<JosekiDbContext>
        {
            private const string ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=local-joseki-db;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            /// <inheritdoc />
            public JosekiDbContext CreateDbContext(string[] args)
            {
                var builder = new DbContextOptionsBuilder<JosekiDbContext>();
                builder.UseSqlServer(ConnectionString, optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(JosekiDbContext).GetTypeInfo().Assembly.GetName().Name));

                return new JosekiDbContext(builder.Options);
            }
        }
    }
}