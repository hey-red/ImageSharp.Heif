using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif.Formats.Avif;

public sealed class AvifFormat : IImageFormat<AvifMetadata>
{
    private AvifFormat()
    {
    }

    /// <summary>
    /// Gets the current instance.
    /// </summary>
    public static AvifFormat Instance { get; } = new AvifFormat();

    public string Name => "AVIF";

    public string DefaultMimeType => "image/avif";

    public IEnumerable<string> FileExtensions => AvifConstants.FileExtensions;

    public IEnumerable<string> MimeTypes => AvifConstants.MimeTypes;

    public AvifMetadata CreateDefaultFormatMetadata() => new();
}
