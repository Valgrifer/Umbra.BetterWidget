using System.Reflection;

namespace Umbra.BetterWidget;

internal sealed class Utils
{
    public static bool Enabled      => GetToolbarConfigFieldValue<bool>("Umbra.Toolbar", "Enabled");
    public static bool IsTopAligned => GetToolbarConfigFieldValue<bool>("Umbra.Toolbar", "IsTopAligned");
    
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
    
    /// <summary>
    /// Retrieves the value of a public static field from the className, as a generic type.
    /// </summary>
    /// <typeparam name="T">The expected type of the field value.</typeparam>
    /// <param name="className">The name of the static field to retrieve.</param>
    /// <param name="fieldName">The name of the static field to retrieve.</param>
    /// <returns>The value of the field cast to the specified generic type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided field name is null or empty.</exception>
    /// <exception cref="TypeLoadException">Thrown when the className type cannot be loaded.</exception>
    /// <exception cref="MissingFieldException">Thrown when the field does not exist or is not public static.</exception>
    /// <exception cref="InvalidCastException">Thrown when the field value cannot be cast to the specified type.</exception>
    public static T GetToolbarConfigFieldValue<T>(string className, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentNullException(nameof(fieldName), "Field name cannot be null or empty.");

        Assembly asm = Framework.Assemblies
                .FirstOrDefault(a => a.GetType(className, false) != null)
            ?? throw new InvalidOperationException("Assembly containing type Umbra.Toolbar not found.");

        Type toolbarConfigType = asm.GetType(className, throwOnError: false)
            ?? throw new InvalidOperationException("Type Umbra.Toolbar not found.");

        if (toolbarConfigType == null)
            throw new TypeLoadException("Failed to load type 'Umbra.Toolbar' from assembly 'Umbra'.");

        var field = toolbarConfigType.GetProperty(fieldName, BindingFlags.Static | BindingFlags.Public);

        if (field == null)
            throw new MissingFieldException($"Public static field '{fieldName}' was not found in Toolbar.");
 
        var value = field.GetValue(null);

        if (value is not T typedValue)
            throw new InvalidCastException($"Field '{fieldName}' cannot be cast to type {typeof(T).FullName}.");

        return typedValue;
    }
}