using System.Reflection;

namespace ImageSharp.Heif.Tests
{
    internal static class FixturesUtils
    {
        private static string GetFixturesDir()
        {
            var assemblyLocation = typeof(FixturesUtils).GetTypeInfo().Assembly.Location;
            var assemblyFile = new FileInfo(assemblyLocation);
            var directoryInfo = assemblyFile.Directory;

            return Path.Combine(directoryInfo!.FullName, "Fixtures");
        }

        public static string FixturesDir { get; } = GetFixturesDir();

        public static string GetFixturePath(string fileName) => Path.Combine(FixturesDir, fileName);
    }
}
