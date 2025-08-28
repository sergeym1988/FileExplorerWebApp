namespace FileExplorerWebApp.Application.DTOs.Preview
{
    public class PreviewResult
    {
        public PreviewKind Kind { get; init; } = PreviewKind.None;
        public byte[]? PreviewBytes { get; init; }
        public string? PreviewMime { get; init; }
    }
}
