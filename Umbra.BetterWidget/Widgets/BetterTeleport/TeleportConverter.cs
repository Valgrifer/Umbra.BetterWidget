using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal class TeleportConverter : JsonConverter<TeleportWidgetPopup.TeleportData>
{
    private static TeleportConverter DefaultConverter => new();
    public static JsonSerializerSettings DefaultSettings
    {
        get
        {
            JsonSerializerSettings settings = new();

            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Converters.Add(DefaultConverter);
            
            return settings;
        }
    }

    public override void WriteJson(JsonWriter writer, TeleportWidgetPopup.TeleportData? value, JsonSerializer serializer)
    {
        if (value == null) {
            writer.WriteNull();
            return;
        }
        
        JObject jo = JObject.FromObject(value);
        jo.WriteTo(writer);
    }

    public override TeleportWidgetPopup.TeleportData? ReadJson(JsonReader reader, Type objectType, TeleportWidgetPopup.TeleportData? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        string? type = (string?) jo["T"];

        if (type == null) return null;

        TeleportWidgetPopup.TeleportData? target = type switch
        {
            "Dest" => new TeleportWidgetPopup.TeleportDestinationData(),
            "Misc" => new TeleportWidgetPopup.TeleportMiscellaneousData(),
            "Wrld" => new TeleportWidgetPopup.TeleportWorldData(),
            "Life" => new TeleportWidgetPopup.TeleportLifeSteamData(),
            _ => null
        };
        
        if (target == null) return null;

        serializer.Populate(jo.CreateReader(), target);
        return target;
    }
}