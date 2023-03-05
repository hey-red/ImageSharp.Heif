using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif;

public sealed class HeifDecoder : SpecializedImageDecoder<HeifDecoderOptions>
{
    private HeifDecoder()
    {
    }

    /// <summary>
    /// Gets the shared instance.
    /// </summary>
    public static HeifDecoder Instance { get; } = new();

    /// <inheritdoc/>
    protected override ImageInfo Identify(DecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        return new HeifDecoderCore(new() { GeneralOptions = options }).Identify(stream, cancellationToken);
    }

    /// <inheritdoc/>
    protected override Image<TPixel> Decode<TPixel>(HeifDecoderOptions options, Stream stream, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(stream, nameof(stream));

        Image<TPixel> image = new HeifDecoderCore(options).Decode<TPixel>(stream, cancellationToken);

        ScaleToTargetSize(options.GeneralOptions, image);

        return image;
    }

    /// <inheritdoc/>
    protected override Image Decode(HeifDecoderOptions options, Stream stream, CancellationToken cancellationToken) 
        => Decode<Rgba32>(options, stream, cancellationToken);

    /// <inheritdoc/>
    protected override HeifDecoderOptions CreateDefaultSpecializedOptions(DecoderOptions options)
        => new() { GeneralOptions = options };
}