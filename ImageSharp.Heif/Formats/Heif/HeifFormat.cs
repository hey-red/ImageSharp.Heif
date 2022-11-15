using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif.Formats.Heif;

public sealed class HeifFormat : IImageFormat<HeifMetadata>
{
    private HeifFormat()
    {
    }

    /// <summary>
    /// Gets the current instance.
    /// </summary>
    public static HeifFormat Instance { get; } = new HeifFormat();

    public string Name => "HEIF";

    public string DefaultMimeType => "image/heif";

    public IEnumerable<string> FileExtensions => HeifConstants.FileExtensions;

    public IEnumerable<string> MimeTypes => HeifConstants.MimeTypes;

    public HeifMetadata CreateDefaultFormatMetadata() => new();
}