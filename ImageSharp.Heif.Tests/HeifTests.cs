using HeyRed.ImageSharp.Heif.Formats.Heif;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

using Xunit.Abstractions;

namespace ImageSharp.Heif.Tests;

/// <summary>
/// Test files from http://nokiatech.github.io/heif/examples.html
/// </summary>
public class HeifTests
{
    private readonly Configuration _configuration;

    private readonly ITestOutputHelper _output;

    public HeifTests(ITestOutputHelper output)
    {
        _output = output;
        _configuration = new Configuration(new HeifConfigurationModule());
    }

    [Theory]
    [InlineData("cheers_1440x960.heic", 1440, 960)]
    [InlineData("random_collection_1440x960.heic", 1440, 960)]
    [InlineData("bird_burst.heic", 1280, 720)]
    [InlineData("starfield_animation.heic", 256, 144)]
    [InlineData("overlay_1000x680.heic", 1000, 680)]
    [InlineData("alpha_1440x960.heic", 1440, 960)]
    [InlineData("stereo_1200x800.heic", 1200, 800)]
    [InlineData("bothie_1440x960.heic", 1440, 960)]
    [InlineData("lights_1440x960.heic", 1440, 960)]
    public void IdentifyTest(string fileName, int width, int height)
    {
        _output.WriteLine($"Processing file: \"{fileName}\"");

        using var inputStream = File.OpenRead(FixturesUtils.GetFixturePath(fileName));
        IImageInfo imageInfo = Image.Identify(_configuration, inputStream);

        Assert.NotNull(imageInfo);

        _output.WriteLine($"Dimensions: {imageInfo.Width}x{imageInfo.Height}");

        Assert.Equal(width, imageInfo.Width);
        Assert.Equal(height, imageInfo.Height);
    }

    [Theory]
    [InlineData("cheers_1440x960.heic")]
    [InlineData("random_collection_1440x960.heic")]
    [InlineData("bird_burst.heic")]
    [InlineData("starfield_animation.heic")]
    [InlineData("overlay_1000x680.heic")]
    [InlineData("alpha_1440x960.heic")]
    [InlineData("stereo_1200x800.heic")]
    [InlineData("bothie_1440x960.heic")]
    [InlineData("lights_1440x960.heic")]
    public void LoadImageTest(string fileName)
    {
        using var inputStream = File.OpenRead(FixturesUtils.GetFixturePath(fileName));
        using var outputStream = new MemoryStream();
        using var image = Image.Load(_configuration, inputStream);

        image.Save(outputStream, new JpegEncoder());

        Assert.NotEqual(0, outputStream.Length);
    }
}