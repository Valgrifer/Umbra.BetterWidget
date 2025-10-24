using Dalamud.Interface;

namespace Umbra.BetterWidget.Widgets.DtrFilteredPopup;

[ToolbarWidget(
    "DtrFilterdPopup",
    "Dtr Filtered Popup",
    "Widget.DtrBar.Description",
    ["dtr", "server", "filter", "info", "bar"]
)]
public sealed partial class DtrPopupFilteredWidget
    : StandardToolbarWidget
{
    public override DtrPopupFilteredWidgetPopup Popup { get; }
    
    protected override StandardWidgetFeatures Features => StandardWidgetFeatures.Text |
        StandardWidgetFeatures.Icon |
        StandardWidgetFeatures.CustomizableIcon;

    protected override string DefaultIconType => IconTypeFontAwesome;
    protected override uint DefaultGameIconId => (uint) FontAwesomeIcon.Bars;

    private readonly IDtrBar _dtrBar = Framework.Service<IDtrBar>();

    public DtrPopupFilteredWidget(
        WidgetInfo                  info,
        string?                     guid         = null,
        Dictionary<string, object>? configValues = null
    ) : base(info, guid, configValues)
    {
        Popup = new(this);
    }

    protected override void OnDraw()
    {
        IsVisible = Popup.EntryCount > 0;
    }
}
