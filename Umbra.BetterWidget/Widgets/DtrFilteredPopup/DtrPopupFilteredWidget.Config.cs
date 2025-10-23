namespace Umbra.BetterWidget.Widgets.DtrFilteredPopup;

public partial class DtrPopupFilteredWidget
{
    private Dictionary<string, string> AllEntries { get; set; } = [];

    private const string EntrySeparator = "|$|";
    internal string[] SelectedEntries => GetConfigValue<string>("FilteredSelectedEntries").Split(EntrySeparator);
    
    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        AllEntries = new() { { "", "" } };

        foreach (var entry in _dtrBar.Entries.OrderBy(e => e.Title)) {
            AllEntries[entry.Title] = entry.Title;
        }

        SelectWidgetConfigVariable select = new SelectWidgetConfigVariable(
            "SelectedEntry",
            I18N.Translate("Widget.DtrSingle.Config.SelectedEntry.Name"),
            I18N.Translate("Widget.DtrSingle.Config.SelectedEntry.Description"),
            "",
            AllEntries
        );

        select.ValueChanged += s =>
        {
            if (s == "")
                return;
            
            select.SetValue("");
            var entries = SelectedEntries.ToList();
            entries.Add(s);
            SetConfigValue("FilteredSelectedEntries", string.Join(EntrySeparator, entries.Distinct().Where(e => !string.IsNullOrEmpty(e))));
        };
        
        return [
            ..base.GetConfigVariables(),
            new BooleanWidgetConfigVariable(
                "PlainText",
                I18N.Translate("Widget.DtrBar.Config.PlainText.Name"),
                I18N.Translate("Widget.DtrBar.Config.PlainText.Description"),
                false
            ),
            new SelectWidgetConfigVariable(
                "DecorateMode",
                I18N.Translate("Widget.DtrBar.Config.DecorateMode.Name"),
                I18N.Translate("Widget.DtrBar.Config.DecorateMode.Description"),
                "Always",
                new() {
                    { "Always", I18N.Translate("Widget.DtrBar.Config.DecorateMode.Option.Always") },
                    { "Never", I18N.Translate("Widget.DtrBar.Config.DecorateMode.Option.Never") },
                    { "Auto", I18N.Translate("Widget.DtrBar.Config.DecorateMode.Option.Auto") },
                }
            ),
            new IntegerWidgetConfigVariable(
                "ItemSpacing",
                I18N.Translate("Widget.DtrBar.Config.ItemSpacing.Name"),
                I18N.Translate("Widget.DtrBar.Config.ItemSpacing.Description"),
                6,
                0,
                64
            ),
            new IntegerWidgetConfigVariable(
                "MaxTextWidth",
                I18N.Translate("Widgets.DefaultToolbarWidget.Config.MaxTextWidth.Name"),
                I18N.Translate("Widgets.DefaultToolbarWidget.Config.MaxTextWidth.Description"),
                0,
                0,
                1000
            ),
            new IntegerWidgetConfigVariable(
                "TextSize",
                I18N.Translate("Widgets.DefaultToolbarWidget.Config.TextSize.Name"),
                I18N.Translate("Widgets.DefaultToolbarWidget.Config.TextSize.Description"),
                13,
                8,
                24
            ),
            new IntegerWidgetConfigVariable(
                "TextYOffset",
                I18N.Translate("Widget.DtrBar.Config.TextYOffset.Name"),
                I18N.Translate("Widget.DtrBar.Config.TextYOffset.Description"),
                0,
                -5,
                5
            ),
            select,
            new StringWidgetConfigVariable(
                "FilteredSelectedEntries",
                "Selected DTR entry",
                "",
                ""
                ),
            new BooleanWidgetConfigVariable(
                "HasBlacklists",
                "Has blacklists",
                "Take list into a blacklists instead of whitelists",
                false
            ),
        ];
    }
}
