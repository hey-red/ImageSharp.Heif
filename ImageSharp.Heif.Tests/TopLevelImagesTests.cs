﻿using HeyRed.ImageSharp.Heif;

namespace ImageSharp.Heif.Tests;

public class TopLevelImagesTests
{
    private readonly HeifDecoderOptions _decoderOptions;

    public TopLevelImagesTests()
    {
        _decoderOptions = new HeifDecoderOptions
        {
            DecodingMode = DecodingMode.TopLevelImages
        };
    }

    [Theory]
    [InlineData("random_collection_1440x960.heic", 4)]
    [InlineData("bird_burst.heic", 4)]
    [InlineData("starfield_animation.heic", 1)]
    [InlineData("stereo_1200x800.heic", 2)]
    public void FramesCountTest(string fileName, int framesCount)
    {
        using var inputStream = File.OpenRead(FixturesUtils.GetFixturePath(fileName));
        using var image = HeifDecoder.Instance.Decode(_decoderOptions, inputStream);

        Assert.Equal(framesCount, image.Frames.Count);
    }

    [Fact]
    public void Multiple_images_with_different_sizes_should_throw()
    {
        using var inputStream = File.OpenRead(FixturesUtils.GetFixturePath("overlay_1000x680.heic"));

        Assert.Throws<ArgumentException>(() => HeifDecoder.Instance.Decode(_decoderOptions, inputStream));
    }
}
