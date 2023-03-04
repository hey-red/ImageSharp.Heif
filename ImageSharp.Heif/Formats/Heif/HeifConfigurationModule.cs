using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif.Formats.Heif;

public sealed class HeifConfigurationModule : IImageFormatConfigurationModule
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the heif format.
    /// </summary>
    public void Configure(Configuration configuration)
    {
        configuration.ImageFormatsManager.SetDecoder(HeifFormat.Instance, HeifDecoder.Instance);
        configuration.ImageFormatsManager.AddImageFormatDetector(new HeifImageFormatDetector());
    }
}