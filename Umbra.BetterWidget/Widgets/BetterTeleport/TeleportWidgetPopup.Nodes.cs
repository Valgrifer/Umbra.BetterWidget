using Umbra.Widgets.Popup;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup
{
    private void UpdateInterface()
    {
        CondensedInterfaceNode.Style.FlowOrder = ReverseCondensedElements ? FlowOrder.Reverse : FlowOrder.Normal;
        
        foreach (var node in CondensedInterfaceNode.QuerySelectorAll(".list .cost")) {
            node.Style.IsVisible = ShowTeleportCost;
        }
    }

    private void BuildInterfaces()
    {
        CondensedInterfaceNode.Clear();

        BuildCondensedInterface();

        UpdateGlobalStyle();
    }

    private void UpdateGlobalStyle() {
        foreach (var node in Node.QuerySelectorAll(".text, .cost")) {
            node.Style.FontSize = PopupFontSize;
        }
        foreach (var node in Node.QuerySelectorAll(".icon")) {
            node.Style.Size = new ((int) Math.Round(PopupFontSize * 1.5));
        }
    }

    private void BuildFavoritesButton(TeleportData destination)
    {
        CondensedBuildFavoritesButton(destination);
    }

    private void RemoveFavoritesButton(TeleportData destination)
    {
        CondensedRemoveFavoritesButton(destination);
    }

    private void UpdateFavoriteSortIndices()
    {
        CondensedSortFavorites();
    }
}
