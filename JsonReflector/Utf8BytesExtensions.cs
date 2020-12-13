using System.IO;
using System.Threading.Tasks;

namespace JsonReflector
{
    public static class Utf8BytesExtensions
    {
        public static string AsString(this byte[] bytes) => System.Text.Encoding.UTF8.GetString(bytes);
        public static byte[] AsUtf(this string s) => System.Text.Encoding.UTF8.GetBytes(s);
        public static async Task<byte[]> GetRawBytesAsync(this Stream stream)
        {
            using (var ms = new MemoryStream(2048))
            {
                await stream.CopyToAsync(ms);
                return ms.ToArray();
            }
        }

    }
}
