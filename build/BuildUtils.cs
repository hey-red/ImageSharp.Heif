using System.IO;
using System.Text;

using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

public partial class Build
{
    public void ExtractArchive(string tarballFileName, string destDirectory)
    {
        using var inputStream = File.OpenRead(tarballFileName);
        using var gzipStream = new GZipInputStream(inputStream);

        using var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
        tarArchive.ExtractContents(destDirectory);
    }
}