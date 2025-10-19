
using Dalamud.Utility;

namespace Umbra.BetterWidget.Widgets.BetterShortcutPanel;

[ToolbarWidget("BetterShortcutPanel", "Better Shortcut Panel", "Widget.ShortcutPanel.Description", ["shortcut", "panel", "hotbar", "action", "ability", "macro", "emote", "item", "macro", "command", "url", "website", "menu"])]
internal sealed partial class ShortcutPanelWidget(
    WidgetInfo                  info,
    string?                     guid         = null,
    Dictionary<string, object>? configValues = null
) : StandardToolbarWidget(info, guid, configValues)
{
    public override ShortcutPanelPopup Popup { get; } = new();

    protected override StandardWidgetFeatures Features =>
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.Icon |
        StandardWidgetFeatures.CustomizableIcon;

    private string _lastShortcutData = string.Empty;
    
    private IChatSender      ChatSender      { get; } = Framework.Service<IChatSender>();
    private ICommandManager  CommandManager  { get; } = Framework.Service<ICommandManager>();

    protected override void OnLoad()
    {
        Popup.OnShortcutsChanged += OnShortcutsChanged;
        Node.OnRightClick += OnRightClick;
    }

    protected override void OnUnload()
    {
        Popup.OnShortcutsChanged -= OnShortcutsChanged;
        Node.OnRightClick += OnRightClick;
    }

    public override string GetInstanceName()
    {
        return $"{Info.Name} - {GetConfigValue<string>("ButtonLabel")}";
    }

    protected override void OnDraw()
    {
        Popup.WidgetInstanceId = Id;
        Popup.NumRows          = (byte)GetConfigValue<int>("NumRows");
        Popup.NumCols          = (byte)GetConfigValue<int>("NumCols");
        Popup.ShowEmptySlots   = GetConfigValue<bool>("ShowEmptySlots");
        Popup.AutoCloseOnUse   = GetConfigValue<bool>("AutoCloseOnUse");

        UpdateNodeCategoryNames();

        string shortcutData = GetConfigValue<string>("SlotConfig");

        if (shortcutData != _lastShortcutData) {
            Popup.LoadShortcuts(shortcutData);
            _lastShortcutData = shortcutData;
        }

        SetText(GetConfigValue<string>("ButtonLabel"));
    }

    private void UpdateNodeCategoryNames()
    {
        List<string> categoryNames = [];

        for (var i = 0; i < 4; i++) {
            string name = GetConfigValue<string>($"CategoryName_{i}");
            if (!string.IsNullOrEmpty(name.Trim())) categoryNames.Add(name);
        }

        Popup.CategoryNames = categoryNames.ToArray();
    }

    private void OnShortcutsChanged(string shortcutData)
    {
        SetConfigValue("SlotConfig", shortcutData);
    }

    private void OnRightClick(Node _)
    {
        string mode = GetConfigValue<string>("AltMode");
        string command = GetConfigValue<string>("AltCommand").Trim();
        switch (mode) {
            case "Command":
                if (string.IsNullOrEmpty(command) || !command.StartsWith('/')) {
                    return;
                }

                if (CommandManager.Commands.ContainsKey(command.Split(" ", 2)[0])) {
                    CommandManager.ProcessCommand(command);
                    return;
                }

                ChatSender.Send(command);
                return;
            case "URL":
                if (!command.StartsWith("http://") && !command.StartsWith("https://")) {
                    command = $"https://{command}";
                }
                Util.OpenLink(command);
                return;
        }
    }
}
