using System.Linq;
using System.Threading.Tasks;

using joseki.db;
using joseki.db.entities;

using Microsoft.EntityFrameworkCore;

using webapp.Models;

namespace webapp.Handlers
{
    /// <summary>
    /// Encapsulates GET operations for Knowledgebase item.
    /// </summary>
    public class GetKnowledgebaseItemsHandler
    {
        private readonly JosekiDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetKnowledgebaseItemsHandler"/> class.
        /// </summary>
        /// <param name="db">Joseki database object.</param>
        public GetKnowledgebaseItemsHandler(JosekiDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Tries to get item from the database by identifier.
        /// </summary>
        /// <param name="id">Knowledgebase item identifier.</param>
        /// <returns>Knowledgebase item content or default NotFound entity.</returns>
        public async Task<KnowledgebaseItem> GetItemById(string id)
        {
            var content = await this.db.Set<KnowledgebaseEntity>()
                .AsNoTracking()
                .Where(i => i.ItemId == id)
                .Select(i => i.Content)
                .FirstOrDefaultAsync();

            if (content != null)
            {
                return new KnowledgebaseItem
                {
                    Id = id,
                    Content = content,
                };
            }

            return KnowledgebaseItem.NotFound;
        }

        /// <summary>
        /// Tries to get items from the database by their public identifiers.
        /// </summary>
        /// <param name="ids">Knowledgebase item identifiers.</param>
        /// <returns>Found knowledgebase items.</returns>
        public async Task<KnowledgebaseItem[]> GetItemsByIds(string[] ids)
        {
            return await this.db.Set<KnowledgebaseEntity>()
                .AsNoTracking()
                .Where(i => ids.Contains(i.ItemId))
                .Select(i => new KnowledgebaseItem(i.ItemId, i.Content))
                .ToArrayAsync();
        }

        /// <summary>
        /// Returns all knowledgebase items from DB.
        /// </summary>
        /// <returns>All Knowledgebase items.</returns>
        public async Task<KnowledgebaseItem[]> GetAll()
        {
            var items = await this.db.Set<KnowledgebaseEntity>()
                .AsNoTracking()
                .Select(i => new KnowledgebaseItem(i.ItemId, i.Content))
                .ToArrayAsync();

            return items;
        }
    }
}