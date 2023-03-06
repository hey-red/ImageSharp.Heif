using System.Collections.Generic;
using System.IO;
using System.Net.Http;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.NuGet;

using Serilog;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

[ShutdownDotNetAfterServerBuild]
public partial class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.All);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    AbsolutePath OutputDirectory => RootDirectory / "output";
    AbsolutePath DownloadDirectory => RootDirectory / "download";
    AbsolutePath BinariesDirectory => RootDirectory / "binaries";

    private const string NATIVE_VERSION = "1.15.1";

    private readonly IEnumerable<string> _architectures = new[] { "linux-x64", "win-x64" };

    private Target DownloadBinaries => _ => _
        .Executes(async () =>
        {
            var httpClient = new HttpClient();
            
            foreach (var architecture in _architectures)
            {
                var fileName = $"libheif-{architecture}.tar.gz";

                var filePath = DownloadDirectory / fileName;
                if (!File.Exists(filePath))
                {
                    var uri = $"https://github.com/hey-red/LibHeif-Build/releases/download/{NATIVE_VERSION}/{fileName}";

                    Log.Information($"Download tarball from {uri}");

                    EnsureExistingDirectory(DownloadDirectory);

                    using var response = await httpClient.GetAsync(uri);
                    await using var fs = new FileStream(filePath, FileMode.CreateNew);
                    await response.Content.CopyToAsync(fs);
                }

                EnsureExistingDirectory(BinariesDirectory);

                Log.Information($"Extract tarball from {fileName}");

                ExtractArchive(filePath, BinariesDirectory / architecture);
            }
        });

    private Target CreateNuGetPackages => _ => _
        .DependsOn(DownloadBinaries)
        .Executes(() =>
        {
            foreach (var architecture in _architectures)
            {
                NuGetPack(p => p
                    .SetTargetPath(RootDirectory / "build/LibHeif.Native." + architecture + ".nuspec")
                    .SetVersion(NATIVE_VERSION)
                    .SetOutputDirectory(OutputDirectory)
                    .AddProperty("NoWarn", "NU5128"));
            }

            NuGetPack(p => p
                .SetTargetPath(RootDirectory / "build/LibHeif.Native.nuspec")
                .SetVersion(NATIVE_VERSION)
                .SetOutputDirectory(OutputDirectory)
                .AddProperty("NoWarn", "NU5128"));
        });

    private Target CleanUp => _ => _
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);
            EnsureCleanDirectory(BinariesDirectory);
        });

    private Target All => _ => _
        .DependsOn(CleanUp)
        .DependsOn(CreateNuGetPackages);
}
