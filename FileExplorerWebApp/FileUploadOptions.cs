namespace FileExplorerWebApp
{
    /// <summary>
    /// The file upload options
    /// </summary>
    public class FileUploadOptions
    {
        /// <summary>
        /// Allowed file extensions. Example: ".txt", ".png"
        /// </summary>
        public string[] AllowedExtensions { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Allowed MIME types. Example: "text/plain", "image/png"
        /// </summary>
        public string[] AllowedMimeTypes { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Maximum file size in megabytes per single file.
        /// </summary>
        public int MaxFileSizeMb { get; set; } = 2;

        /// <summary>
        /// Maximum number of files per upload.
        /// </summary>
        public int MaxFilesCount { get; set; } = 5;
    }
}
