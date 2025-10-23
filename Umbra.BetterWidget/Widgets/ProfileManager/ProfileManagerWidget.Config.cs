namespace Umbra.BetterWidget.Widgets.ProfileManager;

internal sealed partial class ProfileManagerWidget
{
    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            ..base.GetConfigVariables(),
                
            new BooleanWidgetConfigVariable(
                "EditModeEnabled",
                I18N.Translate("Widget.DynamicMenu.Config.EditModeEnabled.Name"),
                I18N.Translate("Widget.DynamicMenu.Config.EditModeEnabled.Description"),
                false
            ),
            new StringWidgetConfigVariable(
                "ButtonLabel",
                I18N.Translate("Widget.DynamicMenu.Config.ButtonLabel.Name"),
                I18N.Translate("Widget.DynamicMenu.Config.ButtonLabel.Description"),
                "Collections",
                1024,
                true
            ),
            new StringWidgetConfigVariable(
                "ButtonTooltip",
                I18N.Translate("Widget.DynamicMenu.Config.ButtonTooltip.Name"),
                I18N.Translate("Widget.DynamicMenu.Config.ButtonTooltip.Description"),
                "",
                1024,
                true
            ),
            new IntegerWidgetConfigVariable(
                "MenuEntryHeight",
                I18N.Translate("Widget.DynamicMenu.Config.MenuEntryHeight.Name"),
                I18N.Translate("Widget.DynamicMenu.Config.MenuEntryHeight.Description"),
                36,
                20,
                64
            ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") },
            new IntegerWidgetConfigVariable(
                "MenuFontSize",
                I18N.Translate("Widget.DynamicMenu.Config.MenuFontSize.Name"),
                I18N.Translate("Widget.DynamicMenu.Config.MenuFontSize.Description"),
                13,
                10,
                20
            ) { Category = I18N.Translate("Widget.ConfigCategory.MenuAppearance") },
            new StringWidgetConfigVariable("ExtraDataEntries", "", null, "", short.MaxValue) { IsHidden = true },
        ];
    }
}
