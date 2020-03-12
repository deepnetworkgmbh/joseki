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

        /// <summary>
        /// Knowledge-base table.
        /// </summary>
        public DbSet<KnowledgebaseEntity> Knowledgebase { get; set; }

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
            modelBuilder.Entity<AuditEntity>().Property(audit => audit.ComponentId).IsRequired().HasDefaultValue("undefined-id");
            modelBuilder.Entity<AuditEntity>().Property(audit => audit.ComponentName).IsRequired().HasDefaultValue("undefined-name");
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
            modelBuilder.Entity<CheckResultEntity>()
                .HasOne(checkResult => checkResult.Check)
                .WithMany()
                .HasForeignKey(checkResult => checkResult.CheckId)
                .IsRequired();

            // TODO: maybe add a computed column which indicates if a record is image-scan? It might make index even faster.
            modelBuilder.Entity<CheckResultEntity>()
                .HasIndex(checkResult => new { checkResult.ComponentId, checkResult.Value })
                .HasFilter("[VALUE] = 'InProgress'");

            #endregion

            #region Azure Metadata

            modelBuilder.Entity<MetadataAzureEntity>().HasKey(metadata => metadata.Id);
            modelBuilder.Entity<MetadataAzureEntity>()
                .Property(metadata => metadata.JSON)
                .HasColumnType("text")
                .IsRequired();
            modelBuilder.Entity<MetadataAzureEntity>()
                .HasOne(metadata => metadata.Audit)
                .WithOne(audit => audit.MetadataAzure)
                .HasForeignKey<MetadataAzureEntity>(metadata => metadata.AuditId)
                .IsRequired(false);

            #endregion

            #region Kube Metadata

            modelBuilder.Entity<MetadataKubeEntity>().HasKey(metadata => metadata.Id);
            modelBuilder.Entity<MetadataKubeEntity>()
                .Property(metadata => metadata.JSON)
                .HasColumnType("text")
                .IsRequired();
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
            modelBuilder.Entity<ImageScanToCveEntity>().Property(scan2cve => scan2cve.UsedPackage).IsRequired();
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

            #region Knowledge-base

            modelBuilder.Entity<KnowledgebaseEntity>().HasKey(item => item.Id);
            modelBuilder.Entity<KnowledgebaseEntity>().HasIndex(item => item.ItemId).IsUnique();
            var now = new DateTime(2020, 3, 9, 12, 56, 38, 559, DateTimeKind.Utc).AddTicks(793);
            modelBuilder.Entity<KnowledgebaseEntity>().HasData(
                new KnowledgebaseEntity { Id = 1, DateCreated = now, DateUpdated = now, ItemId = "polaris.security", Content = "Kubernetes provides a great deal of configurability when it comes to the security of your workloads. A key principle here involves limiting the level of access any individual workload has. Polaris has validations for a number of best practices, mostly focused on ensuring that unnecessary access has not been granted to an application workload." },
                new KnowledgebaseEntity { Id = 2, DateCreated = now, DateUpdated = now, ItemId = "polaris.networking", Content = "Networking configuration in Kubernetes can be quite powerful. Polaris validates that pods are not configured to have access to sensitive host networking configuration. There are certain use cases such as a container overlay network like Calico, where this level of access is required, but the majority of workloads running on Kubernetes should not need this." },
                new KnowledgebaseEntity { Id = 3, DateCreated = now, DateUpdated = now, ItemId = "polaris.resources", Content = "Configuring resource requests and limits for workloads running in Kubernetes helps ensure that every container will have access to all the resources it needs. These are also a crucial part of cluster autoscaling logic, as new nodes are only spun up when there is insufficient capacity on existing infrastructure for new pod(s). By default, Polaris validates that resource requests and limits are set, it also includes optional functionality to ensure these requests and limits fall within specified ranges." },
                new KnowledgebaseEntity { Id = 4, DateCreated = now, DateUpdated = now, ItemId = "polaris.healthchecks", Content = "Properly configured health checks can ensure the long term availability and reliability of your application running in Kubernetes. Polaris validates that health checks are configured for each pod running in your cluster." },
                new KnowledgebaseEntity { Id = 5, DateCreated = now, DateUpdated = now, ItemId = "polaris.images", Content = "Images are the backbone of any Kubernetes cluster, containing the applications that run in each container. Polaris validates that images are configured with specific tags instead of just pulling the latest image on each run. This is important for the stability and security of your workloads." },
                new KnowledgebaseEntity { Id = 6, DateCreated = now, DateUpdated = now, ItemId = "azsk.analysisservices", Content = "A set of verifications related to Azure Analysis services. It enforces: sensitive data encryption, proper RBAC and AAD configuration, backup setup. the entire list is at https://aka.ms/azsktcp/analysisservices" },
                new KnowledgebaseEntity { Id = 7, DateCreated = now, DateUpdated = now, ItemId = "azsk.apiconnection", Content = "Verifies if Logic App connectors use AAD-based authentication and data transit across connectors use encrypted channel. The entire list is at https://aka.ms/azsktcp/logicapps" },
                new KnowledgebaseEntity { Id = 8, DateCreated = now, DateUpdated = now, ItemId = "azsk.apimanagement", Content = "Group of checks for Azure API Management service: RBAC, alerts, enabled diagnostic-logs, TLS configuration, secrets management and others. The entire list is at https://aka.ms/azsktcp/apim" },
                new KnowledgebaseEntity { Id = 9, DateCreated = now, DateUpdated = now, ItemId = "azsk.appservice", Content = "Verifies if Azure App Service is properly configured: SSL, Active directory configuration, secrets management, webhooks, alerts, etc. The entire list is at https://aka.ms/azsktcp/appservice" },
                new KnowledgebaseEntity { Id = 10, DateCreated = now, DateUpdated = now, ItemId = "azsk.applicationproxy", Content = "Azure ApplicationProxy related checks: AAD, personal-data management, gateway-machine configuration. The entire list is at https://github.com/azsk/DevOpsKit-docs/blob/master/02-Secure-Development/ControlCoverage/Feature/ApplicationProxy.md" },
                new KnowledgebaseEntity { Id = 11, DateCreated = now, DateUpdated = now, ItemId = "azsk.automation", Content = "Verifies that Azure Automation is properly setup: variables are encrypted, webhooks are safe, log-analytics integration. The entire list is at https://aka.ms/azsktcp/automation" },
                new KnowledgebaseEntity { Id = 12, DateCreated = now, DateUpdated = now, ItemId = "azsk.batch", Content = "Azure Batch service verifications: proper ADD permissions are set, data is encrypted, alerts are set. The entire list is at https://aka.ms/azsktcp/batch" },
                new KnowledgebaseEntity { Id = 13, DateCreated = now, DateUpdated = now, ItemId = "azsk.botservice", Content = "Group of checks for Azure Bot service: traffic management, enabled logging. The entire list is at https://github.com/azsk/DevOpsKit-docs/blob/master/02-Secure-Development/ControlCoverage/Feature/BotService.md" },
                new KnowledgebaseEntity { Id = 14, DateCreated = now, DateUpdated = now, ItemId = "azsk.cdn", Content = "Azure CDN checks: RBAC and HTTPS configuration. The entire list is at https://aka.ms/azsktcp/cdn" },
                new KnowledgebaseEntity { Id = 15, DateCreated = now, DateUpdated = now, ItemId = "azsk.cloudservice", Content = "Azure Cloud Service verification: HTTPS configuration, switched-off remote debugging, OS version, RDP, and others. The entire list is at https://aka.ms/azsktcp/cloudservice" },
                new KnowledgebaseEntity { Id = 16, DateCreated = now, DateUpdated = now, ItemId = "azsk.containerinstances", Content = "A group of checks related to Azure Container Instances. The entire list is at https://aka.ms/azsktcp/containerinstances" },
                new KnowledgebaseEntity { Id = 17, DateCreated = now, DateUpdated = now, ItemId = "azsk.containerregistry", Content = "Verifies Azure Container Registry: admin user is disabled, RBAC, image-vulnerabilities scans, logs, and others. The entire list is at https://aka.ms/azsktcp/containerregistry" },
                new KnowledgebaseEntity { Id = 18, DateCreated = now, DateUpdated = now, ItemId = "azsk.cosmosdb", Content = "Azure CosmosDB should: use firewall, use only trusted network, has failover and backups setuped. The entire list is at https://aka.ms/azsktcp/cosmosdb" },
                new KnowledgebaseEntity { Id = 19, DateCreated = now, DateUpdated = now, ItemId = "azsk.databricks", Content = "A set of checks for Azure Databricks: secrets management, RBAC and users management. The entire list is at https://github.com/azsk/DevOpsKit-docs/blob/master/02-Secure-Development/ControlCoverage/Feature/Databricks.md" },
                new KnowledgebaseEntity { Id = 20, DateCreated = now, DateUpdated = now, ItemId = "azsk.datafactory", Content = "Azure Data Factory is properly configured: data is encrypted, keys are rotated, monitoring is enabled. The entire list is at https://aka.ms/azsktcp/datafactory" },
                new KnowledgebaseEntity { Id = 21, DateCreated = now, DateUpdated = now, ItemId = "azsk.datafactoryv2", Content = "Azure Data Factory V2 configration. The entire list is at https://https://github.com/azsk/DevOpsKit-docs/blob/master/02-Secure-Development/ControlCoverage/Feature/DataFactoryV2.md" },
                new KnowledgebaseEntity { Id = 22, DateCreated = now, DateUpdated = now, ItemId = "azsk.datalakeanalytics", Content = "Azure Data Lake configuration: proper AAD usage, data is encrypted and backup-ed, monitoring is enabled. The entire list is at https://aka.ms/azsktcp/datalakeanalytics" },
                new KnowledgebaseEntity { Id = 23, DateCreated = now, DateUpdated = now, ItemId = "azsk.datalakestore", Content = "Azure Data Lake Store configuration: AAD, access-control list, firewall, and others. The entire list is at https://aka.ms/azsktcp/datalakestore" },
                new KnowledgebaseEntity { Id = 24, DateCreated = now, DateUpdated = now, ItemId = "azsk.eventhub", Content = "Groups Azure Event Hub verifications: access-control, data-encryption, logs. The entire list is at https://aka.ms/azsktcp/eventhub" },
                new KnowledgebaseEntity { Id = 25, DateCreated = now, DateUpdated = now, ItemId = "azsk.ervnet", Content = "Azure ERvNet configuration. The entire list is at https://aka.ms/azsktcp/ervnet" },
                new KnowledgebaseEntity { Id = 26, DateCreated = now, DateUpdated = now, ItemId = "azsk.hdinsight", Content = "Azure HDInsights config: data-encryption, proper networking configuration, HDI version, and others. The entire list is at https://azsk.azurewebsites.net/02-Secure-Development/ControlCoverage/Feature/HDInsight.html" },
                new KnowledgebaseEntity { Id = 27, DateCreated = now, DateUpdated = now, ItemId = "azsk.keyvault", Content = "Azure Key Vault config: RBAC, secrets rotation, logging. the entire list is at https://azsk.azurewebsites.net/02-Secure-Development/ControlCoverage/Feature/KeyVault.html" },
                new KnowledgebaseEntity { Id = 28, DateCreated = now, DateUpdated = now, ItemId = "azsk.kubernetesservice", Content = "Azure AKS configuration: RBAC amd AAD integration, k8s version, public access, etc. The entire list is at https://aka.ms/azsktcp/KubernetesService" },
                new KnowledgebaseEntity { Id = 29, DateCreated = now, DateUpdated = now, ItemId = "azsk.loadbalancer", Content = "Azure Load Balancer config: RBAC, logs, public-ips. the entire list is at https://aka.ms/azsktcp/loadbalancer" },
                new KnowledgebaseEntity { Id = 30, DateCreated = now, DateUpdated = now, ItemId = "azsk.loganalytics", Content = "Azure Log Analytics config" },
                new KnowledgebaseEntity { Id = 31, DateCreated = now, DateUpdated = now, ItemId = "azsk.logicapps", Content = "Verifies if Logic App connectors use AAD-based authentication and data transit across connectors use encrypted channel. The entire list is at https://aka.ms/azsktcp/logicapps" },
                new KnowledgebaseEntity { Id = 32, DateCreated = now, DateUpdated = now, ItemId = "azsk.notificationhub", Content = "Azure Notification Hub configuration: access-policies and RBAC, logging and recovery policies. The entire list is at https://aka.ms/azsktcp/notificationhub" },
                new KnowledgebaseEntity { Id = 33, DateCreated = now, DateUpdated = now, ItemId = "azsk.odg", Content = "Azure on-premise data-gateway configuration: user permissions, logging, OS hardening. The entire list is at https://aka.ms/azsktcp/odg" },
                new KnowledgebaseEntity { Id = 34, DateCreated = now, DateUpdated = now, ItemId = "azsk.rediscache", Content = "Azure managed Redis cache configuration: firewall and user-access, keys rotation, vnet. The entire list is at https://aka.ms/azsktcp/rediscache" },
                new KnowledgebaseEntity { Id = 35, DateCreated = now, DateUpdated = now, ItemId = "azsk.search", Content = "Azure Search service configuration: access-control, data encryption, replication. The entire list is at https://aka.ms/azsktcp/search" },
                new KnowledgebaseEntity { Id = 36, DateCreated = now, DateUpdated = now, ItemId = "azsk.servicebus", Content = "Azure Service Bus service configuration: access-policies and tokens, data-encryption, recovery, etc. The entire list is at https://aka.ms/azsktcp/servicebus" },
                new KnowledgebaseEntity { Id = 37, DateCreated = now, DateUpdated = now, ItemId = "azsk.servicefabric", Content = "Azure Service Fabric config: data replication and encryption, secrets management, size of replicas sets, AAD, and others. The entire list is at https://aka.ms/azsktcp/servicefabric" },
                new KnowledgebaseEntity { Id = 38, DateCreated = now, DateUpdated = now, ItemId = "azsk.storage", Content = "Azure Storage service configuration: public access (HTTPS, CORS, disabled anonymous access), access-management and secrets rotation, AAD. The entire list is at https://aka.ms/azsktcp/storage" },
                new KnowledgebaseEntity { Id = 39, DateCreated = now, DateUpdated = now, ItemId = "azsk.streamanalytics", Content = "Azure Stream Analytics config: RBAC, logs, backup. The entire list is at https://aka.ms/azsktcp/streamanalytics" },
                new KnowledgebaseEntity { Id = 40, DateCreated = now, DateUpdated = now, ItemId = "azsk.subscriptioncore", Content = "Azure subscription related checks: amount of admin accounts, permissions and users, certificates, MFA, and others. The entire list is at https://aka.ms/azsktcp/sshealth" },
                new KnowledgebaseEntity { Id = 41, DateCreated = now, DateUpdated = now, ItemId = "azsk.sqldatabase", Content = "Azure managed MS SQL Server configuration: user access, encryption, firewall. The entire list is at https://aka.ms/azsktcp/sqldatabase" },
                new KnowledgebaseEntity { Id = 42, DateCreated = now, DateUpdated = now, ItemId = "azsk.trafficmanager", Content = "Azure Traffic Manager HTTPS and RBAC configuration. The entire list is at https://aka.ms/azsktcp/trafficmanager" },
                new KnowledgebaseEntity { Id = 43, DateCreated = now, DateUpdated = now, ItemId = "azsk.virtualmachine", Content = "Azure VM config: OS, network, disk, policies. The entire list is at https://aka.ms/azsktcp/virtualmachine" },
                new KnowledgebaseEntity { Id = 44, DateCreated = now, DateUpdated = now, ItemId = "azsk.virtualmachinescaleset", Content = "Azure VM Scale Sets config: version, enabled antimalware, VNet and RBAC, etc. The entire list is at https://aka.ms/azsktcp/virtualmachinescaleset" },
                new KnowledgebaseEntity { Id = 45, DateCreated = now, DateUpdated = now, ItemId = "azsk.virtualnetwork", Content = "Azure Network configuration: IPs, RBAC, security-groups. The entire list is at https://aka.ms/azsktcp/virtualnetwork" });

            var checksTooltipsInsertTime = new DateTime(2020, 3, 12, 13, 33, 48, 595, DateTimeKind.Utc).AddTicks(7551);
            modelBuilder.Entity<KnowledgebaseEntity>().HasData(
                new KnowledgebaseEntity { Id = 46, DateCreated = checksTooltipsInsertTime, DateUpdated = checksTooltipsInsertTime, ItemId = "metadata.checks_nodata_description", Content = "NoData means one of the following: 1. Joseki is not able to perform the Check due to not sufficient permissions; 2. Docker Image scanner was not able to complete a scan; 3. the Check requires a manual step to be performed" },
                new KnowledgebaseEntity { Id = 47, DateCreated = checksTooltipsInsertTime, DateUpdated = checksTooltipsInsertTime, ItemId = "metadata.checks_warning_description", Content = "Warning indicates when Joseki found, likely, a not critical issue with a particular infrastructure component" },
                new KnowledgebaseEntity { Id = 48, DateCreated = checksTooltipsInsertTime, DateUpdated = checksTooltipsInsertTime, ItemId = "metadata.checks_failed_description", Content = "Failed check highlights the most critical issues that should be reviewed first" },
                new KnowledgebaseEntity { Id = 49, DateCreated = checksTooltipsInsertTime, DateUpdated = checksTooltipsInsertTime, ItemId = "metadata.checks_passed_description", Content = "You're good ;) A component satisfies verified rule" },
                new KnowledgebaseEntity { Id = 50, DateCreated = checksTooltipsInsertTime, DateUpdated = checksTooltipsInsertTime, ItemId = "metadata.checks_score_description", Content = "The audit score. It indicates how close the infrastructure is to known best-practices configuration. The formula excludes NoData checks, gives doubled weight to Passed and Failed results: (Passed*2)/(Failed*2 + Passed*2 + Warning)" });
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