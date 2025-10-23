namespace Umbra.BetterWidget.Widgets.ProfileManager;

internal sealed partial class ProfileManagerPopup
{
    private int? _selectedItemIndex;

    private void CreateContextMenu()
    {
        ContextMenu = new(
            [
                new("EnableEditMode") {
                    Label = I18N.Translate("Widget.DynamicMenu.ContextMenu.EnableEditMode"),
                    OnClick = () => {
                        EmptyButtonPlaceholder.Style.IsVisible = true;
                        EditModeEnabled                        = true;
                        Framework.DalamudFramework.Run(RebuildMenu);
                        OnEditModeChanged?.Invoke(true);
                    },
                },
                new("DisableEditMode") {
                    Label = I18N.Translate("Widget.DynamicMenu.ContextMenu.DisableEditMode"),
                    OnClick = () => {
                        EmptyButtonPlaceholder.Style.IsVisible = false;
                        EditModeEnabled                        = false;
                        Framework.DalamudFramework.Run(RebuildMenu);
                        OnEditModeChanged?.Invoke(false);
                        if (Entries.Count == 0) Close();
                    },
                },
                new("Configure") {
                    Label   = I18N.Translate("Widget.DynamicMenu.ContextMenu.Configure"),
                    OnClick = () => {
                        if (_selectedItemIndex == null) return;

                        OpenEnttryEditor();
                    },
                },
                new("MoveUp") {
                    Label   = I18N.Translate("Widget.DynamicMenu.ContextMenu.MoveUp"),
                    OnClick = () => MoveItemUp(Entries[_selectedItemIndex!.Value]),
                },
                new("MoveDown") {
                    Label   = I18N.Translate("Widget.DynamicMenu.ContextMenu.MoveDown"),
                    OnClick = () => MoveItemDown(Entries[_selectedItemIndex!.Value]),
                }
            ]
        );
    }

    private void OpenContextMenu(int? itemIndex = null)
    {
        _selectedItemIndex = itemIndex;

        ContextMenu!.SetEntryVisible("DisableEditMode", EditModeEnabled);
        ContextMenu!.SetEntryVisible("EnableEditMode",  !EditModeEnabled);
        ContextMenu!.SetEntryVisible("Configure",       itemIndex != null);
        ContextMenu!.SetEntryVisible("MoveUp",          itemIndex != null);
        ContextMenu!.SetEntryVisible("MoveDown",        itemIndex != null);
        ContextMenu!.SetEntryVisible("Remove",          itemIndex != null);

        if (itemIndex != null) {
            ContextMenu!.SetEntryDisabled("MoveUp",    !CanMoveItemUp(Entries[itemIndex.Value]));
            ContextMenu!.SetEntryDisabled("MoveDown",  !CanMoveItemDown(Entries[itemIndex.Value]));
        }

        ContextMenu!.Present();
    }
}
