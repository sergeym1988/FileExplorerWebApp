namespace FileExplorerWebApp
{
    using System;
    using System.Collections.Concurrent;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using FileExplorerWebApp.Application.DTOs.Preview;

    public static class PreviewGenerator
    {
        private static readonly ConcurrentDictionary<Guid, PreviewResult> _cache = new();

        private const int TextPreviewChars = 512;
        private const int ImageThumbMaxSide = 200;

        /// <summary>
        /// Get or generate preview for provided file entity.
        /// Caches result by file id.
        /// Non-throwing: on errors returns PreviewKind.None.
        /// </summary>
        public static async Task<PreviewResult> GetOrCreatePreviewAsync(
            Guid fileId,
            byte[]? content,
            string? mime
        )
        {
            if (
                fileId == Guid.Empty
                || content == null
                || content.Length == 0
                || string.IsNullOrEmpty(mime)
            )
            {
                return PreviewResultNone();
            }

            if (_cache.TryGetValue(fileId, out var existing))
                return existing;

            try
            {
                var generated = await Task.Run(() => GeneratePreviewInternal(content, mime));
                _cache[fileId] = generated;
                return generated;
            }
            catch
            {
                return PreviewResultNone();
            }
        }

        /// <summary>
        /// Generate preview synchronously (internal).
        /// Uses System.Drawing for images, UTF8 truncation for text.
        /// </summary>
        private static PreviewResult GeneratePreviewInternal(byte[] content, string mime)
        {
            try
            {
                if (mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    var thumb = CreateImageThumbnail(content, ImageThumbMaxSide, ImageThumbMaxSide);
                    if (thumb != null && thumb.Length > 0)
                    {
                        return new PreviewResult
                        {
                            Kind = PreviewKind.Image,
                            PreviewBytes = thumb,
                            PreviewMime = "image/jpeg",
                        };
                    }

                    return PreviewResultNone();
                }

                if (
                    string.Equals(mime, "text/plain", StringComparison.OrdinalIgnoreCase)
                    || mime.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
                )
                {
                    var text = SafeDecodeText(content, TextPreviewChars);
                    var bytes = Encoding.UTF8.GetBytes(text);
                    return new PreviewResult
                    {
                        Kind = PreviewKind.Text,
                        PreviewBytes = bytes,
                        PreviewMime = "text/plain; charset=utf-8",
                    };
                }

                return PreviewResultNone();
            }
            catch
            {
                return PreviewResultNone();
            }
        }

        private static PreviewResult PreviewResultNone() =>
            new PreviewResult
            {
                Kind = PreviewKind.None,
                PreviewBytes = null,
                PreviewMime = null,
            };

        /// <summary>
        /// Create JPEG thumbnail (max width/height). Returns JPEG bytes or null on failure.
        /// Uses System.Drawing.
        /// </summary>
        private static byte[]? CreateImageThumbnail(byte[] imageBytes, int maxWidth, int maxHeight)
        {
            try
            {
                using var ms = new MemoryStream(imageBytes);
                using var srcImg = System.Drawing.Image.FromStream(ms);

                var (newW, newH) = ScaleToFit(srcImg.Width, srcImg.Height, maxWidth, maxHeight);

                using var thumb = new Bitmap(newW, newH);
                using (var g = Graphics.FromImage(thumb))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    var rect = new Rectangle(0, 0, newW, newH);
                    g.DrawImage(srcImg, rect);
                }

                using var outMs = new MemoryStream();
                var encoder = GetEncoder(ImageFormat.Jpeg);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality,
                    80L
                );
                thumb.Save(outMs, encoder, encoderParams);
                return outMs.ToArray();
            }
            catch
            {
                return null;
            }
        }

        private static (int w, int h) ScaleToFit(int srcW, int srcH, int maxW, int maxH)
        {
            if (srcW <= 0 || srcH <= 0)
                return (maxW, maxH);
            var ratio = Math.Min((double)maxW / srcW, (double)maxH / srcH);
            if (ratio >= 1.0)
                return (srcW, srcH);
            return ((int)(srcW * ratio), (int)(srcH * ratio));
        }

        private static ImageCodecInfo? GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            foreach (var c in codecs)
                if (c.FormatID == format.Guid)
                    return c;
            return null;
        }

        /// <summary>
        /// Safely decode bytes to UTF8 string and truncate to charLimit characters.
        /// If decoding fails falls back to replacement characters.
        /// </summary>
        private static string SafeDecodeText(byte[] bytes, int charLimit)
        {
            try
            {
                var text = Encoding.UTF8.GetString(bytes);
                if (text.Length <= charLimit)
                    return text;
                return text.Substring(0, charLimit) + "…";
            }
            catch
            {
                try
                {
                    var text = Encoding.ASCII.GetString(bytes);
                    if (text.Length <= charLimit)
                        return text;
                    return text.Substring(0, charLimit) + "…";
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Optional: clear cache (for tests or explicit invalidation).
        /// </summary>
        public static void ClearCache() => _cache.Clear();

        /// <summary>
        /// Optional: remove preview for specific file (when file updated/removed).
        /// </summary>
        public static void Invalidate(Guid fileId) => _cache.TryRemove(fileId, out _);
    }
}
