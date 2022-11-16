using LibHeifSharp;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;

namespace HeyRed.ImageSharp.Heif;

/*
 * Based on https://github.com/0xC0000054/libheif-sharp-samples/blob/main/src/decoder/Program.cs
 */
public sealed class HeifDecoder : IImageDecoder, IImageInfoDetector, IHeifDecoderOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether high bit-depth images should be converted to 8-bits-per-channel.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if high bit-depth images should be converted to 8-bits-per-channel; otherwise, <see langword="false"/>.
    /// </value>
    public bool ConvertHdrToEightBit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether transformations are ignored when decoding the image.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if transformations are ignored when decoding the image; otherwise, <see langword="false"/>.
    /// </value>
    public bool IgnoreTransformations { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an error is returned for invalid input.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if an error is returned for invalid input; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property is supported starting with LibHeif 1.13.0, it is ignored on earlier versions.
    /// </remarks>
    public bool Strict { get; set; }

    /// <summary>
    /// The image decoding mode.
    /// </summary>
    /// <remarks>
    /// Default is <see cref="DecodingMode.PrimaryImage"/>
    /// </remarks>
    public DecodingMode DecodingMode { get; set; } = DecodingMode.PrimaryImage;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    /// <param name="configuration"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Image<TPixel> Decode<TPixel>(Configuration configuration, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel>
    {
        var decodingOptions = new HeifDecodingOptions
        {
            ConvertHdrToEightBit = ConvertHdrToEightBit,
            IgnoreTransformations = IgnoreTransformations,
            Strict = Strict
        };

        using var context = new HeifContext(stream);

        if (DecodingMode == DecodingMode.PrimaryImage)
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

    public Image Decode(Configuration configuration, Stream stream, CancellationToken cancellationToken) =>
        Decode<Rgba32>(configuration, stream, cancellationToken);

    public IImageInfo? Identify(Configuration configuration, Stream stream, CancellationToken cancellationToken)
    {
        using var context = new HeifContext(stream);
        using var imageHandle = context.GetPrimaryImageHandle();

        if (imageHandle != null)
        {
            var metadata = new ImageMetadata();

            HeifImageUtils.FillImageMetadata(metadata, imageHandle);

            return new ImageInfo(
                new PixelTypeInfo(imageHandle.BitDepth),
                imageHandle.Width,
                imageHandle.Height,
                metadata);
        }

        return null;
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