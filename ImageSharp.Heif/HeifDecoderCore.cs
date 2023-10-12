using LibHeifSharp;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.Metadata.Profiles.Xmp;

namespace HeyRed.ImageSharp.Heif;

/*
 * Based on https://github.com/0xC0000054/libheif-sharp-samples/blob/main/src/decoder/Program.cs
 */
internal sealed class HeifDecoderCore
{
    /// <summary>
    /// The global configuration.
    /// </summary>
    private readonly Configuration configuration;

    /// <summary>
    /// The maximum number of frames to decode. Inclusive.
    /// </summary>
    private readonly uint maxFrames;

    /// <summary>
    /// Gets a value indicating whether to ignore encoded metadata when decoding.
    /// </summary>
    private readonly bool skipMetadata;

    /// <summary>
    /// The image decoding mode.
    /// </summary>
    private readonly DecodingMode decodingMode;

    /// <summary>
    /// Gets or sets a value indicating whether high bit-depth images should be converted to 8-bits-per-channel.
    /// </summary>
    private readonly bool convertHdrToEightBit;

    /// <summary>
    /// Gets or sets a value indicating whether transformations are ignored when decoding the image.
    /// </summary>
    private readonly bool ignoreTransformations;

    /// <summary>
    /// Gets or sets a value indicating whether an error is returned for invalid input.
    /// </summary>
    private readonly bool strict;

    public HeifDecoderCore(HeifDecoderOptions options)
    {
        configuration = options.GeneralOptions.Configuration;
        maxFrames = options.GeneralOptions.MaxFrames;
        decodingMode = options.DecodingMode;
        convertHdrToEightBit = options.ConvertHdrToEightBit;
        ignoreTransformations = options.IgnoreTransformations;
        strict = options.Strict;
    }

    public ImageInfo Identify(Stream stream, CancellationToken cancellationToken)
    {
        using var context = new HeifContext(stream, true);
        using var imageHandle = context.GetPrimaryImageHandle();

        var metadata = new ImageMetadata();

        FillImageMetadata(metadata, imageHandle);

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
            ConvertHdrToEightBit = convertHdrToEightBit,
            IgnoreTransformations = ignoreTransformations,
            Strict = strict,
        };

        using var context = new HeifContext(stream, true);

        if (decodingMode == DecodingMode.PrimaryImage)
        {
            using var imageHandle = context.GetPrimaryImageHandle();

            return DoDecode<TPixel>(imageHandle, decodingOptions);
        }
        else
        {
            Image<TPixel>? resultImage = null;

            try
            {
                var topLevelImageIds = context.GetTopLevelImageIds();

                uint frameCount = 0;
                foreach (HeifItemId topLevelImageId in topLevelImageIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    using var imageHandle = context.GetImageHandle(topLevelImageId);

                    // Image
                    if (resultImage == default)
                    {
                        resultImage = DoDecode<TPixel>(imageHandle, decodingOptions);
                    }
                    // Frame
                    else
                    {
                        ImageFrame<TPixel> frame = DoDecode<TPixel>(imageHandle, decodingOptions).Frames.RootFrame;

                        if (resultImage.Size != frame.Size())
                        {
                            throw new ArgumentException("Images with different sizes are not supported");
                        }

                        resultImage.Frames.AddFrame(frame);
                    }

                    if (++frameCount == maxFrames)
                    {
                        break;
                    }
                }

                return resultImage!;
            }
            catch
            {
                resultImage?.Dispose();

                throw;
            }
        }
    }

    private Image<TPixel> DoDecode<TPixel>(HeifImageHandle imageHandle, HeifDecodingOptions decodingOptions) where TPixel : unmanaged, IPixel<TPixel>
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
                HeifChroma.InterleavedRgb24 => CreateEightBitImageWithoutAlpha(image),
                HeifChroma.InterleavedRgba32 => CreateEightBitImageWithAlpha(image, imageHandle.IsPremultipliedAlpha),
                HeifChroma.InterleavedRgb48BE or HeifChroma.InterleavedRgb48LE => 
                    CreateSixteenBitImageWithoutAlpha(image),
                HeifChroma.InterleavedRgba64BE or HeifChroma.InterleavedRgba64LE =>
                    CreateSixteenBitImageWithAlpha(image, imageHandle.IsPremultipliedAlpha, imageHandle.BitDepth),
                _ => throw new InvalidOperationException("Unsupported HeifChroma value."),
            };

            if (!skipMetadata && image.IccColorProfile != null)
            {
                outputImage.Metadata.IccProfile = new IccProfile(image.IccColorProfile.GetIccProfileBytes());
            }
        }

        if (!skipMetadata)
        {
            FillImageMetadata(outputImage.Metadata, imageHandle);
        }

        return outputImage.CloneAs<TPixel>(configuration);
    }

    private static void FillImageMetadata(ImageMetadata metadata, HeifImageHandle imageHandle)
    {
        byte[] exif = imageHandle.GetExifMetadata();
        if (exif != null)
        {
            metadata.ExifProfile = new ExifProfile(exif);
            // The HEIF specification states that the EXIF orientation tag is only
            // informational and should not be used to rotate the image.
            // See https://github.com/strukturag/libheif/issues/227#issuecomment-642165942
            metadata.ExifProfile.RemoveValue(ExifTag.Orientation);
        }

        byte[] xmp = imageHandle.GetXmpMetadata();
        if (xmp != null)
        {
            metadata.XmpProfile = new XmpProfile(xmp);
        }
    }

    private static unsafe Image CreateEightBitImageWithAlpha(HeifImage heifImage, bool premultiplied)
    {
        var image = new Image<Rgba32>(heifImage.Width, heifImage.Height);

        var heifPlaneData = heifImage.GetPlane(HeifChannel.Interleaved);

        byte* srcScan0 = (byte*)heifPlaneData.Scan0;
        int srcStride = heifPlaneData.Stride;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                byte* src = srcScan0 + (y * srcStride);
                var dst = accessor.GetRowSpan(y);

                for (int x = 0; x < accessor.Width; x++)
                {
                    ref var pixel = ref dst[x];

                    if (premultiplied)
                    {
                        byte alpha = src[3];

                        switch (alpha)
                        {
                            case 0:
                                pixel.R = 0;
                                pixel.G = 0;
                                pixel.B = 0;
                                break;
                            case 255:
                                pixel.R = src[0];
                                pixel.G = src[1];
                                pixel.B = src[2];
                                break;
                            default:
                                pixel.R = (byte)Math.Min(MathF.Round(src[0] * 255f / alpha), 255);
                                pixel.G = (byte)Math.Min(MathF.Round(src[1] * 255f / alpha), 255);
                                pixel.B = (byte)Math.Min(MathF.Round(src[2] * 255f / alpha), 255);
                                break;
                        }
                    }
                    else
                    {
                        pixel.R = src[0];
                        pixel.G = src[1];
                        pixel.B = src[2];
                    }
                    pixel.A = src[3];

                    src += 4;
                }
            }
        });

        return image;
    }

    private static unsafe Image CreateEightBitImageWithoutAlpha(HeifImage heifImage)
    {
        var image = new Image<Rgb24>(heifImage.Width, heifImage.Height);

        var heifPlaneData = heifImage.GetPlane(HeifChannel.Interleaved);

        byte* srcScan0 = (byte*)heifPlaneData.Scan0;
        int srcStride = heifPlaneData.Stride;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                byte* src = srcScan0 + (y * srcStride);
                var dst = accessor.GetRowSpan(y);

                for (int x = 0; x < accessor.Width; x++)
                {
                    ref var pixel = ref dst[x];

                    pixel.R = src[0];
                    pixel.G = src[1];
                    pixel.B = src[2];

                    src += 3;
                }
            }
        });

        return image;
    }

    private static unsafe Image CreateSixteenBitImageWithAlpha(HeifImage heifImage, bool premultiplied, int bitDepth)
    {
        var image = new Image<Rgba64>(heifImage.Width, heifImage.Height);

        var heifPlaneData = heifImage.GetPlane(HeifChannel.Interleaved);

        byte* srcScan0 = (byte*)heifPlaneData.Scan0;
        int srcStride = heifPlaneData.Stride;

        int maxChannelValue = (1 << bitDepth) - 1;
        float maxChannelValueFloat = maxChannelValue;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                ushort* src = (ushort*)(srcScan0 + (y * srcStride));
                var dst = accessor.GetRowSpan(y);

                for (int x = 0; x < accessor.Width; x++)
                {
                    ref var pixel = ref dst[x];

                    if (premultiplied)
                    {
                        ushort alpha = src[3];

                        if (alpha == maxChannelValue)
                        {
                            pixel.R = src[0];
                            pixel.G = src[1];
                            pixel.B = src[2];
                        }
                        else
                        {
                            switch (alpha)
                            {
                                case 0:
                                    pixel.R = 0;
                                    pixel.G = 0;
                                    pixel.B = 0;
                                    break;
                                default:
                                    pixel.R = (ushort)Math.Min(MathF.Round(src[0] * maxChannelValueFloat / alpha), maxChannelValue);
                                    pixel.G = (ushort)Math.Min(MathF.Round(src[1] * maxChannelValueFloat / alpha), maxChannelValue);
                                    pixel.B = (ushort)Math.Min(MathF.Round(src[2] * maxChannelValueFloat / alpha), maxChannelValue);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        pixel.R = src[0];
                        pixel.G = src[1];
                        pixel.B = src[2];
                    }
                    pixel.A = src[3];

                    src += 4;
                }
            }
        });

        return image;
    }

    private static unsafe Image CreateSixteenBitImageWithoutAlpha(HeifImage heifImage)
    {
        var image = new Image<Rgb48>(heifImage.Width, heifImage.Height);

        var heifPlaneData = heifImage.GetPlane(HeifChannel.Interleaved);

        byte* srcScan0 = (byte*)heifPlaneData.Scan0;
        int srcStride = heifPlaneData.Stride;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                ushort* src = (ushort*)(srcScan0 + (y * srcStride));
                var dst = accessor.GetRowSpan(y);

                for (int x = 0; x < accessor.Width; x++)
                {
                    ref var pixel = ref dst[x];

                    pixel.R = src[0];
                    pixel.G = src[1];
                    pixel.B = src[2];

                    src += 3;
                }
            }
        });

        return image;
    }
}
