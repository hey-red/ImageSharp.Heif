using LibHeifSharp;

using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;

namespace HeyRed.ImageSharp.Heif;

/*
 * Based on https://github.com/0xC0000054/libheif-sharp-samples/blob/main/src/decoder/Program.cs
 */
internal sealed class HeifDecoderCore
{
    private readonly HeifDecoderOptions _options;

    public HeifDecoderCore(HeifDecoderOptions options)
    {
        _options = options;
    }

    public ImageInfo Identify(Stream stream, CancellationToken cancellationToken)
    {
        using var context = new HeifContext(stream, true);
        using var imageHandle = context.GetPrimaryImageHandle();

        var metadata = new ImageMetadata();

        HeifImageUtils.FillImageMetadata(metadata, imageHandle);

        return new ImageInfo(
            new PixelTypeInfo(imageHandle.BitDepth),
            new Size(imageHandle.Width, imageHandle.Height),
            metadata);
    }

    public Image<TPixel> Decode<TPixel>(Stream stream, CancellationToken cancellationToken)
       where TPixel : unmanaged, IPixel<TPixel>
    {
        var decodingOptions = new HeifDecodingOptions
        {
            ConvertHdrToEightBit = _options.ConvertHdrToEightBit,
            IgnoreTransformations = _options.IgnoreTransformations,
            Strict = _options.Strict
        };

        using var context = new HeifContext(stream, true);

        if (_options.DecodingMode == DecodingMode.PrimaryImage)
        {
            using var imageHandle = context.GetPrimaryImageHandle();

            return DoDecode<TPixel>(imageHandle, decodingOptions);
        }
        else
        {
            var topLevelImageIds = context.GetTopLevelImageIds();

            Image<TPixel>? resultImage = null;

            for (int i = 0; i < topLevelImageIds.Count; i++)
            {
                using var imageHandle = context.GetImageHandle(topLevelImageIds[i]);

                // Root image
                if (resultImage == null)
                {
                    resultImage = DoDecode<TPixel>(imageHandle, decodingOptions);
                }
                else
                {
                    using var image = DoDecode<TPixel>(imageHandle, decodingOptions);

                    resultImage.Frames.AddFrame(image.Frames.RootFrame);
                }
            }

            return resultImage!;
        }
    }

    private static Image<TPixel> DoDecode<TPixel>(HeifImageHandle imageHandle, HeifDecodingOptions decodingOptions) where TPixel : unmanaged, IPixel<TPixel>
    {
        Image outputImage;

        HeifChroma chroma;
        bool hasAlpha = imageHandle.HasAlphaChannel;
        int bitDepth = imageHandle.BitDepth;

        if (bitDepth == 8 || decodingOptions.ConvertHdrToEightBit)
        {
            chroma = hasAlpha ? HeifChroma.InterleavedRgba32 : HeifChroma.InterleavedRgb24;
        }
        else
        {
            // Use the native byte order of the operating system.
            if (BitConverter.IsLittleEndian)
            {
                chroma = hasAlpha ? HeifChroma.InterleavedRgba64LE : HeifChroma.InterleavedRgb48LE;
            }
            else
            {
                chroma = hasAlpha ? HeifChroma.InterleavedRgba64BE : HeifChroma.InterleavedRgb48BE;
            }
        }

        using (var image = imageHandle.Decode(HeifColorspace.Rgb, chroma, decodingOptions))
        {
            outputImage = chroma switch
            {
                HeifChroma.InterleavedRgb24 => HeifImageUtils.CreateEightBitImageWithoutAlpha(image),
                HeifChroma.InterleavedRgba32 => HeifImageUtils.CreateEightBitImageWithAlpha(image, imageHandle.IsPremultipliedAlpha),
                HeifChroma.InterleavedRgb48BE or HeifChroma.InterleavedRgb48LE => HeifImageUtils.CreateSixteenBitImageWithoutAlpha(image),
                HeifChroma.InterleavedRgba64BE or HeifChroma.InterleavedRgba64LE =>
                    HeifImageUtils.CreateSixteenBitImageWithAlpha(image, imageHandle.IsPremultipliedAlpha, imageHandle.BitDepth),
                _ => throw new InvalidOperationException("Unsupported HeifChroma value."),
            };

            if (image.IccColorProfile != null)
            {
                outputImage.Metadata.IccProfile = new IccProfile(image.IccColorProfile.GetIccProfileBytes());
            }
        }

        HeifImageUtils.FillImageMetadata(outputImage.Metadata, imageHandle);

        return outputImage.CloneAs<TPixel>(outputImage.GetConfiguration());
    }
}
