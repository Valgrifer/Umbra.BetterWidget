using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup
{
    public bool LifeSteamEnable
    {
        get {
            List<IExposedPlugin> pluginList = [..Framework.DalamudPlugin.InstalledPlugins];
            
            return pluginList.Exists(plugin => plugin.InternalName == "Lifestream");
        }
    }
    
    private readonly ICallGateSubscriber<bool>? _getIsBusy =
        Framework.DalamudPlugin.GetIpcSubscriber<bool>("Lifestream.IsBusy");
    
    private readonly ICallGateSubscriber<bool>? _getAbort =
        Framework.DalamudPlugin.GetIpcSubscriber<bool>("Lifestream.Abort");
    
    private readonly ICallGateSubscriber<string, bool>? _getExecuteCommand =
        Framework.DalamudPlugin.GetIpcSubscriber<string, bool>("Lifestream.ExecuteCommand");
    
    private readonly ICallGateSubscriber<string, bool>? _getChangeWorld =
        Framework.DalamudPlugin.GetIpcSubscriber<string, bool>("Lifestream.ChangeWorld");

    /// <summary>
    /// Returns if LifeSteam is busy.
    /// </summary>
    private bool LifeSteamIsBussy()
    {
        try {
            return _getIsBusy?.InvokeFunc() ?? false;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Stop LifeSteam.
    /// </summary>
    private void LifeSteamAbort()
    {
        try {
            _getAbort?.InvokeAction();
        } catch {
            // ignored
        }
    }

    /// <summary>
    /// Execute Lifestream command
    /// </summary>
    private void LifeSteamExecuteCommand(string arguments)
    {
        try {
            _getExecuteCommand?.InvokeAction(arguments);
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// Returns change world with Lifestream.
    /// </summary>
    private bool LifeSteamChangeWorld(string world)
    {
        try {
            return _getChangeWorld?.InvokeFunc(world) ?? false;
        } catch {
            return false;
        }
    }
}
