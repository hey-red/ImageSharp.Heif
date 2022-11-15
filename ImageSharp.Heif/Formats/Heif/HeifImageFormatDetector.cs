using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif.Formats.Heif;

public sealed class HeifImageFormatDetector : IImageFormatDetector
{
    public int HeaderSize => 12;

    public IImageFormat? DetectFormat(ReadOnlySpan<byte> header)
    {
        return IsSupportedFileFormat(header) ? HeifFormat.Instance : null;
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
            header.SequenceEqual(HeifConstants.HeifBrandHeader) ||
            header.SequenceEqual(HeifConstants.HeimBrandHeader) ||
            header.SequenceEqual(HeifConstants.HeisBrandHeader) ||
            header.SequenceEqual(HeifConstants.HeixBrandHeader) ||
            header.SequenceEqual(HeifConstants.HevcBrandHeader) ||
            header.SequenceEqual(HeifConstants.HevxBrandHeader) ||
            header.SequenceEqual(HeifConstants.HevmBrandHeader) ||
            header.SequenceEqual(HeifConstants.HevsBrandHeader) ||
            header.SequenceEqual(HeifConstants.Mif1BrandHeader) ||
            header.SequenceEqual(HeifConstants.Msf1BrandHeader);
    }
}