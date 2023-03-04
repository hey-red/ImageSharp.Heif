using HeyRed.ImageSharp.Heif.Formats.Avif;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;

using Xunit.Abstractions;

namespace ImageSharp.Heif.Tests;

/// <summary>
/// Test files from https://github.com/AOMediaCodec/av1-avif
/// </summary>
public class AvifTests
{
    private readonly DecoderOptions _decoderOptions;

    private readonly ITestOutputHelper _output;

    public AvifTests(ITestOutputHelper output)
    {
        _output = output;
        _decoderOptions = new DecoderOptions()
        {
            Configuration = new Configuration(new AvifConfigurationModule()) 
        };
    }
    
    [Theory]
    [InlineData("abandoned_filmgrain.avif", 1404, 936)]
    [InlineData("fox.profile0.8bpc.yuv420.avif", 1204, 800)]
    [InlineData("hdr_cosmos01650_cicp9-16-9_yuv444_full_qp40.avif", 2048, 858)]
    [InlineData("kids_720p.avif", 1280, 720)]
    [InlineData("Mexico_YUV444.avif", 960, 540)]
    [InlineData("Summer_in_Tomsk_720p_5x4_grid.avif", 6400, 2880)]
    [InlineData("Summer_Nature_4k.avif", 3840, 2160)]
    [InlineData("tiger_3layer_1res.avif", 1216, 832)]
    [InlineData("elster_animated.avif", 1280, 720)]
    public void IdentifyTest(string fileName, int width, int height)
    {
        _output.WriteLine($"Processing file: \"{fileName}\"");

        using var inputStream = File.OpenRead(FixturesUtils.GetFixturePath(fileName));

        ImageInfo imageInfo = Image.Identify(_decoderOptions, inputStream);

        Assert.NotNull(imageInfo);

        _output.WriteLine($"Dimensions: {imageInfo.Width}x{imageInfo.Height}");

        Assert.Equal(width, imageInfo.Width);
        Assert.Equal(height, imageInfo.Height);
    }

    [Theory]
    [InlineData("abandoned_filmgrain.avif")]
    [InlineData("fox.profile0.8bpc.yuv420.avif")]
    [InlineData("hdr_cosmos01650_cicp9-16-9_yuv444_full_qp40.avif")]
    [InlineData("kids_720p.avif")]
    [InlineData("Mexico_YUV444.avif")]
    [InlineData("Summer_in_Tomsk_720p_5x4_grid.avif")]
    [InlineData("Summer_Nature_4k.avif")]
    [InlineData("tiger_3layer_1res.avif")]
    [InlineData("elster_animated.avif")]
    public void LoadImageTest(string fileName)
    {
        using var inputStream = File.OpenRead(FixturesUtils.GetFixturePath(fileName));
        using var outputStream = new MemoryStream();
        using var image = Image.Load(_decoderOptions, inputStream);

        image.Save(outputStream, new JpegEncoder());

        Assert.NotEqual(0, outputStream.Length);
    }
}
