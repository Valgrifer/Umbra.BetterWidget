namespace Umbra.BetterWidget.Widgets.ProfileManager;

internal sealed partial class ProfileManagerPopup : WidgetPopup
{
    public bool                   EditModeEnabled  { get; set; }
    public int                    EntryHeight      { get; set; } = 36;
    public int                    EntryFontSize    { get; set; } = 13;
    public int                    AltEntryFontSize { get; set; } = 11;
    public List<ProfileExtraData> Entries          { get; set; } = [];

    public event Action?       OnEntriesChanged;
    public event Action<bool>? OnEditModeChanged;

    protected override Node Node { get; }

    private UdtDocument Document { get; } = Utils.DocumentFrom("umbra.widgets.popup_dynamic_menu.xml");

    public ProfileManagerPopup()
    {
        Node = Document.RootNode!;
        
        CreateContextMenu();

        EmptyButtonPlaceholder.NodeValue = "Edit Mode";
        EmptyButtonPlaceholder.OnRightClick += _ => OpenContextMenu();
        EmptyButtonPlaceholder.OnClick      += _ => { };

        Framework.DalamudFramework.Run(RebuildMenu);
    }

    protected override bool CanOpen()
    {
        return EditModeEnabled || Entries.Count > 0;
    }

    protected override void OnOpen()
    {
        EmptyButtonPlaceholder.Style.IsVisible   = EditModeEnabled;
        EmptyButtonPlaceholder.Style.BorderWidth = new() { Top = Entries.Count > 0 ? 1 : 0 };

        RebuildMenu();
    }

    private void RebuildMenu()
    {
        ClearMenu();
        
        List<Guid> list = new List<Guid>();

        foreach (var profile in GetProfiles())
        {
            ProfileExtraData entry = GetEntry(profile);
            Node node = CreateEntryNode(profile, entry);
            list.Add(profile.Guid);

            ItemList.AppendChild(node);
        }
        
        CleanEntries(list);
    }

    private void ClearMenu()
    {
        foreach (var node in ItemList.ChildNodes.ToArray()) {
            node.Dispose();
        }
    }

    private void InvokeProfileEntry(ProfileExtraData entry)
    {
        ProfileWrapper? profile = GetProfile(entry);
        
        if (profile == null) return;

        profile.SetStateAsync(!profile.IsEnabled);
        Close();
    }
}
