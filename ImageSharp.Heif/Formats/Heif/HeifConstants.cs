namespace HeyRed.ImageSharp.Heif.Formats.Heif;

/// <summary>
/// TODO: C# 11 string literals
/// Brand headers taken from https://github.com/strukturag/libheif/blob/master/libheif/heif.cc#L155
/// </summary>
internal static class HeifConstants
{
    public static readonly IEnumerable<string> FileExtensions = new[] { "heif", "heic" };

    public static readonly IEnumerable<string> MimeTypes = new[] { "image/heif", "image/heic" };

    public static readonly byte[] HeifBrandHeader =
    {
        0x68,   // h
        0x65,   // e
        0x69,   // i
        0x63,   // c
    };

    public static readonly byte[] HeimBrandHeader =
    {
        0x68,   // h
        0x65,   // e
        0x69,   // i
        0x6d,   // m
    };

    public static readonly byte[] HeisBrandHeader =
    {
        0x68,   // h
        0x65,   // e
        0x69,   // i
        0x73,   // s
    };

    public static readonly byte[] HeixBrandHeader =
    {
        0x68,   // h
        0x65,   // e
        0x69,   // i
        0x78,   // x
    };

    public static readonly byte[] HevcBrandHeader =
    {
        0x68,   // h
        0x65,   // e
        0x76,   // v
        0x63,   // c
    };

    public static readonly byte[] HevxBrandHeader =
    {
        0x68,   // h
        0x65,   // e
        0x76,   // v
        0x78,   // x
    };

    public static readonly byte[] HevmBrandHeader =
    {
        0x68,   // h
        0x65,   // e
        0x76,   // v
        0x6d,   // m
    };

    public static readonly byte[] HevsBrandHeader =
    {
        0x68,   // h
        0x65,   // e
        0x76,   // v
        0x73,   // s
    };

    public static readonly byte[] Mif1BrandHeader =
    {
        0x6d,   // m
        0x69,   // i
        0x66,   // f
        0x31,   // 1
    };

    public static readonly byte[] Msf1BrandHeader =
    {
        0x6d,   // m
        0x73,   // s
        0x66,   // f
        0x31,   // 1
    };
}