namespace FileExplorerWebApp.Domain.Entities
{
    /// <summary>
    /// The entity for audit.
    /// </summary>
    public class Audit
    {
        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last modified date time.
        /// </summary>
        /// <value>
        /// The last modified date time.
        /// </value>
        public DateTime? LastModifiedDateTime { get; set; }
    }
}
