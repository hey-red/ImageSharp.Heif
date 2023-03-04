using SixLabors.ImageSharp.Formats;

namespace HeyRed.ImageSharp.Heif;

/// <summary>
/// Image decoder options for generating an image out of a heif stream.
/// </summary>
public sealed class HeifDecoderOptions : ISpecializedDecoderOptions
{
    /// <inheritdoc/>
    public DecoderOptions GeneralOptions { get; init; } = new();

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
}