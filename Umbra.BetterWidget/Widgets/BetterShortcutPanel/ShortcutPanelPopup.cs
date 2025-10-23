using Dalamud.Utility;
using Umbra.BetterWidget.Widgets.BetterShortcutPanel.Providers;

namespace Umbra.BetterWidget.Widgets.BetterShortcutPanel;

public sealed partial class ShortcutPanelPopup : ButtonGridPopup
{
    public event Action<string>? OnShortcutsChanged;

    public string   WidgetInstanceId { get; set; } = string.Empty;
    public string[] CategoryNames    { get; set; } = ["Category 1", "Category 2", "Category 3", "Category 4"];
    public byte     NumRows          { get; set; } = 4;
    public byte     NumCols          { get; set; } = 8;
    public bool     ShowEmptySlots   { get; set; } = true;
    public bool     AutoCloseOnUse   { get; set; } = true;

    private ICommandManager            CommandManager { get; } = Framework.Service<ICommandManager>();
    private IChatSender                ChatSender     { get; } = Framework.Service<IChatSender>();
    private ShortcutProviderRepository Providers      { get; } = Framework.Service<ShortcutProviderRepository>();

    private readonly Dictionary<byte, Dictionary<int, ShortcutEntry?>> _buttonActions = new();

    public ShortcutPanelPopup()
    {
        CreateContextMenu();
        UpdateGridDimensions();
    }

    public void LoadShortcuts(string shortcutData)
    {
        try {
            DecodeShortcutData(shortcutData);
        } catch (Exception e) {
            Logger.Error($"Failed to load shortcut data: {e.Message}");
        }
    }

    protected override bool CanOpen()
    {
        return CategoryNames.Length > 0 && NumRows > 0 && NumCols > 0;
    }

    protected override void OnOpen()
    {
        UpdateGridDimensions();
        if (string.IsNullOrEmpty(_shortcutData)) return;

        for (byte c = 0; c < NumCategories; c++) {
            for (int s = 0; s < (NumRows * NumCols); s++) {
                SetButton(c, s, GetShortcutData(c, s));
            }
        }
    }

    private void AssignAction(byte categoryId, int slotId, ShortcutEntry? data)
    {
        if (!_buttonActions.TryGetValue(categoryId, out Dictionary<int, ShortcutEntry?>? slots)) {
            slots                      = new();
            _buttonActions[categoryId] = slots;
        }

        slots[slotId] = data;
    }

    private void InvokeAction(byte categoryId, int slotId)
    {
        if (!_buttonActions.TryGetValue(categoryId, out Dictionary<int, ShortcutEntry?>? slots)) return;
        if (!slots.TryGetValue(slotId, out ShortcutEntry? action)) return;
        if (null == action?.Type) return;

        switch (action)
        {
            case CustomShortcutEntry entry:
                InvokeCustomEntry(entry);
                return;
            case GameShortcutEntry entry:
                AbstractShortcutProvider? provider = Providers.GetProvider(action.Type);
                if (provider == null) return;

                provider.OnInvokeShortcut(categoryId, slotId, entry.Id, WidgetInstanceId);
                return;
        }

        if (AutoCloseOnUse) Close();
    }

    private void InvokeCustomEntry(CustomShortcutEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.Value)) return;
        if (string.IsNullOrWhiteSpace(entry.ActionType)) return;

        string cmd = entry.ActionType.Trim();

        switch (entry.Value) {
            case "Chat":
                if (string.IsNullOrEmpty(cmd) || !cmd.StartsWith('/')) {
                    return;
                }

                if (CommandManager.Commands.ContainsKey(cmd.Split(" ", 2)[0])) {
                    CommandManager.ProcessCommand(cmd);
                    return;
                }

                ChatSender.Send(cmd);
                return;
            case "URL":
                if (!cmd.StartsWith("http://") && !cmd.StartsWith("https://")) {
                    cmd = $"https://{cmd}";
                }

                Util.OpenLink(cmd);
                return;
            default:
                Logger.Warning($"Invalid custom entry type: {entry.Value} for command: {cmd}");
                break;
        }
    }
}
