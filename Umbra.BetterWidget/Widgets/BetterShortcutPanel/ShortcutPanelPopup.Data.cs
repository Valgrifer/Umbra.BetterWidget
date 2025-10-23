using Newtonsoft.Json;
using Umbra.Common.Utility;

namespace Umbra.BetterWidget.Widgets.BetterShortcutPanel;

public sealed partial class ShortcutPanelPopup
{
    private Dictionary<byte, Dictionary<int, ShortcutEntry>> _shortcuts    = [];
    private string                                           _shortcutData = "{}";

    private void AssignShortcut(byte category, int index, ShortcutEntry? shortcut)
    {
        if (!_shortcuts.TryGetValue(category, out Dictionary<int, ShortcutEntry>? slots)) {
            slots                = [];
            _shortcuts[category] = slots;
        }

        if (shortcut == null) {
            slots.Remove(index);
            EncodeShortcutData();
            return;
        }

        slots[index] = shortcut;
        EncodeShortcutData();
    }

    private ShortcutEntry? GetShortcutData(byte category, int index)
    {
        if (_shortcuts.TryGetValue(category, out Dictionary<int, ShortcutEntry>? slots)) {
            if (slots.TryGetValue(index, out ShortcutEntry? shortcut)) {
                return shortcut;
            }
        }

        return null;
    }

    private void EncodeShortcutData()
    {
        string oldData = _shortcutData;
        _shortcutData = $"SPD|{Compression.Compress(JsonConvert.SerializeObject(_shortcuts, ShortcutConverter.DefaultSettings))}";
        if (oldData != _shortcutData) OnShortcutsChanged?.Invoke(_shortcutData);
    }

    private void DecodeShortcutData(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        string[] parts = data.Split('|');

        if (parts.Length != 2) throw new("Invalid shortcut data. Missing header.");
        if (parts[0] != "SPD") throw new("Invalid shortcut data. Header mismatch.");

        _shortcutData = data;
        _shortcuts = JsonConvert.DeserializeObject<Dictionary<byte, Dictionary<int, ShortcutEntry>>>(Compression.Decompress(parts[1]), ShortcutConverter.DefaultSettings) ?? [];
    }

    // Contains short property names to reduce the size of the JSON data.
    [Serializable]
    public class ShortcutEntry
    {
        /// <summary>
        /// The type id of the provider that provided this item.
        /// Applicable to ShortcutProvider items only.
        /// </summary>
        [JsonProperty("St")] public string Type { get; set; } = null!;

        public string Serialize()
        {
            return Compression.Compress(JsonConvert.SerializeObject(this, ShortcutConverter.DefaultSettings));
        }
        
        public static ShortcutEntry? Deserialize(string data)
        {
            return JsonConvert.DeserializeObject<ShortcutEntry>(Compression.Decompress(data), ShortcutConverter.DefaultSettings);
        }
    }
    
    [Serializable]
    public class GameShortcutEntry: ShortcutEntry
    {
        /// <summary>
        /// The ID of the item that was picked using a provider.
        /// Applicable to ShortcutProvider items only.
        /// </summary>
        [JsonProperty("Pi")] public uint Id { get; set; }
    }
    
    [Serializable]
    public class CustomShortcutEntry: ShortcutEntry
    {
        public CustomShortcutEntry()
        {
            Type = "CI";
        }
        
        /// <summary>
        /// The item label.
        /// Applicable to custom items only.
        /// </summary>
        [JsonProperty("Cl")] public string Label { get; set; } = null!;

        /// <summary>
        /// The item icon ID.
        /// Applicable to custom items only.
        /// </summary>
        [JsonProperty("Ci")] public uint IconId { get; set; }

        /// <summary>
        /// The item icon color.
        /// Applicable to custom items only.
        /// </summary>
        [JsonProperty("Cj")] public uint IconColor { get; set; } = 0xFFFFFFFF;

        /// <summary>
        /// The chat command or website URL.
        /// Applicable to custom items only.
        /// </summary>
        [JsonProperty("Cc")] public string ActionType { get; set; } = null!;

        /// <summary>
        /// The type of custom command (ChatCommand or WebLink).
        /// Applicable to custom items only.
        /// </summary>
        [JsonProperty("Ct")] public string Value { get; set; } = null!;
    }
}
