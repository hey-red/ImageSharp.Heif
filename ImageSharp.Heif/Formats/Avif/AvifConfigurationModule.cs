using SixLabors.ImageSharp;

namespace HeyRed.ImageSharp.Heif.Formats.Avif;

public class AvifConfigurationModule : IConfigurationModule
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the avif format.
    /// </summary>
    public void Configure(Configuration configuration)
    {
        configuration.ImageFormatsManager.SetDecoder(AvifFormat.Instance, new HeifDecoder());
        configuration.ImageFormatsManager.AddImageFormatDetector(new AvifImageFormatDetector());
    }
}