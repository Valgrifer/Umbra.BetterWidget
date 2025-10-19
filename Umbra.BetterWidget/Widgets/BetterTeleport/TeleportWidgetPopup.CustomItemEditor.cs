using Umbra.Windows;
using Umbra.Windows.Library.VariableEditor;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup
{
    private void OpenLifeStreamEditorEditor()
    {
        if (_selectedItem is not TeleportLifeSteamData entry)
            return;

        StringVariable labelVar = new("Label") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Label.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Label.Description"),
            Value       = entry.CustomName ?? "",
            MaxLength   = 100,
        };

        GameIconVariable iconVar = new("Icon") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Icon.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Icon.Description"),
            Value       = entry.CustomIcon ?? 111,
        };

        ColorVariable iconColorVar = new("IconColor") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.IconColor.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.IconColor.Description"),
            Value       = entry.CustomColor,
        };

        StringVariable commandVar = new("Command") {
            Name        = "LifeStream Command",
            Value       = entry.Cmd,
            MaxLength   = 128,
        };

        List<Variable> variables = [labelVar, iconVar, iconColorVar, commandVar];

        VariablesEditorWindow window = new(I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Title"), variables, []);

        Framework
           .Service<WindowManager>()
           .Present(
                "LifeStreamEditorEditor",
                window,
                _ => {
                    entry.CustomName = labelVar.Value;
                    entry.CustomIcon = iconVar.Value;
                    entry.CustomColor = iconColorVar.Value;
                    entry.Cmd = commandVar.Value;
                    
                    AddFavorite(entry);
                }
            );
    }
}