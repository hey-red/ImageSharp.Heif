using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif.Formats.Heif;

public sealed class HeifEncoder : ImageEncoder
{
    protected override void Encode<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
