using Dalamud.Game.Text;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup
{
    private Node CondensedInterfaceNode => Node.QuerySelector("#condensed-ui")!;
    private Node CondensedSidePanelNode => CondensedInterfaceNode.QuerySelector(".side-panel")!;
    private Node CondensedContentsNode  => CondensedInterfaceNode.QuerySelector(".contents > .list")!;
    private Node FavoritesButton        => CondensedSidePanelNode.QuerySelector("#Favorites_Button")!;
    private Node FavoritesList          => CondensedContentsNode.QuerySelector("#Favorites_Content > .condensed-region > .list")!;

    private void BuildCondensedInterface()
    {
        CondensedInterfaceNode.Clear();
        CondensedInterfaceNode.Style.IsVisible = true;
        CondensedInterfaceNode.AppendChild(new() { ClassList = ["side-panel", Utils.IsTopAligned ? "top" : "bottom" ] });
        CondensedSidePanelNode.AppendChild(new() { ClassList = ["side-panel-spacer"], SortIndex = int.MinValue });
        
        Node contentsWrapper = new() {
            ClassList  = ["contents", "scrollbars"],
            ChildNodes = [new() { ClassList = ["list"] }],
        };

        contentsWrapper.Overflow   = false;
        contentsWrapper.Style.Size = new(FixedPopupWidth && CustomPopupWidth >= 250 ? CustomPopupWidth : 0, PopupHeight);

        CondensedInterfaceNode.AppendChild(contentsWrapper);

        foreach (var expansion in _expansions.Values) {
            BuildCondensedSidePanelExpansionButton(expansion);
            BuildCondensedExpansionContent(expansion);
        }

        CondensedSidePanelNode.AppendChild(new() { ClassList = ["side-panel-separator"], SortIndex = int.MaxValue - 10 });
        BuildMiscellaneousEntries();

        if (LifeSteamEnable) {
            BuildCondensedWorldEntries();
        }
        
        BuildCondensedFavoriteEntries();

        switch (DefaultOpenedGroupName) {
            case "Favorites" when Favorites.Count > 0:
                ActivateExpansion("Favorites");
                break;
            case "Other":
                ActivateExpansion("Other");
                break;
            default:
                ActivateExpansion(_selectedExpansion ?? _expansions.Keys.First());
                break;
        }
    }

    private void BuildCondensedSidePanelExpansionButton(TeleportExpansion expansion)
    {
        Node node = Document.CreateNodeFromTemplate("condensed-side-panel-button", new() {
            { "label", expansion.Name }
        });

        node.Id        = $"{expansion.NodeId}_Button";
        node.SortIndex = expansion.SortIndex;
        node.ToggleClass("selected", _selectedExpansion == expansion.NodeId);

        node.OnClick += _ => ActivateExpansion(expansion.NodeId);
        node.OnMouseEnter += _ => {
            if (OpenCategoryOnHover) {
                ActivateExpansion(expansion.NodeId);
            }
        };

        CondensedSidePanelNode.AppendChild(node);
    }

    private void BuildCondensedExpansionContent(TeleportExpansion expansion)
    {
        Node node = new() { ClassList = ["condensed-expansion"] };

        node.Style.IsVisible = false;
        node.Id              = $"{expansion.NodeId}_Content";
        node.SortIndex       = expansion.SortIndex;

        foreach (var region in expansion.Regions.Values) {
            Node regionNode = Document.CreateNodeFromTemplate("condensed-region", new() {
                { "label", region.Name }
            });
            node.AppendChild(regionNode);

            foreach (var map in region.Maps.Values) {
                foreach (var destination in map.Destinations.Values) {

                    Node destinationNode = Document.CreateNodeFromTemplate("condensed-teleport", new() {
                        { "label", GetDestinationName(map, destination) },
                        { "cost", $"{SeIconChar.Gil.ToIconChar()} {I18N.FormatNumber(destination.GilCost)}" }
                    });

                    regionNode.QuerySelector(".list")!.AppendChild(destinationNode);
                    destinationNode.QuerySelector(".icon")!.Style.UldPartId = destination.UldPartId;

                    destinationNode.OnClick      += _ => Teleport(GetDataFrom(destination));
                    destinationNode.OnRightClick += _ => OpenContextMenu(GetDataFrom(destination));
                }
            }
        }

        CondensedContentsNode.AppendChild(node);
    }

    private void BuildMiscellaneousEntries()
    {
        Node button = Document.CreateNodeFromTemplate("condensed-side-panel-button", new() { { "label", I18N.Translate("Widget.Teleport.Misc") } });

        button.Id        = "Other_Button";
        button.SortIndex = int.MaxValue - 2;
        button.ToggleClass("selected", _selectedExpansion == "Other");
        button.OnClick += _ => ActivateExpansion("Other");
        button.OnMouseEnter += _ => {
            if (OpenCategoryOnHover) {
                ActivateExpansion("Other");
            }
        };
        CondensedSidePanelNode.AppendChild(button);

        Node expansionNode = new() { ClassList = ["condensed-expansion"] };

        expansionNode.Style.IsVisible = false;
        expansionNode.Id              = "Other_Content";
        expansionNode.SortIndex       = int.MaxValue - 1;

        CondensedContentsNode.AppendChild(expansionNode);

        Node regionNode = Document.CreateNodeFromTemplate("condensed-region", new() { { "label", I18N.Translate("Widget.Teleport.Misc") } });
        expansionNode.AppendChild(regionNode);

        foreach (MainMenuItem item in MainMenuRepository.GetCategory(MenuCategory.Travel).Items) {
            switch (item.Type) {
                case MainMenuItemType.MainCommand when item.CommandId != 36:
                case MainMenuItemType.Separator:
                    continue;
                case MainMenuItemType.ChatCommand:
                case MainMenuItemType.Callback:
                default: {
                    var menuItemNode = Document.CreateNodeFromTemplate("condensed-teleport", new() {
                        { "label", item.Name },
                        { "cost", item.ShortKey }
                    });

                    menuItemNode.Id        = item.Id;
                    menuItemNode.SortIndex = item.SortIndex;

                    var iconNode = menuItemNode.QuerySelector(".icon")!;
                    var iconNodeStyle = iconNode.Style;
                    
                    switch (item.Icon) {
                        case uint iconId:
                            iconNodeStyle.IconId = iconId;
                            iconNode.NodeValue    = null;
                            break;
                        case SeIconChar seIconChar:
                            iconNodeStyle.IconId = null;
                            iconNodeStyle.Color  = item.IconColor != null ? new(item.IconColor.Value) : null;
                            iconNode.NodeValue   = seIconChar.ToIconString();
                            break;
                    }

                    menuItemNode.SortIndex =  item.SortIndex;
                    menuItemNode.OnClick      += _ => Teleport(GetDataFrom(item));
                    menuItemNode.OnRightClick += _ => OpenContextMenu(GetDataFrom(item));

                    regionNode.QuerySelector(".list")!.AppendChild(menuItemNode);
                    break;
                }
            }
        }
    }

    private void BuildCondensedFavoriteEntries()
    {
        Node button = Document.CreateNodeFromTemplate("condensed-side-panel-button", new() { { "label", I18N.Translate("Widget.Teleport.Favorites") } });

        button.Id        = "Favorites_Button";
        button.SortIndex = int.MaxValue - 2;
        button.ToggleClass("selected", _selectedExpansion == "Favorites");
        button.OnClick += _ => ActivateExpansion("Favorites");
        button.OnMouseEnter += _ => {
            if (OpenCategoryOnHover) {
                ActivateExpansion("Favorites");
            }
        };

        CondensedSidePanelNode.AppendChild(button);

        button.Style.IsVisible = Favorites.Count > 0;

        Node expansionNode = new() { ClassList = ["condensed-expansion"] };

        expansionNode.Style.IsVisible = false;
        expansionNode.Id              = "Favorites_Content";
        expansionNode.SortIndex       = int.MaxValue - 1;

        CondensedContentsNode.AppendChild(expansionNode);

        Node regionNode = Document.CreateNodeFromTemplate("condensed-region", new() { { "label", I18N.Translate("Widget.Teleport.Favorites") } });
        expansionNode.AppendChild(regionNode);

        foreach (var favorite in Favorites) {
            CondensedBuildFavoritesButton(favorite);
        }
    }

    private void BuildCondensedWorldEntries()
    {
        Node button = Document.CreateNodeFromTemplate("condensed-side-panel-button", new() { { "label", "Worlds" } });

        button.Id        = "Worlds_Button";
        button.SortIndex = int.MaxValue - 2;
        button.ToggleClass("selected", _selectedExpansion == "Worlds");
        button.OnClick += _ => ActivateExpansion("Worlds");
        button.OnMouseEnter += _ => {
            if (OpenCategoryOnHover) {
                ActivateExpansion("Worlds");
            }
        };

        CondensedSidePanelNode.AppendChild(button);

        Node expansionNode = new() { ClassList = ["condensed-expansion"] };

        expansionNode.Style.IsVisible = false;
        expansionNode.Id              = "Worlds_Content";
        expansionNode.SortIndex       = int.MaxValue - 1;

        CondensedContentsNode.AppendChild(expansionNode);

        int index = 0;
        foreach (var dcWorld in _worlds) {
            if (dcWorld.Value.Count == 0)
                continue;
            
            int groupId = index / 2 + 1;
            index++;

            Node? group = expansionNode.QuerySelector($"#Worlds_Content_Group_{groupId}");

            if (group == null) {
                group = new() { Id = $"Worlds_Content_Group_{groupId}", SortIndex = int.MaxValue - 1 - groupId};

                group.Style.AutoSize  = (AutoSize.Grow, AutoSize.Fit);
                group.Style.Flow      = Flow.Horizontal;
            
                expansionNode.AppendChild(group);
            }
            
            Node regionNode = Document.CreateNodeFromTemplate("condensed-region", new() {
                { "label", dcWorld.Key }
            });
            group.AppendChild(regionNode);
            
            foreach (var world in dcWorld.Value.OrderBy(w => w.Name)) {
                var menuItemNode = Document.CreateNodeFromTemplate("condensed-teleport", new() {
                    { "label", world.Name },
                    { "cost", "" }
                });

                menuItemNode.Id        = world.Name;

                menuItemNode.OnClick   += _ => {
                    LifeSteamChangeWorld(world.Name);
                    Close();
                };
                menuItemNode.OnClick      += _ => Teleport(GetDataFrom(world));
                menuItemNode.OnRightClick += _ => OpenContextMenu(GetDataFrom(world));

                regionNode.QuerySelector(".list")!.AppendChild(menuItemNode);
            }
        }
    }

    private void CondensedBuildFavoritesButton(TeleportData data)
    {
        if (!LifeSteamEnable)
            if (data is TeleportWorldData or TeleportLifeSteamData)
                return;
        
        FavoritesButton.Style.IsVisible = true;

        string name = "Error";
        string cost = "";

        TeleportDestination? destination = null;
        MainMenuItem?        item        = null;

        
        if (_destinations.TryGetValue(data.ToString(), out TeleportDestination dest))
            destination = dest;

        if (data is TeleportMiscellaneousData miscellaneousData)
            item = miscellaneousData.GetItem();
        

        if (data.CustomName == null)
        {
            if (destination.HasValue)
                name = destination.Value.Name;

            if (item != null)
                name = item.Name;

            if (data is TeleportWorldData worldData)
                name = $"{worldData.DcName} - {worldData.Name}";
        }
        else name = data.CustomName;
        
        
        if (destination.HasValue)
            cost = $"{SeIconChar.Gil.ToIconChar()} {I18N.FormatNumber(destination.Value.GilCost)}";

        if (item != null)
            cost = item.ShortKey;
        

        Node node = Document.CreateNodeFromTemplate("condensed-teleport", new() {
            { "label", name },
            { "cost", cost }
        });

        node.Id        = $"SortableCondensed_{data}";
        node.SortIndex = Favorites.IndexOf(data);

        var iconNode = node.QuerySelector(".icon");
        if (iconNode != null) {
            var iconNodeStyle = iconNode.Style;
            
            if (destination.HasValue)
                iconNode.Style.UldPartId = destination.Value.UldPartId;
            
            if (item != null) {       
                switch (item.Icon) {
                    case uint iconId:
                        iconNodeStyle.IconId = iconId;
                        iconNode.NodeValue    = null;
                        break;
                    case SeIconChar seIconChar:
                        iconNodeStyle.IconId = null;
                        iconNodeStyle.Color  = item.IconColor != null ? new(item.IconColor.Value) : null;
                        iconNode.NodeValue   = seIconChar.ToIconString();
                        break;
                }
            }

            if (data.CustomIcon != null)
                iconNodeStyle.IconId = data.CustomIcon;
            
            iconNodeStyle.ImageColor = new (data.CustomColor);
            
        } else {
            Logger.Warning($"CondensedBuildFavoritesButton: Icon not found in {node.Id}");
        }
        
        node.OnClick      += _ => Teleport(data);
        node.OnRightClick += _ => OpenContextMenu(data, true);

        FavoritesList.AppendChild(node);
    }

    private void CondensedRemoveFavoritesButton(TeleportData destination)
    {
        FavoritesList.QuerySelector($"#SortableCondensed_{destination}")?.Dispose();

        if (Favorites.Count == 0) {
            FavoritesButton.Style.IsVisible = false;
        }

        if (_selectedExpansion == "Favorites") {
            ActivateExpansion("Other");
        }
    }

    private void CondensedSortFavorites()
    {
        foreach (var favoriteId in Favorites) {
            var node = FavoritesList.QuerySelector($"#SortableCondensed_{favoriteId}");
            if (node != null) {
                node.SortIndex = Favorites.IndexOf(favoriteId);
            }
        }
    }

    private void ActivateExpansion(string id)
    {
        _selectedExpansion = id;

        foreach (var node in CondensedContentsNode.QuerySelectorAll(".condensed-expansion")) {
            node.Style.IsVisible = node.Id == $"{id}_Content";
        }

        foreach (var node in CondensedSidePanelNode.QuerySelectorAll(".side-panel-button")) {
            node.ToggleClass("selected", node.Id == $"{id}_Button");
        }
    }

    private string GetDestinationName(TeleportMap region, TeleportDestination destination)
    {
        return ShowMapNames && destination.Name != region.Name
            ? $"{region.Name} - {destination.Name}"
            : destination.Name;
    }
}
