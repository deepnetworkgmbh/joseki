using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using joseki.db;
using Microsoft.EntityFrameworkCore;
using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Encapsulates GET operations for Knowledgebase item.
    /// </summary>
    public class GetKnowledgebaseItemsHandler
    {
        private readonly string fallbackTemplateId = "template.check.fallback";
        private readonly string rootPath;

        private readonly JosekiDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetKnowledgebaseItemsHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        /// <param name="rootPath">Root path for the files.</param>
        public GetKnowledgebaseItemsHandler(JosekiDbContext db, string rootPath = "Docs")
        {
            this.db = db;
            this.rootPath = rootPath;
        }

        /// <summary>
        /// Tries to get item from the file identifier.
        /// </summary>
        /// <param name="id">Knowledgebase item identifier.</param>
        /// <returns>Knowledgebase item content or default NotFound entity.</returns>
        public async Task<KnowledgebaseItem> GetItemById(string id)
        {
            var path = $"{this.rootPath}/{id}.md";
            var content = string.Empty;

            // check if md file exists
            if (File.Exists(path))
            {
                content = await File.ReadAllTextAsync(path);
                return new KnowledgebaseItem
                {
                    Id = id,
                    Content = content,
                };
            }

            // if a check document is requested
            // use db.Check from dbContext
            else if (id.StartsWith("checks."))
            {
                var entityCheckId = id.Replace("checks.", string.Empty).ToLower();

                // check if such check exists
                var checkEntity = this.db
                    .Check
                    .AsNoTracking()
                    .FirstOrDefault(c => c.CheckId.ToLower() == entityCheckId);

                if (checkEntity == null)
                {
                    return KnowledgebaseItem.NotFound;
                }

                // check if template path exists
                var templatePath = $"{this.rootPath}/{this.fallbackTemplateId}.md";
                if (!File.Exists(templatePath))
                {
                    return KnowledgebaseItem.NotFound;
                }

                // generate KnowledgebaseItem using fallback template
                content = await File.ReadAllTextAsync(templatePath);
                content = content.Replace("{checkId}", entityCheckId);
                content = content.Replace("{description}", checkEntity.Description);
                content = content.Replace("{remediation}", checkEntity.Remediation);

                return new KnowledgebaseItem
                {
                    Id = entityCheckId,
                    Content = content,
                };
            }

            return KnowledgebaseItem.NotFound;
        }

        /// <summary>
        /// Tries to get items from the Docs folder by their public identifiers.
        /// </summary>
        /// <param name="ids">Knowledgebase item identifiers.</param>
        /// <returns>Found knowledgebase items.</returns>
        public async Task<KnowledgebaseItem[]> GetItemsByIds(string[] ids)
        {
            var result = new List<KnowledgebaseItem>();

            foreach (string id in ids)
            {
                var item = await this.GetItemById(id);
                result.Add(item);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns all knowledgebase items from Docs.
        /// </summary>
        /// <returns>All Knowledgebase items.</returns>
        public async Task<KnowledgebaseItem[]> GetAll()
        {
            var result = new List<KnowledgebaseItem>();

            var files = Directory.EnumerateFiles(this.rootPath, "*.md", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                var id = this.ExtractIdFromPath(file);
                var item = await this.GetItemById(id);
                result.Add(item);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns all metadata items from Docs.
        /// </summary>
        /// <returns>Metadata items.</returns>
        public async Task<KnowledgebaseItem[]> GetMetadataItems()
        {
            var result = new List<KnowledgebaseItem>();

            var files = Directory.EnumerateFiles(this.rootPath, "metadata.*.md", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                var id = this.ExtractIdFromPath(file);
                var item = await this.GetItemById(id);
                result.Add(item);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Creates a file under path for item.
        /// </summary>
        /// <param name="item">KnowledgebaseItem.</param>
        public async Task AddItem(KnowledgebaseItem item)
        {
            var path = $"{this.rootPath}/{item.Id}.md";
            if (!File.Exists(path))
            {
                await File.WriteAllTextAsync(path, item.Content);
            }
        }

        /// <summary>
        /// Updates a file under path for item.
        /// </summary>
        /// <param name="item">KnowledgebaseItem.</param>
        public async Task UpdateItem(KnowledgebaseItem item)
        {
            var path = $"{this.rootPath}/{item.Id}.md";
            await File.WriteAllTextAsync(path, item.Content);
        }

        /// <summary>
        /// Extracts id of document from requested path.
        /// </summary>
        /// <param name="path">Path of the document.</param>
        /// <returns>id of the document.</returns>
        private string ExtractIdFromPath(string path)
        {
            Regex pattern = new Regex(this.rootPath + "/(?<documentid>.*).md");
            Match match = pattern.Match(path);
            return match.Groups["documentid"].Value;
        }
    }
}