namespace FileExplorerWebApp.Application.DTOs
{
    /// <summary>
    /// The DTO for audit.
    /// </summary>
    public class AuditDto
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
