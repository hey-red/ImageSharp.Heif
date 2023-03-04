# ImageSharp.Heif

HEIF/AVIF decoder for [ImageSharp](https://github.com/SixLabors/ImageSharp)

> **🚧 WIP. Some features(like encoder) is not implemented.**

> Use [libheif-sharp](https://github.com/0xC0000054/libheif-sharp) for all features provided by [libheif](https://github.com/strukturag/libheif).

## Install
via [NuGet](https://www.nuget.org/packages/HeyRed.ImageSharp.Heif):
```
PM> Install-Package HeyRed.ImageSharp.Heif
```
Native libs for **x64** Linux and Windows:
```
PM> Install-Package LibHeif.Native
```
Separate native packages:

```
PM> Install-Package LibHeif.Native.win-x64
PM> Install-Package LibHeif.Native.linux-x64
```

## Usage

```C#
using System.IO;

using SixLabors.ImageSharp;

using HeyRed.ImageSharp.Heif.Formats.Heif;
using HeyRed.ImageSharp.Heif.Formats.Avif;

var decoderOptions = new DecoderOptions()
{
    Configuration = new Configuration(new AvifConfigurationModule())
};

using var inputStream = File.OpenRead("/path/to/image.avif"); // or image.heic

using var image = Image.Load(decoderOptions, inputStream);

// Do something with image
...
```

# Top level images
⚠️ Due to the ImageSharp [limitations](https://github.com/SixLabors/ImageSharp/discussions/1982#discussioncomment-2132564), decoding top level images with different sizes are not supported. ⚠️

Note: libheif have some [limitations](https://github.com/strukturag/heif-gimp-plugin/issues/6)


By default DecodingMode set to PrimaryImage, but if you want decode all top level images see example listed below:

```C#
var decoderOptions = new HeifDecoderOptions
{
    DecodingMode = DecodingMode.TopLevelImages
};

using var inputStream = File.OpenRead("/path/to/image.avif"); // or image.heic
using var image = HeifDecoder.Instance.Decode(decoderOptions, inputStream);

// Saves all frames
for (int i = 0; i < image.Frames.Count; i++)
{
    image.Frames
        .CloneFrame(i)
        .SaveAsJpeg($"frame{i}.jpg");
}
```

## License
[MIT](LICENSE)
