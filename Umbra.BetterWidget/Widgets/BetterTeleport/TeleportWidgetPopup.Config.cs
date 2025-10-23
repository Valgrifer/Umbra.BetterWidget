namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup
{
    private bool             ShowNotification       { get; set; } = true;
    private string           DefaultOpenedGroupName { get; set; } = "Auto";
    private bool             OpenCategoryOnHover    { get; set; } = false;
    private bool             FixedPopupWidth        { get; set; } = true;
    private int              CustomPopupWidth       { get; set; } = 400;
    private int              PopupHeight            { get; set; } = 400;
    private int              PopupFontSize          { get; set; } = 14;
    private bool             ShowMapNames           { get; set; } = true;
    private bool             ShowTeleportCost       { get; set; } = true;

    protected override void UpdateConfigVariables(ToolbarWidget widget)
    {
        ShowNotification       = widget.GetConfigValue<bool>("ShowNotification");
        DefaultOpenedGroupName = widget.GetConfigValue<string>("DefaultOpenedGroupName");
        OpenCategoryOnHover    = widget.GetConfigValue<bool>("OpenCategoryOnHover");
        FixedPopupWidth        = widget.GetConfigValue<bool>("FixedPopupWidth");
        CustomPopupWidth       = widget.GetConfigValue<int>("CustomPopupWidth");
        PopupHeight            = widget.GetConfigValue<int>("PopupHeight");
        PopupFontSize          = widget.GetConfigValue<int>("PopupFontSize");
        ShowMapNames           = widget.GetConfigValue<bool>("ShowMapNames");
        ShowTeleportCost       = widget.GetConfigValue<bool>("ShowTeleportCost");
    }
}
