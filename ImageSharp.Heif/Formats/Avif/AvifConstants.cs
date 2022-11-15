namespace HeyRed.ImageSharp.Heif.Formats.Avif;

internal static class AvifConstants
{
    public static readonly IEnumerable<string> FileExtensions = new[] { "avif" };

    public static readonly IEnumerable<string> MimeTypes = new[] { "image/avif" };

    public static readonly byte[] AvifBrandHeader =
    {
        0x61,   // a
        0x76,   // v
        0x69,   // i
        0x66,   // f
    };

    // TODO: Some AVIS are not supported.
    // See https://github.com/strukturag/libheif/issues/377
    public static readonly byte[] AvisBrandHeader =
    {
        0x61,   // a
        0x76,   // v
        0x69,   // i
        0x73,   // s
    };
}
