namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup : WidgetPopup
{
    public event Action<string>? OnFavoritesChanged;
    
    public bool ReverseCondensedElements { get; set; }
    
    protected override Node Node { get; }

    private UdtDocument Document { get; }

    public TeleportWidgetPopup()
    {
        Document = Utils.DocumentFrom("umbra.widgets.popup_teleport.xml");
        Node     = Document.RootNode!;

        Node.Style.Margin =  new (3, 6, 0, 0);
    }

    protected override void OnOpen()
    {
        HydrateAetherytePoints();
        BuildContextMenu();
        BuildInterfaces();
    }

    protected override void OnClose()
    {
    }
    
    public void LoadFavorites(string favoritesData)
    {
        try {
            DecodeLoadFavorites(favoritesData);
        } catch (Exception e) {
            Logger.Error($"Failed to load favorites data: {e.Message}");
        }
    }

    protected override void OnUpdate()
    {
        UpdateInterface();
    }
}
