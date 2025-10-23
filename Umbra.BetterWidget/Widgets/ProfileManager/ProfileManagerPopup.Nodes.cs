namespace Umbra.BetterWidget.Widgets.ProfileManager;

internal sealed partial class ProfileManagerPopup
{
    private Node CreateEntryNode(ProfileWrapper profile, ProfileExtraData entry)
    {
        Node textNode    = new() { ClassList = ["text"], InheritTags       = true };
        Node iconNode    = new() { ClassList = ["icon"], InheritTags       = true };
        Node altTextNode = new() { ClassList = ["text-alt"], InheritTags   = true };

        iconNode.Style.Size = new(EntryHeight - 5, EntryHeight - 5);

        if (entry.IconColor != 0) {
            iconNode.Style.ImageColor = new(entry.IconColor);
        }

        textNode.Style.FontSize    = EntryFontSize;
        altTextNode.Style.FontSize = AltEntryFontSize;
        altTextNode.NodeValue      = entry.SubLabel;

        Node node = new() {
            ClassList = ["item"],
            SortIndex = Entries.IndexOf(entry),
            Style     = new() { Size = new(0, EntryHeight), IsVisible = !entry.Disable || EditModeEnabled},
            ChildNodes = [
                new() {
                    ClassList  = ["icon-wrapper"],
                    ChildNodes = [iconNode],
                    Style = new() {
                        Size = new(EntryHeight, EntryHeight),
                    }
                },
                textNode,
                altTextNode,
            ],
        };

        iconNode.Style.IconId = entry.IconId;
        textNode.NodeValue    = !string.IsNullOrEmpty(entry.CustomLabel) ? entry.CustomLabel : profile.Name;
        textNode.Style.Color  = entry.Disable ? new Color(0, 200, 200) : profile.IsEnabled ? new Color(0, 200, 0) : new Color(0, 0, 200);


        node.OnMouseUp += _ => {
            InvokeProfileEntry(entry);
            Close();
        };

        node.OnRightClick += _ => OpenContextMenu(Entries.IndexOf(entry));

        return node;
    }

    private Node ItemList               => Node.FindById("ItemList")!;
    private Node EmptyButtonPlaceholder => Node.FindById("EmptyButtonPlaceholder")!;
}
