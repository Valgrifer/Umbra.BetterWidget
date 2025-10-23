using Umbra.Windows;
using Umbra.Windows.Library.VariableEditor;

namespace Umbra.BetterWidget.Widgets.ProfileManager;

internal sealed partial class ProfileManagerPopup
{
    private void OpenEnttryEditor()
    {
        if (_selectedItemIndex == null) return;
        ProfileExtraData entry = Entries[_selectedItemIndex.Value];

        BooleanVariable disableVar = new("Disable") {
            Name        = "Is Disabled",
            Description = "Is collection is showed or not.",
            Value       = entry.Disable,
        };
        
        StringVariable labelVar = new("Label") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Label.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Label.Description"),
            Value       = entry.CustomLabel,
            MaxLength   = 100,
        };

        StringVariable altLabelVar = new("SubLabel") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.AltLabel.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.AltLabel.Description"),
            Value       = entry.SubLabel,
            MaxLength   = 100,
        };

        GameIconVariable iconVar = new("Icon") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Icon.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Icon.Description"),
            Value       = entry.IconId,
        };

        ColorVariable iconColorVar = new("IconColor") {
            Name        = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.IconColor.Name"),
            Description = I18N.Translate("Widget.DynamicMenu.CustomItemEditor.IconColor.Description"),
            Value       = entry.IconColor,
        };

        List<Variable> variables = [disableVar, labelVar, altLabelVar, iconVar, iconColorVar];

        VariablesEditorWindow window = new(I18N.Translate("Widget.DynamicMenu.CustomItemEditor.Title"), variables, []);

        Framework
           .Service<WindowManager>()
           .Present(
                "CustomItemEditor",
                window,
                _ => {
                    entry.Disable = disableVar.Value;
                    entry.CustomLabel = labelVar.Value;
                    entry.SubLabel = altLabelVar.Value;
                    entry.IconId = iconVar.Value;
                    entry.IconColor = iconColorVar.Value;

                    RebuildMenu();
                    OnEntriesChanged?.Invoke();
                }
            );
    }
}
