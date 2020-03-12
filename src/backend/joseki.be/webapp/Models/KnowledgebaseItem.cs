namespace webapp.Models
{
    /// <summary>
    /// Represents content of Knowledgebase entry.
    /// </summary>
    public class KnowledgebaseItem
    {
        /// <summary>
        /// Represents not found entity.
        /// </summary>
        public static readonly KnowledgebaseItem NotFound = new KnowledgebaseItem();

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgebaseItem"/> class.
        /// </summary>
        /// <param name="id">Item public identifier.</param>
        /// <param name="content">The item content.</param>
        public KnowledgebaseItem(string id, string content)
        {
            this.Id = id;
            this.Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgebaseItem"/> class.
        /// </summary>
        public KnowledgebaseItem()
        {
        }

        /// <summary>
        /// Knowledgebase entry identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Knowledgebase entry content.
        /// </summary>
        public string Content { get; set; }
    }
}