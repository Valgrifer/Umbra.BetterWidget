using FFXIVClientStructs.FFXIV.Component.GUI;

namespace Umbra.BetterWidget.Widgets.DtrFilteredBar;

internal unsafe partial class DtrBarFilteredWidget
{
    private void UpdateNativeServerInfoBar()
    {
        if (! GetConfigValue<bool>("HideNative")) {
            SetNativeServerInfoBarVisibility(true);
            return;
        }

        SetNativeServerInfoBarVisibility(!(Utils.Enabled && Framework.Service<UmbraVisibility>().IsToolbarVisible()));
    }

    private void SetNativeServerInfoBarVisibility(bool isVisible)
    {
        var dtrBar = (AtkUnitBase*) _gameGui!.GetAddonByName("_DTR").Address;
        if (dtrBar != null && dtrBar->IsVisible != isVisible) {
            dtrBar->IsVisible = isVisible;
        }
    }
}
