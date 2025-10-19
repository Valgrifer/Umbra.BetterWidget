namespace Umbra.BetterWidget.Widgets.BetterShortcutPanel;

internal sealed partial class ShortcutPanelWidget
{
    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        List<StringWidgetConfigVariable> categoryNameFields = [];

        for (var i = 0; i < 4; i++) {
            categoryNameFields.Add(
                new(
                    $"CategoryName_{i}",
                    I18N.Translate("Widget.ShortcutPanel.Config.CategoryName.Name",        i + 1),
                    I18N.Translate("Widget.ShortcutPanel.Config.CategoryName.Description", i + 1),
                    i == 0 ? Info.Name : "",
                    1024,
                    true
                ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") }
            );
        }

        return [
            ..base.GetConfigVariables(),
            
            new StringWidgetConfigVariable(
                "ButtonLabel",
                I18N.Translate("Widget.ShortcutPanel.Config.ButtonLabel.Name"),
                I18N.Translate("Widget.ShortcutPanel.Config.ButtonLabel.Description"),
                Info.Name,
                1024,
                true
            ),
            
            new SelectWidgetConfigVariable(
                "AltMode",
                I18N.Translate("Widget.CustomButton.Config.AltMode.Name"),
                I18N.Translate("Widget.CustomButton.Config.AltMode.Description"),
                "None",
                new() {
                    { "None", I18N.Translate("Widget.CustomButton.Config.Mode.Option.None") },
                    { "Command", I18N.Translate("Widget.CustomButton.Config.Mode.Option.Command") },
                    { "URL", I18N.Translate("Widget.CustomButton.Config.Mode.Option.URL") }
                }
            ),
            new StringWidgetConfigVariable(
                "AltCommand",
                I18N.Translate("Widget.CustomButton.Config.AltCommand.Name"),
                I18N.Translate("Widget.CustomButton.Config.AltCommand.Description"),
                ""
            ) { DisplayIf = () => GetConfigValue<string>("AltMode") != "None" },
            
            ..categoryNameFields,
            new IntegerWidgetConfigVariable(
                "NumCols",
                I18N.Translate("Widget.ShortcutPanel.Config.NumCols.Name"),
                I18N.Translate("Widget.ShortcutPanel.Config.NumCols.Description"),
                8,
                1,
                16
            ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") },
            new IntegerWidgetConfigVariable(
                "NumRows",
                I18N.Translate("Widget.ShortcutPanel.Config.NumRows.Name"),
                I18N.Translate("Widget.ShortcutPanel.Config.NumRows.Description"),
                4,
                1,
                16
            ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") },
            new BooleanWidgetConfigVariable(
                "ShowEmptySlots",
                I18N.Translate("Widget.ShortcutPanel.Config.ShowEmptySlots.Name"),
                I18N.Translate("Widget.ShortcutPanel.Config.ShowEmptySlots.Description"),
                true
            ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") },
            new BooleanWidgetConfigVariable(
                "AutoCloseOnUse",
                I18N.Translate("Widget.ShortcutPanel.Config.AutoCloseOnUse.Name"),
                I18N.Translate("Widget.ShortcutPanel.Config.AutoCloseOnUse.Description"),
                true
            ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") },
            new StringWidgetConfigVariable("SlotConfig", "", null, "", 0) { IsHidden = true },
        ];
    }
}
