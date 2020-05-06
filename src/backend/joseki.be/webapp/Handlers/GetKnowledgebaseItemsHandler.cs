using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Encapsulates GET operations for Knowledgebase item.
    /// </summary>
    public class GetKnowledgebaseItemsHandler
    {
        private readonly string rootPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetKnowledgebaseItemsHandler"/> class.
        /// </summary>
        /// <param name="rootPath">Root path for the files.</param>
        public GetKnowledgebaseItemsHandler(string rootPath = "Docs")
        {
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

            if (!File.Exists(path))
            {
                return KnowledgebaseItem.NotFound;
            }

            var content = await File.ReadAllTextAsync(path);
            return new KnowledgebaseItem
            {
                Id = id,
                Content = content,
            };
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