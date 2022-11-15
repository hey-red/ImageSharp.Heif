using SixLabors.ImageSharp;

namespace HeyRed.ImageSharp.Heif.Formats.Avif;

public class AvifMetadata : IDeepCloneable
{
    public IDeepCloneable DeepClone() => new AvifMetadata();
}