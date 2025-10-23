using Dalamud.Game.Gui.Dtr;

namespace Umbra.BetterWidget.Widgets.DtrFilteredPopup;

public class DtrPopupFilteredWidgetPopup : WidgetPopup
{
    private const string ClassNameDtrEntry = "dtr-bar-entry";
    private const string ClassNameDecorated = "decorated";
    private DtrPopupFilteredWidget Widget { get; }

    private UdtDocument Document { get; } = Utils.DocumentFrom("umbra.widgets._popup_menu.xml");
    protected override Node Node { get; }

    private readonly IDtrBarEntryRepository _repository = Framework.Service<IDtrBarEntryRepository>();

    private readonly Dictionary<string, Node> _entries = [];

    internal DtrPopupFilteredWidgetPopup(DtrPopupFilteredWidget widget)
    {
        Widget = widget;
        Node = Document.RootNode!;
        Document.Stylesheet!.AddRule(".popup", new()
        {
            Padding = new (8, 8, 3, 8),
        });
        Document.Stylesheet!.AddRule("."+ClassNameDtrEntry, new()
        {
            AutoSize = new(AutoSize.Grow, AutoSize.Fit),
            Padding = new (5, 13),
            BackgroundColor = new(0x00000000),
            BorderRadius    = 5,
            StrokeColor     = new(0x00000000),
            StrokeWidth     = 1,
            StrokeInset     = 1,
            StrokeRadius    = 3,
        });
        Document.Stylesheet!.AddRule($".{ClassNameDtrEntry}>#Label", new()
        {
            Anchor       = Anchor.MiddleCenter,
            Color        = new(0xFFB6B6B6),
            OutlineColor = new(0xC0000000),
            OutlineSize  = 1,
            FontSize     = 13
        });
        Document.Stylesheet!.AddRule($".{ClassNameDtrEntry}.{ClassNameDecorated}>#Label", new()
        {
            Padding = new (0, 4),
        });
        Document.Stylesheet!.AddRule($".{ClassNameDtrEntry}.{ClassNameDecorated}", new()
        {
            BackgroundColor = new(0xFF040404),
            StrokeColor     = new(0xFF343434), 
        });
        Document.Stylesheet!.AddRule($".{ClassNameDtrEntry}.{ClassNameDecorated}:hover", new()
        {
            BackgroundColor = new(0xFF202020),
            StrokeColor     = new(0xFF444444), 
        });
        
        _repository.OnEntryAdded   += OnDtrBarEntryAdded;
        _repository.OnEntryRemoved += OnDtrBarEntryRemoved;
        _repository.OnEntryUpdated += OnDtrBarEntryUpdated;
    }

    protected override void UpdateConfigVariables(ToolbarWidget _)
    {
        var entries = Widget.SelectedEntries;

        var toAdd = _repository.GetEntries().Where(e => entries.Contains(e.Name) != Widget.GetConfigValue<bool>("HasBlacklists"));
        var toRemove = _repository.GetEntries().Where(e => entries.Contains(e.Name) == Widget.GetConfigValue<bool>("HasBlacklists"));
        
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
        if (Widget.SelectedEntries.Contains(entry.Name) == Widget.GetConfigValue<bool>("HasBlacklists")) return;
        
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
        if (Widget.SelectedEntries.Contains(entry.Name) == Widget.GetConfigValue<bool>("HasBlacklists")) return;
        
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