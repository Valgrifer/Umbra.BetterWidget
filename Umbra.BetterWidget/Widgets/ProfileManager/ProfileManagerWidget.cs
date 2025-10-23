using Newtonsoft.Json;
using Umbra.Common.Utility;

namespace Umbra.BetterWidget.Widgets.ProfileManager;

[ToolbarWidget(
    "ProfileManager",
    "Collection Manager Widget",
    "A widget for manage Dalamud Plugin Collection.",
    ["button", "collection", "menu", "list"]
)]
internal sealed partial class ProfileManagerWidget(
    WidgetInfo                  info,
    string?                     guid         = null,
    Dictionary<string, object>? configValues = null
) : StandardToolbarWidget(info, guid, configValues)
{
    public override ProfileManagerPopup Popup { get; } = new();
    
    protected override StandardWidgetFeatures Features =>
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.Icon |
        StandardWidgetFeatures.CustomizableIcon;

    protected override string DefaultIconType => IconTypeGameIcon;
    protected override uint DefaultGameIconId => 65067;

    public override string GetInstanceName()
    {
        return $"Collection Manager Widget - {GetConfigValue<string>("ButtonLabel")}";
    }
    
    protected override void OnLoad()
    {
        Popup.OnEditModeChanged += OnEditModeChanged;
        Popup.OnEntriesChanged  += OnEntriesChanged;

        string data = GetConfigValue<string>("ExtraDataEntries");

        if (string.IsNullOrEmpty(data)) return;

        try {
            var entries =
                JsonConvert.DeserializeObject<List<ProfileManagerPopup.ProfileExtraData>>(Compression.Decompress(data));

            if (null != entries) {
                Popup.Entries = entries;
            }
        } catch (Exception e) {
            Logger.Error($"Failed to load dynamic menu entries: {e.Message}");
        }
    }

    protected override void OnUnload()
    {
        Popup.OnEditModeChanged -= OnEditModeChanged;
        Popup.OnEntriesChanged  -= OnEntriesChanged;
    }

    protected override void OnConfigurationChanged()
    {
        Popup.EditModeEnabled  = GetConfigValue<bool>("EditModeEnabled");
        Popup.EntryHeight      = GetConfigValue<int>("MenuEntryHeight");
        Popup.EntryFontSize    = GetConfigValue<int>("MenuFontSize");

        SetText(GetConfigValue<string>("ButtonLabel"));

        string tooltip = GetConfigValue<string>("ButtonTooltip").Trim();
        Node.Tooltip = string.IsNullOrEmpty(tooltip) ? null : tooltip;
    }

    private void OnEditModeChanged(bool state)
    {
        SetConfigValue("EditModeEnabled", state);
    }

    private void OnEntriesChanged()
    {
        string data = Compression.Compress(JsonConvert.SerializeObject(Popup.Entries));
        SetConfigValue("ExtraDataEntries", data);
    }
}