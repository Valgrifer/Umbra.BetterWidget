using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbra.BetterWidget.Widgets.BetterShortcutPanel;

public class ShortcutConverter : JsonConverter<ShortcutPanelPopup.ShortcutEntry>
{
    private static ShortcutConverter DefaultConverter => new();
    public static JsonSerializerSettings DefaultSettings
    {
        get
        {
            JsonSerializerSettings settings = new();

            settings.Converters.Add(DefaultConverter);
            
            return settings;
        }
    }

    public override void WriteJson(JsonWriter writer, ShortcutPanelPopup.ShortcutEntry? value, JsonSerializer serializer)
    {
        if (value == null) {
            writer.WriteNull();
            return;
        }
        
        JObject jo = JObject.FromObject(value);
        jo.WriteTo(writer);
    }

    public override ShortcutPanelPopup.ShortcutEntry? ReadJson(JsonReader reader, Type objectType, ShortcutPanelPopup.ShortcutEntry? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        string? type = (string?) jo["St"];

        if (type == null) return null;

        ShortcutPanelPopup.ShortcutEntry target = type switch
        {
            "CI" => new ShortcutPanelPopup.CustomShortcutEntry(),
            _ => new ShortcutPanelPopup.GameShortcutEntry()
        };

        serializer.Populate(jo.CreateReader(), target);
        return target;
    }
}