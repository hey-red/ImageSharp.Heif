using HeyRed.ImageSharp.Heif.Formats.Avif;

using LibHeifSharp;

using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Xmp;

namespace HeyRed.ImageSharp.Heif;

public static class MathF
{
    public static float Round(float a) => (float)Math.Round(a);  
}

internal static unsafe class HeifImageUtils
{
    public static void FillImageMetadata(ImageMetadata metadata, HeifImageHandle imageHandle)
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

    public static unsafe Image CreateEightBitImageWithAlpha(HeifImage heifImage, bool premultiplied)
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

    public static unsafe Image CreateEightBitImageWithoutAlpha(HeifImage heifImage)
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

    public static unsafe Image CreateSixteenBitImageWithAlpha(HeifImage heifImage, bool premultiplied, int bitDepth)
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

    public static unsafe Image CreateSixteenBitImageWithoutAlpha(HeifImage heifImage)
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