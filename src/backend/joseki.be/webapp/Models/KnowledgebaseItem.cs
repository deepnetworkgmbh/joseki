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
        /// Knowledgebase entry identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Knowledgebase entry content.
        /// </summary>
        public string Content { get; set; }
    }
}