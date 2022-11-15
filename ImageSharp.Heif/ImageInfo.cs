using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;

namespace HeyRed.ImageSharp.Heif;

/// <summary>
/// Contains information about the image including dimensions, pixel type information and additional metadata
/// </summary>
internal sealed class ImageInfo : IImageInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageInfo"/> class.
    /// </summary>
    /// <param name="pixelType">The image pixel type information.</param>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <param name="metadata">The images metadata.</param>
    public ImageInfo(PixelTypeInfo pixelType, int width, int height, ImageMetadata metadata)
    {
        PixelType = pixelType;
        Width = width;
        Height = height;
        Metadata = metadata;
    }

    /// <inheritdoc />
    public PixelTypeInfo PixelType { get; }

    /// <inheritdoc />
    public int Width { get; }

    /// <inheritdoc />
    public int Height { get; }

    /// <inheritdoc />
    public ImageMetadata Metadata { get; }
}
