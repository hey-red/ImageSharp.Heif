# ImageSharp.Heif

HEIF/AVIF decoder for [ImageSharp](https://github.com/SixLabors/ImageSharp)

> **🚧 WIP. Some features(like encoder, thumbnails) is not implemented.**

> Use [libheif-sharp](https://github.com/0xC0000054/libheif-sharp) for all features provided by [libheif](https://github.com/strukturag/libheif).

## Install
via [NuGet](https://www.nuget.org/packages/HeyRed.ImageSharp.Heif):
```
PM> Install-Package HeyRed.ImageSharp.Heif -Version 1.0.0-alpha.2
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

var configuration = new Configuration(
    new AvifConfigurationModule(),
    new HeifConfigurationModule());

using var inputStream = File.OpenRead("/path/to/image.avif"); // or image.heic

using var image = Image.Load(configuration, inputStream);

// Do something with image
...
```
