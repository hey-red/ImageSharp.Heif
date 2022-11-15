/*
 * Copied from https://github.com/0xC0000054/libheif-sharp/blob/main/src/HeifDecodingOptions.cs
 */

namespace HeyRed.ImageSharp.Heif;

/// <summary>
/// Image decoder options for generating an image out of a heif stream.
/// </summary>
internal interface IHeifDecoderOptions
{
    bool ConvertHdrToEightBit { get; }

    bool IgnoreTransformations { get; }

    bool Strict { get; }
}