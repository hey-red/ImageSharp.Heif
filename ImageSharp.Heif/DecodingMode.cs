namespace HeyRed.ImageSharp.Heif;

public enum DecodingMode
{
    /// <summary>
    /// Extract the primary image.
    /// </summary>
    PrimaryImage = 0,

    /// <summary>
    /// Extract all top-level images.
    /// </summary>
    TopLevelImages,
}