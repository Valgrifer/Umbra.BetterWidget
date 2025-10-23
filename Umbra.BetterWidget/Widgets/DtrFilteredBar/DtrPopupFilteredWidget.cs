using Dalamud.Game.Gui.Dtr;

namespace Umbra.BetterWidget.Widgets.DtrFilteredBar;

[ToolbarWidget(
    "DtrFilterdBar",
    "Dtr Filtered Bar",
    "Widget.DtrBar.Description",
    ["dtr", "server", "filter", "info", "bar"]
)]
internal sealed partial class DtrPopupFilteredWidget(
    WidgetInfo                  info,
    string?                     guid         = null,
    Dictionary<string, object>? configValues = null
)
    : ToolbarWidget(info, guid, configValues)
{
    public override Node Node { get; } = Utils.DocumentFrom("umbra.widgets.dtr_bar.xml").RootNode!;

    public override WidgetPopup? Popup => null;

    private readonly IDtrBar                _dtrBar     = Framework.Service<IDtrBar>();
    private readonly IDtrBarEntryRepository _repository = Framework.Service<IDtrBarEntryRepository>();
    private readonly IGameGui               _gameGui    = Framework.Service<IGameGui>();

    private readonly Dictionary<string, Node> _entries = [];

    protected override void Initialize()
    {
        _repository.OnEntryAdded   += OnDtrBarEntryAdded;
        _repository.OnEntryRemoved += OnDtrBarEntryRemoved;
        _repository.OnEntryUpdated += OnDtrBarEntryUpdated;
    }

    protected override void OnConfigurationChanged()
    {
        var entries = SelectedEntries;

        var toAdd = _repository.GetEntries().Where(e => entries.Contains(e.Name) != GetConfigValue<bool>("HasBlacklists"));
        var toRemove = _repository.GetEntries().Where(e => entries.Contains(e.Name) == GetConfigValue<bool>("HasBlacklists"));
        
        foreach (var entry in toRemove)
            OnDtrBarEntryRemoved(entry);
        
        foreach (var entry in toAdd)
        {
            if (_entries.ContainsKey(entry.Name)) continue;
            
            OnDtrBarEntryAdded(entry);
        }
    }

    protected override void OnUpdate()
    {
        UpdateNativeServerInfoBar();

        var decorateMode = GetConfigValue<string>("DecorateMode");
        var textOffset   = GetConfigValue<int>("TextYOffset");

        Node.Style.Gap  = GetConfigValue<int>("ItemSpacing");
        Node.Style.Size = new(0, SafeHeight);

        foreach ((string id, Node node) in _entries) {
            switch (decorateMode) {
                case "Always":
                    node.ToggleClass("decorated", true);
                    break;
                case "Never":
                    node.ToggleClass("decorated", false);
                    break;
                case "Auto" when node.IsInteractive:
                    node.ToggleClass("decorated", true);
                    break;
                case "Auto" when !node.IsInteractive:
                    node.ToggleClass("decorated", false);
                    break;
            }

            node.Style.Size = new(0, SafeHeight);

            var entry     = _repository!.Get(id);
            var labelNode = node.FindById("Label");

            if (null == labelNode || entry is not { IsVisible: true }) continue;
            SetNodeLabel(node, entry);
            labelNode.Style.MaxWidth   = MaxTextWidth;
            labelNode.Style.TextOffset = new(0, textOffset);
            labelNode.Style.FontSize   = GetConfigValue<int>("TextSize");
        }
    }

    protected override void OnDisposed()
    {
        _repository.OnEntryAdded   -= OnDtrBarEntryAdded;
        _repository.OnEntryRemoved -= OnDtrBarEntryRemoved;
        _repository.OnEntryUpdated -= OnDtrBarEntryUpdated;

        SetNativeServerInfoBarVisibility(true);
    }

    private void OnDtrBarEntryAdded(DtrBarEntry entry)
    {
        if (SelectedEntries.Contains(entry.Name) == GetConfigValue<bool>("HasBlacklists")) return;
        
        if (_entries.ContainsKey(entry.Name)) {
            OnDtrBarEntryUpdated(entry);
            return;
        }

        Node node = new() {
            ClassList = ["dtr-bar-entry"],
            SortIndex = entry.SortIndex,
            Style = new() {
                Anchor = Anchor.MiddleRight
            },
            ChildNodes = [
                new() {
                    Id          = "Label",
                    NodeValue   = entry.Text,
                    InheritTags = true,
                    Style = new() {
                        MaxWidth     = MaxTextWidth,
                        WordWrap     = false,
                        TextOverflow = false,
                    }
                }
            ]
        };

        if (entry.IsInteractive) {
            node.Tooltip =  entry.TooltipText?.TextValue;
            node.OnClick += _ => entry.InvokeClickAction(MouseClickType.Left, GetModifierKeyState());
            node.OnRightClick += _ => entry.InvokeClickAction(MouseClickType.Right, GetModifierKeyState());
        }

        _entries.Add(entry.Name, node);

        Node.AppendChild(node);
    }

    private static ClickModifierKeys GetModifierKeyState()
    {
        bool shift = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);
        bool ctrl  = ImGui.IsKeyDown(ImGuiKey.LeftCtrl) || ImGui.IsKeyDown(ImGuiKey.RightCtrl);
        bool alt   = ImGui.IsKeyDown(ImGuiKey.LeftAlt) || ImGui.IsKeyDown(ImGuiKey.RightAlt);
        
        ClickModifierKeys modifierKeys = ClickModifierKeys.None;
        
        if (shift) modifierKeys |= ClickModifierKeys.Shift;
        if (ctrl)  modifierKeys |= ClickModifierKeys.Ctrl;
        if (alt)   modifierKeys |= ClickModifierKeys.Alt;
        
        return modifierKeys;
    }

    private void OnDtrBarEntryRemoved(DtrBarEntry entry)
    {
        if (!_entries.TryGetValue(entry.Name, out Node? node)) return;

        node.Remove();

        _entries.Remove(entry.Name);
    }

    private void OnDtrBarEntryUpdated(DtrBarEntry entry)
    {
        if (SelectedEntries.Contains(entry.Name) == GetConfigValue<bool>("HasBlacklists")) return;
        
        if (!_entries.TryGetValue(entry.Name, out Node? node)) return;

        if (node.Style.IsVisible != entry.IsVisible) {
            node.Style.IsVisible = entry.IsVisible;
        }

        if (entry.IsVisible) {
            SetNodeLabel(node, entry);
        }

        node.Tooltip   = entry.TooltipText?.TextValue;
        node.SortIndex = entry.SortIndex;
    }

    private void SetNodeLabel(Node node, DtrBarEntry entry)
    {
        var labelNode = node.FindById("Label");
        if (labelNode == null) return;

        labelNode.NodeValue = GetConfigValue<bool>("PlainText")
            ? entry.Text?.TextValue ?? ""
            : entry.Text;
    }

    private int? MaxTextWidth => GetConfigValue<int>("MaxTextWidth") switch {
        0 => null,
        _ => GetConfigValue<int>("MaxTextWidth")
    };
}
