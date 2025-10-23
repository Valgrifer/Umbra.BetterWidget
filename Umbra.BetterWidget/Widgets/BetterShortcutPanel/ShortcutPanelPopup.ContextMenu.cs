
using Umbra.BetterWidget.Widgets.BetterShortcutPanel.Providers;
using Umbra.BetterWidget.Widgets.BetterShortcutPanel.Windows;
using Umbra.Windows;

namespace Umbra.BetterWidget.Widgets.BetterShortcutPanel;

public sealed partial class ShortcutPanelPopup
{
    private void CreateContextMenu()
    {
        ContextMenu = new(
            [
                new("AddCustomItem") {
                    Label = I18N.Translate("Widget.DynamicMenu.ContextMenu.AddCustomItem"),
                    OnClick = () =>
                    {
                        CustomShortcutEntry entry = new() {
                            IconId = 14,
                            Label = "Custom Item",
                            Value = "Chat",
                            ActionType = "/dance",
                        };
                        
                        AssignShortcut(_selectedCategory, _selectedSlotIndex, entry);
                        SetButton(_selectedCategory, _selectedSlotIndex, entry);
                        
                        OpenCustomItemEditor();
                    },
                },
                ..Providers.GetAllProviders().Select(provider => new ContextMenuEntry(provider.ShortcutType) {
                    Label = provider.ContextMenuEntryName,
                    IconId = provider.ContextMenuEntryIcon,
                    OnClick = () => OpenPickerWindow(provider),
                }),
                new("-"),
                new("Configure") {
                    Label   = I18N.Translate("Widget.DynamicMenu.ContextMenu.Configure"),
                    OnClick = () => {
                        ShortcutEntry? data = GetShortcutData(_selectedCategory, _selectedSlotIndex);
        
                        if (data is not CustomShortcutEntry) return;
                        
                        OpenCustomItemEditor();
                    },
                },
                new("Copy") {
                    Label      = I18N.Translate("Widget.ShortcutPanel.ContextMenu.CopySlot"),
                    OnClick    = ContextActionCopySlot,
                    IsDisabled = true,
                },
                new("Paste") {
                    Label      = I18N.Translate("Widget.ShortcutPanel.ContextMenu.PasteSlot"),
                    OnClick    = ContextActionPasteSlot,
                    IsDisabled = true,
                },
                new("Clear") {
                    Label      = I18N.Translate("Widget.ShortcutPanel.ContextMenu.ClearSlot"),
                    OnClick    = ContextActionClearSlot,
                    IconId     = 61502u,
                    IsDisabled = true,
                },
            ]
        );
    }

    private void ContextActionClearSlot()
    {
        if (_selectedSlotNode == null) return;
        ClearSlot(_selectedSlotNode, _selectedCategory, _selectedSlotIndex);
    }

    private void ContextActionCopySlot()
    {
        if (_selectedSlotNode == null) return;

        ShortcutEntry? data = GetShortcutData(_selectedCategory, _selectedSlotIndex);
        if (data == null) return;
        
        ImGui.SetClipboardText($"SC:{data.Serialize()}");
    }
    
    private void ContextActionPasteSlot()
    {
        if (_selectedSlotNode == null) return;

        string clipboard = ImGui.GetClipboardText();
        if (string.IsNullOrEmpty(clipboard) || !clipboard.StartsWith("SC:")) return;

        var data = ShortcutEntry.Deserialize(clipboard[3..]);
        
        if (data == null) return;
        
        AssignShortcut(_selectedCategory, _selectedSlotIndex, data);
        SetButton(_selectedCategory, _selectedSlotIndex, data);
    }

    private void OpenPickerWindow(AbstractShortcutProvider provider)
    {
        if (_selectedSlotNode == null) return;

        Framework.Service<WindowManager>().Present("Picker", new ShortcutPickerWindow(provider),
            window => {
                var entry = window.PickedId;
                if (entry == null) return;
                AssignShortcut(_selectedCategory, _selectedSlotIndex, entry);
            });
    }
}
