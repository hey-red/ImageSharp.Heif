using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif.Formats.Avif;

public sealed class AvifImageFormatDetector : IImageFormatDetector
{
    public int HeaderSize => 12;

    public IImageFormat? DetectFormat(ReadOnlySpan<byte> header)
    {
        return IsSupportedFileFormat(header) ? AvifFormat.Instance : null;
    }

    private bool IsSupportedFileFormat(ReadOnlySpan<byte> header)
    {
        return
            header.Length >= HeaderSize &&
            header.Slice(4, 4).SequenceEqual(FormatConstants.FtypHeader) &&
            ContainsSupportedBrand(header.Slice(8, 4));
    }

    private static bool ContainsSupportedBrand(ReadOnlySpan<byte> header)
    {
        return
            header.SequenceEqual(AvifConstants.AvifBrandHeader) ||
            header.SequenceEqual(AvifConstants.AvisBrandHeader);
    }
}