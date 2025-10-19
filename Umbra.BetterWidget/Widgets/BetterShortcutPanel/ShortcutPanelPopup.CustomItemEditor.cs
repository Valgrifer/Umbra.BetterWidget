using Umbra.Windows;
using Umbra.Windows.Library.VariableEditor;

namespace Umbra.BetterWidget.Widgets.BetterShortcutPanel;

public sealed partial class ShortcutPanelPopup
{
    private void OpenCustomItemEditor()
    {
        ShortcutEntry? data = GetShortcutData(_selectedCategory, _selectedSlotIndex);
        
        if (data is not CustomShortcutEntry entry) return;

        StringVariable labelVar = new("Label") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Label.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Label.Description"),
            Value       = entry.Cl,
            MaxLength   = 100,
        };

        GameIconVariable iconVar = new("Icon") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Icon.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Icon.Description"),
            Value       = entry.Ci,
        };

        ColorVariable iconColorVar = new("IconColor") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.IconColor.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.IconColor.Description"),
            Value       = entry.Cj,
        };

        StringSelectVariable typeVar = new("Type") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Type.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Type.Description"),
            Value       = entry.Ct,
            Choices = new() {
                { "Chat", I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Type.Option.ChatCommand") },
                { "URL", I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Type.Option.WebLink") },
            },
        };

        StringVariable commandVar = new("Command") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Command.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Command.Description"),
            Value       = entry.Cc,
            MaxLength   = 4096,
        };

        List<Variable> variables = [labelVar, iconVar, iconColorVar, typeVar, commandVar];

        VariablesEditorWindow window = new(I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Title"), variables, []);

        Framework
           .Service<WindowManager>()
           .Present(
                "CustomShortcutEditor",
                window,
                _ => {
                    entry.Cl = labelVar.Value;
                    entry.Ci = iconVar.Value;
                    entry.Cj = iconColorVar.Value;
                    entry.Ct = typeVar.Value;
                    entry.Cc = commandVar.Value;

                    AssignShortcut(_selectedCategory, _selectedSlotIndex, entry);
                    SetButton(_selectedCategory, _selectedSlotIndex, entry);
                }
            );
    }
}