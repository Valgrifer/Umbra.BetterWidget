using Umbra.Windows;
using Umbra.Windows.Library.VariableEditor;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup
{
    private void OpenCustomizeEditor()
    {
        if (_selectedItem == null)
            return;
        
        var entry = _selectedItem!;
        
        StringVariable labelVar = new("Label") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Label.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Label.Description"),
            Value       = entry.CustomName ?? "",
            MaxLength   = 100,
        };

        GameIconVariable iconVar = new("Icon") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Icon.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Icon.Description"),
            Value       = entry.CustomIcon ?? 0,
        };

        ColorVariable iconColorVar = new("IconColor") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.IconColor.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.IconColor.Description"),
            Value       = entry.CustomColor,
        };

        List<Variable> variables = [labelVar, iconVar, iconColorVar];

        VariablesEditorWindow window = new(I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Title"), variables, []);

        Framework
           .Service<WindowManager>()
           .Present(
                "CustomizeEditor",
                window,
                _ => {
                    entry.CustomName = labelVar.Value != "" ? labelVar.Value : null;
                    entry.CustomIcon = iconVar.Value != 0 ? iconVar.Value : null;
                    entry.CustomColor = iconColorVar.Value;

                    PersistFavorites();
                }
            );
    }
}