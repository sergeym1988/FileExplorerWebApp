namespace FileExplorerWebApp.Domain.Entities
{
    /// <summary>
    /// Represents a file in the file system.
    /// </summary>
    public class File : Audit
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the folder identifier.
        /// </summary>
        /// <value>
        /// The folder identifier.
        /// </value>
        public Guid? FolderId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public required string Mime { get; set; }

        /// <summary>
        /// Gets or sets the file content.
        /// </summary>
        /// <value>
        /// The binary content of the file.
        /// </value>
        public required byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>ы
        public virtual Folder Folder { get; set; } = null!;
    }
}
