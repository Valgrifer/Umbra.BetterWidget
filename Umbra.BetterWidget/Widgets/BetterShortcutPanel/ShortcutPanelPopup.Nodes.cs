
using Umbra.BetterWidget.Widgets.BetterShortcutPanel.Providers;

namespace Umbra.BetterWidget.Widgets.BetterShortcutPanel;

public sealed partial class ShortcutPanelPopup
{
    private Node?  _selectedSlotNode;
    private byte   _selectedCategory;
    private int    _selectedSlotIndex;
    private byte   _numRows;
    private byte   _numCols;
    private string _catHash = string.Empty;

    private void UpdateGridDimensions()
    {
        string catHash = string.Join('$', CategoryNames);

        if (_catHash != catHash || _numRows != NumRows || _numCols != NumCols) {
            _numRows = NumRows;
            _numCols = NumCols;
            _catHash = catHash;

            ReconfigurePanel(CategoryNames, _numRows, _numCols);
        }
    }

    protected override void OnButtonSlotCreated(Node node, byte categoryId, int slotId)
    {
        node.OnMouseUp += _ => InvokeAction(categoryId, slotId);

        node.OnRightClick += n => {
            string[] parts = n.Id!.Split('_');
            _selectedSlotNode  = n;
            _selectedCategory  = byte.Parse(parts[1]);
            _selectedSlotIndex = int.Parse(parts[2]);

            ShortcutEntry? data = GetShortcutData(_selectedCategory, _selectedSlotIndex);

            ContextMenu?.SetEntryVisible("AddCustomItem", data == null);
            foreach(AbstractShortcutProvider provider in Providers.GetAllProviders()) {
                ContextMenu?.SetEntryVisible(provider.ShortcutType, data == null);
            }
            ContextMenu?.SetEntryVisible("-", data == null);
            
            ContextMenu?.SetEntryDisabled("Copy", data == null);
            ContextMenu?.SetEntryDisabled("Clear", data == null);
            ContextMenu?.SetEntryVisible("Configure", data is CustomShortcutEntry);

            string clipboardText = ImGui.GetClipboardText();
            ContextMenu?.SetEntryDisabled("Paste", string.IsNullOrEmpty(clipboardText) || !clipboardText.StartsWith("SC:"));

            ContextMenu?.Present();
        };
    }

    private void SetButton(byte categoryId, int slotId, ShortcutEntry? data)
    {
        Node? node = GetSlotContainer(categoryId).QuerySelector($"#Slot_{categoryId}_{slotId}");
        if (node == null) return;

        if (data == null) {
            ClearSlot(node, categoryId, slotId);
            return;
        }

        switch (data)
        {
            case CustomShortcutEntry entry:
                node.TagsList.Remove("empty");
                node.TagsList.Remove("empty-hidden");
                node.TagsList.Remove("empty-visible");
            
                SetSlotState(node, new() {
                    Id = 0,
                    Name = entry.Cl,
                    IconId = entry.Ci,
                    IconColor = entry.Cj,
                });
                AssignAction(categoryId, slotId, data);
                return;
            case GameShortcutEntry entry:
                string typeId = entry.St;
                
                uint itemId = entry.Pi;

                if (typeId == string.Empty) {
                    ClearSlot(node, categoryId, slotId);
                    return;
                }

                AbstractShortcutProvider? provider = Providers.GetProvider(typeId);
                if (provider == null) {
                    Logger.Error($"No provider found for shortcut type: {typeId}");
                    return;
                }

                Shortcut? shortcut = provider.GetShortcut(itemId, WidgetInstanceId);

                if (shortcut == null) {
                    Logger.Warning($"Shortcut could not be constructed: {typeId}:{itemId}");
                    ClearSlot(node, categoryId, slotId);
                    return;
                }

                node.TagsList.Remove("empty");
                node.TagsList.Remove("empty-hidden");
                node.TagsList.Remove("empty-visible");

                SetSlotState(node, shortcut.Value);
                AssignAction(categoryId, slotId, entry);
                return;
        }
    }

    private void ClearSlot(Node slotNode, byte categoryId, int slotId)
    {
        slotNode.Tooltip = null;

        slotNode.QuerySelector(".icon")!.Style.IconId     = null;
        slotNode.QuerySelector(".sub-icon")!.Style.IconId = null;
        slotNode.QuerySelector(".count")!.NodeValue       = null;

        slotNode.TagsList.Add("empty");
        slotNode.TagsList.Add($"empty-{(ShowEmptySlots ? "visible" : "hidden")}");
        slotNode.TagsList.Remove($"empty-{(ShowEmptySlots ? "hidden" : "visible")}");
        slotNode.TagsList.Remove("blocked");

        AssignAction(categoryId, slotId, null);
        AssignShortcut(categoryId, slotId, null);
    }

    private static void SetSlotState(Node slotNode, Shortcut shortcut)
    {
        var iconNode = slotNode.QuerySelector(".icon")!;
        iconNode.Style.IconId                             = shortcut.IconId;
        if (shortcut.IconColor != 0)
            iconNode.Style.ImageColor = new(shortcut.IconColor);
        slotNode.QuerySelector(".sub-icon")!.Style.IconId = shortcut.SubIconId;
        slotNode.QuerySelector(".count")!.NodeValue       = shortcut.Count?.ToString();

        slotNode.Tooltip = shortcut.Name;
    }
}
