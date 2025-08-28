using FileExplorerWebApp.Application.DTOs.Preview;

namespace FileExplorerWebApp.Application.DTOs
{
    /// <summary>
    /// The file dto.
    /// </summary>
    /// <seealso cref="FileExplorerWebApp.Application.DTOs.AuditDto" />
    public class FileDto : AuditDto
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
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the MIME.
        /// </summary>
        /// <value>
        /// The MIME.
        /// </value>
        public string Mime { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public byte[]? Content { get; set; }

        /// <summary>
        /// Gets or sets the preview.
        /// </summary>
        /// <value>
        /// The preview.
        /// </value>
        public byte[]? Preview { get; set; }

        /// <summary>
        /// Gets or sets the preview MIME.
        /// </summary>
        /// <value>
        /// The preview MIME.
        /// </value>
        public string? PreviewMime { get; set; }

        /// <summary>
        /// Gets or sets the kind of the preview.
        /// </summary>
        /// <value>
        /// The kind of the preview.
        /// </value>
        public PreviewKind PreviewKind { get; set; } = PreviewKind.None;
    }
}
