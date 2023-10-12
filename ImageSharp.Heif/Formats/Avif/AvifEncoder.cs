using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif.Formats.Avif;

public sealed class AvifEncoder : ImageEncoder
{
    protected override void Encode<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
