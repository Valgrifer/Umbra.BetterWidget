using System.IO;
using System.IO.Compression;
using System.Text;

namespace Umbra.BetterWidget;

internal sealed class Utils
{
    [ConfigVariable("Toolbar.IsTopAligned", "General", "Toolbar")]
    public static bool IsTopAligned { get; set; } = false;
    
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
}