using SixLabors.ImageSharp;

namespace HeyRed.ImageSharp.Heif.Formats.Heif;

public class HeifConfigurationModule : IConfigurationModule
{
    /// <summary>
    /// Registers the image encoders, decoders and mime type detectors for the heif format.
    /// </summary>
    public void Configure(Configuration configuration)
    {
        configuration.ImageFormatsManager.SetDecoder(HeifFormat.Instance, new HeifDecoder());
        configuration.ImageFormatsManager.AddImageFormatDetector(new HeifImageFormatDetector());
    }
}