namespace FileExplorerWebApp.Application.DTOs
{
    /// <summary>
    /// The folder dto.
    /// </summary>
    /// <seealso cref="FileExplorerWebApp.Application.DTOs.AuditDto" />
    public class FolderDto : AuditDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the parent folder identifier.
        /// </summary>
        /// <value>
        /// The parent folder identifier.
        /// </value>
        public Guid? ParentFolderId { get; set; }

        /// <summary>
        /// Gets or sets the sub folders.
        /// </summary>
        /// <value>
        /// The sub folders.
        /// </value>
        public List<FolderDto>? SubFolders { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        public List<FileDto>? Files { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren { get; set; }
    }
}
