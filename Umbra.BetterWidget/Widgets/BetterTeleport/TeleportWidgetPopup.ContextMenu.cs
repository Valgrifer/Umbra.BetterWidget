using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup
{
    private TeleportData? _selectedItem;
    
    private void BuildContextMenu()
    {
        ContextMenu = new(
            [
                new("Teleport") {
                    Label   = I18N.Translate("Widget.Teleport.Name"),
                    IconId  = 111,
                    OnClick = ContextMenuTeleport
                },
                new("ShowMap") {
                    Label   = I18N.Translate("Widget.Teleport.OpenMap"),
                    OnClick = ContextMenuShowOnMap
                },
                new("AddFav") {
                    Label   = I18N.Translate("Widget.Teleport.Favorites.Add"),
                    OnClick = ContextMenuAddToFavorites
                },
                new("DelFav") {
                    Label   = I18N.Translate("Widget.Teleport.Favorites.Remove"),
                    OnClick = ContextMenuRemoveFromFavorites
                },
                new("Configure") {
                    Label   = I18N.Translate("Widget.DynamicMenu.ContextMenu.Configure"),
                    OnClick = ContextMenuLifeCustomizeToFavorite
                },
                new("LifeStreamCommande") {
                    Label   = "Add LifeStream Commande",
                    OnClick = ContextMenuLifeStreamCommandeToFavorites
                },
                new("MoveUp") {
                    Label   = I18N.Translate("Widget.Teleport.Favorites.MoveUp"),
                    OnClick = () => ContextMenuMoveFavorite(-1),
                },
                new("MoveDown") {
                    Label   = I18N.Translate("Widget.Teleport.Favorites.MoveDown"),
                    OnClick = () => ContextMenuMoveFavorite(1),
                }
            ]
        );
    }

    private void OpenContextMenu(TeleportData data, bool showSortables = false)
    {
        _selectedItem = data;

        bool isFav = IsFavorite(data);
        
        ContextMenu!.SetEntryVisible("ShowMap", data is TeleportDestinationData);
        ContextMenu!.SetEntryVisible("AddFav", !isFav);
        ContextMenu!.SetEntryVisible("DelFav", isFav);
        ContextMenu!.SetEntryVisible("Configure", isFav);
        ContextMenu!.SetEntryVisible("LifeStreamCommande", LifeSteamEnable && isFav);
        ContextMenu!.SetEntryVisible("MoveUp", showSortables);
        ContextMenu!.SetEntryVisible("MoveDown", showSortables);

        if (showSortables && IsFavorite(data)) {
            var indexAt = Favorites.IndexOf(data);
            ContextMenu!.SetEntryDisabled("MoveUp", indexAt == 0);
            ContextMenu!.SetEntryDisabled("MoveDown", indexAt == Favorites.Count - 1);
        }
        
        ContextMenu!.Present();
    }
    
    private void ContextMenuTeleport()
    {
        if (null == _selectedItem) return;
        Teleport(_selectedItem);
    }

    private unsafe void ContextMenuShowOnMap()
    {
        if (_selectedItem is not TeleportDestinationData destData)
            return;
        
        if(!_destinations.TryGetValue(destData.ToString(), out TeleportDestination destination))
            return;

        AgentMap* am = AgentMap.Instance();
        am->ShowMap(true, false);

        OpenMapInfo info = new() {
            Type        = MapType.Teleport,
            MapId       = destination.MapId,
            TerritoryId = destination.TerritoryId,
        };

        am->OpenMap(&info);
    }

    private void ContextMenuAddToFavorites()
    {
        if (null == _selectedItem) return;
        AddFavorite(_selectedItem);
        UpdateGlobalStyle();
    }

    private void ContextMenuRemoveFromFavorites()
    {
        if (null == _selectedItem) return;
        RemoveFavorite(_selectedItem);
        UpdateGlobalStyle();
    }
    
    private void ContextMenuLifeCustomizeToFavorite()
    {
        if (null == _selectedItem) return;
        
        if (_selectedItem is TeleportLifeSteamData) {
            OpenLifeStreamEditorEditor();
            return;
        }
        
        OpenCustomizeEditor();
    }
    
    private void ContextMenuLifeStreamCommandeToFavorites()
    {
        _selectedItem = new TeleportLifeSteamData("");
        _selectedItem.CustomName = "LifeStream Commande";
        _selectedItem.CustomIcon = 111;
        
        OpenLifeStreamEditorEditor();
    }

    private void ContextMenuMoveFavorite(int direction)
    {
        if (null == _selectedItem) return;

        var index = Favorites.IndexOf(_selectedItem);
        if (index == -1) return;

        var newIndex = index + direction;
        if (newIndex < 0 || newIndex >= Favorites.Count) return;

        Favorites.RemoveAt(index);
        Favorites.Insert(newIndex, _selectedItem);
        
        PersistFavorites();
        UpdateFavoriteSortIndices();
    }

    private void Teleport(TeleportData data)
    {
        if (!Framework.Service<IPlayer>().CanUseTeleportAction) return;

        if (ShowNotification) {
            string name = "Error";

            TeleportDestination? destination = null;

            if (_destinations.TryGetValue(data.ToString(), out TeleportDestination dest)) {
                destination = dest;
            }
        
            if (destination.HasValue) {
                name = destination.Value.Name;
            }

            if (data is TeleportMiscellaneousData miscellaneousData) {
                var item = miscellaneousData.GetItem();
                name = item.Name;
            }

            if (data is TeleportWorldData worldData)
                name = $"{worldData.DcName} - {worldData.Name}";

            if (data.CustomName != null)
                name = data.CustomName;
            
            Framework
                .Service<IToastGui>()
                .ShowQuest(
                    $"{I18N.Translate("Widget.Teleport.Name")}: {name}",
                    new() { IconId = 111, PlaySound = true, DisplayCheckmark = false }
                );
        }
        
        switch (data)
        {
            case TeleportDestinationData destData:
                unsafe {
                    Telepo.Instance()->Teleport(destData.AetheryteId, destData.SubIndex);
                }
                break;
            case TeleportMiscellaneousData miscellaneousData:
                miscellaneousData.GetItem().Invoke();
                break;
            case TeleportWorldData worldData:
                if (LifeSteamIsBussy())
                    LifeSteamAbort();
                LifeSteamChangeWorld(worldData.Name);
                break;
            case TeleportLifeSteamData lifeSteamData:
                if (LifeSteamIsBussy())
                    LifeSteamAbort();
                LifeSteamExecuteCommand(lifeSteamData.Cmd);
                break;
        }

        Close();
    }
}
