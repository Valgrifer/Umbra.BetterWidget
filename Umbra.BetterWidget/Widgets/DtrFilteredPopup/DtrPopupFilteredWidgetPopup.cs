using Dalamud.Game.Gui.Dtr;

namespace Umbra.BetterWidget.Widgets.DtrFilteredPopup;

public class DtrPopupFilteredWidgetPopup : WidgetPopup
{
    private const string ClassNameDtrEntry = "dtr-bar-entry";
    private const string ClassNameDecorated = "decorated";
    private DtrPopupFilteredWidget Widget { get; }

    protected override Node Node { get; } = Utils.DocumentFrom("umbra.better_widgets._popup_menu.xml").RootNode!;

    private readonly IDtrBarEntryRepository _repository = Framework.Service<IDtrBarEntryRepository>();

    private readonly Dictionary<string, Node> _entries = [];
    
    public int EntryCount => _entries.Where(node => node.Value.Style.IsVisible != false).ToArray().Length;

    internal DtrPopupFilteredWidgetPopup(DtrPopupFilteredWidget widget)
    {
        Widget = widget;
        
        _repository.OnEntryAdded   += OnDtrBarEntryAdded;
        _repository.OnEntryRemoved += OnDtrBarEntryRemoved;
        _repository.OnEntryUpdated += OnDtrBarEntryUpdated;
    }

    protected override void UpdateConfigVariables(ToolbarWidget _)
    {
        var entries = Widget.SelectedEntries;

        var toAdd = _repository.GetEntries().Where(e => entries.Contains(e.Name) != Widget.GetConfigValue<bool>("AsBlacklists"));
        var toRemove = _repository.GetEntries().Where(e => entries.Contains(e.Name) == Widget.GetConfigValue<bool>("AsBlacklists"));
        
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
        var decorateMode = Widget.GetConfigValue<string>("DecorateMode");
        var textOffset   = Widget.GetConfigValue<int>("TextYOffset");

        Node.Style.Gap  = Widget.GetConfigValue<int>("ItemSpacing");
        if (MaxTextWidth != null)
            Node.Style.Size = new((float) (MaxTextWidth! + 26f), 0);
        else
            Node.Style.Size = null;

        foreach ((string id, Node node) in _entries) {
            switch (decorateMode) {
                case "Always":
                    node.ToggleClass(ClassNameDecorated, true);
                    break;
                case "Never":
                    node.ToggleClass(ClassNameDecorated, false);
                    break;
                case "Auto" when node.IsInteractive:
                    node.ToggleClass(ClassNameDecorated, true);
                    break;
                case "Auto" when !node.IsInteractive:
                    node.ToggleClass(ClassNameDecorated, false);
                    break;
            }

            var entry     = _repository.Get(id);
            var labelNode = node.FindById("Label");

            if (null == labelNode || entry is not { IsVisible: true }) continue;
            SetNodeLabel(node, entry);
            labelNode.Style.MaxWidth   = MaxTextWidth;
            labelNode.Style.TextOffset = new(0, textOffset);
            labelNode.Style.FontSize   = Widget.GetConfigValue<int>("TextSize");
        }
    }

    protected override void OnDisposed()
    {
        _repository.OnEntryAdded   -= OnDtrBarEntryAdded;
        _repository.OnEntryRemoved -= OnDtrBarEntryRemoved;
        _repository.OnEntryUpdated -= OnDtrBarEntryUpdated;
    }

    private void OnDtrBarEntryAdded(DtrBarEntry entry)
    {
        if (Widget.SelectedEntries.Contains(entry.Name) == Widget.GetConfigValue<bool>("AsBlacklists")) return;
        
        if ((entry.Text?.Payloads.Count ?? 0) == 0) return;
        
        if (_entries.ContainsKey(entry.Name)) {
            OnDtrBarEntryUpdated(entry);
            return;
        }

        Node node = new() {
            ClassList = [ClassNameDtrEntry],
            SortIndex = entry.SortIndex,
            Style = new() {
                Anchor = Anchor.MiddleCenter,
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
        if (Widget.SelectedEntries.Contains(entry.Name) == Widget.GetConfigValue<bool>("AsBlacklists")) return;
        
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

        labelNode.NodeValue = Widget.GetConfigValue<bool>("PlainText")
            ? entry.Text?.TextValue ?? ""
            : entry.Text;
    }

    private int? MaxTextWidth => Widget.GetConfigValue<int>("MaxTextWidth") switch {
        0 => null,
        _ => Widget.GetConfigValue<int>("MaxTextWidth")
    };
}