using System.IO;
using System.IO.Compression;
using System.Text;

namespace Umbra.BetterWidget;

internal sealed class Utils
{
    [ConfigVariable("BetterTeleport.Favorites")]
    private static string FavoritesData { get; set; } = "";
    
    internal static UdtDocument DocumentFrom(string resourceName)
    {
        foreach (var assembly in Framework.Assemblies) {
            var resource = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
            
            if (resource == null) continue;
            
            return UdtLoader.LoadFromAssembly(assembly, resourceName);
        }
        
        throw new Exception($"No UDT document with the name \"{resourceName}\" exists in any assembly.");
    }

    internal static string Encode(string text)
    {
        byte[]    bytes  = Encoding.UTF8.GetBytes(text);
        using var output = new MemoryStream();

        using (var deflateStream = new DeflateStream(output, CompressionLevel.SmallestSize)) {
            deflateStream.Write(bytes, 0, bytes.Length);
        }

        bytes = output.ToArray();

        return Convert.ToBase64String(bytes);
    }

    internal static string Decode(string text, string defaultValue = "{}")
    {
        if (string.IsNullOrEmpty(text)) return defaultValue;

        byte[]    bytes  = Convert.FromBase64String(text);
        using var input  = new MemoryStream(bytes);
        using var output = new MemoryStream();

        using (var deflateStream = new DeflateStream(input, CompressionMode.Decompress)) {
            deflateStream.CopyTo(output);
        }

        return Encoding.UTF8.GetString(output.ToArray());
    }
}