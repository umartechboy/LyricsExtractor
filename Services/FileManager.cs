using System.Globalization;
using System.Reflection;

namespace LyricsExtractor.Services
{
    public class FileManager
    {
        public static string Root { get; set; } = "";

        public static byte[] GetBytes(string path)
        {
            var p = Path.Combine(Root, path.Trim('/')).Replace("/", ".").Trim('.');
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(p))
            {
                using (var ms = new MemoryStream())
                {
                    s.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
        public static Stream GetStream(string path)
        {
            return new MemoryStream(GetBytes(path));
        }
    }
}
