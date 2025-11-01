using Dalamud.Game.ClientState.Aetherytes;
using Lumina.Excel.Sheets;
using TerritoryType = Lumina.Excel.Sheets.TerritoryType;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal partial class TeleportWidgetPopup
{
    private IDataManager        DataManager        { get; } = Framework.Service<IDataManager>();
    private IAetheryteList      AetheryteList      { get; } = Framework.Service<IAetheryteList>();
    private static IMainMenuRepository MainMenuRepository { get; } = Framework.Service<IMainMenuRepository>();

    private readonly Dictionary<string, TeleportExpansion>   _expansions   = [];
    private readonly Dictionary<string, TeleportDestination> _destinations = [];
    private readonly Dictionary<string, List<TeleportWorld>> _worlds       = [];

    private List<uint>? EstateAetheryteIds { get; set; }
    private string?     _selectedExpansion;

    /// <summary>
    /// Hydrates the aetheryte points into the expansion/region/destination structure that can be used to build
    /// the graphical nodes in the popup menu.
    /// </summary>
    private void HydrateAetherytePoints()
    {
        _expansions.Clear();
        _destinations.Clear();
        _worlds.Clear();
        
        IZone currentZone   = Framework.Service<IZoneManager>().CurrentZone;
        var   territoryType = DataManager.GetExcelSheet<TerritoryType>().FindRow(currentZone.TerritoryId);

        if (territoryType == null) return;
        var currentExNodeId = $"Ex_{territoryType.Value.ExVersion.ValueNullable?.RowId}";

        foreach (var aetheryte in AetheryteList) {
            if (IsAetherytePlayerHousing(aetheryte)) continue;

            var gameData = aetheryte.AetheryteData.ValueNullable;
            if (gameData == null) continue;

            if (gameData.Value.Invisible || !gameData.Value.IsAetheryte) continue;

            var territory = gameData.Value.Territory.ValueNullable;
            if (territory == null) continue;

            var expansion = territory.Value.ExVersion.ValueNullable;
            if (expansion == null) continue;

            var   regionName    = territory.Value.Map.ValueNullable?.PlaceNameRegion.ValueNullable?.Name.ToString();
            var   mapName       = territory.Value.Map.ValueNullable?.PlaceName.ValueNullable?.Name.ToString();
            var   aetheryteName = gameData.Value.PlaceName.ValueNullable?.Name.ToString();
            uint? mapId         = territory.Value.Map.ValueNullable?.RowId;

            if (regionName == null || mapName == null || aetheryteName == null || mapId == null) continue;

            var regionNameRowId = territory.Value.Map.Value.PlaceNameRegion.RowId;
            var territoryRowId  = territory.Value.RowId;

            var expansionNodeId = $"Ex_{expansion.Value.RowId}";
            var regionNodeId    = $"Ex_{expansion.Value.RowId}_{regionNameRowId}";
            var aetheryteNodeId = $"Ex_{expansion.Value.RowId}_{territoryRowId}_{gameData.Value.RowId}";

            if (currentExNodeId == expansionNodeId) {
                _selectedExpansion = expansionNodeId;
            }

            _expansions.TryAdd(
                expansionNodeId,
                new() {
                    NodeId    = expansionNodeId,
                    Name      = expansion.Value.Name.ToString(),
                    SortIndex = (int)expansion.Value.RowId,
                    Regions   = [],
                }
            );

            var expansionNode = _expansions[expansionNodeId];

            expansionNode.Regions.TryAdd(
                regionNodeId,
                new() {
                    NodeId = regionNodeId,
                    Name   = regionName,
                    Maps   = [],
                }
            );

            var regionNode = expansionNode.Regions[regionNodeId];

            regionNode.Maps.TryAdd(
                $"Map_{mapId}",
                new() {
                    NodeId       = aetheryteNodeId,
                    Name         = mapName,
                    Destinations = [],
                }
            );

            var mapNode = regionNode.Maps[$"Map_{mapId}"];

            var partId = GetPartId(GetRegionFromRegionNamePlace(regionNameRowId), territoryRowId);

            TeleportDestination destination = new() {
                NodeId      = aetheryteNodeId,
                Name        = aetheryteName,
                AetheryteId = aetheryte.AetheryteId,
                SubIndex    = aetheryte.SubIndex,
                GilCost     = aetheryte.GilCost,
                SortIndex   = gameData.Value.Order,
                UldPartId   = partId,
                MapId       = (uint)mapId,
                TerritoryId = territory.Value.RowId,
            };

            mapNode.Destinations.TryAdd(aetheryteNodeId, destination);
            _destinations.TryAdd(GetDataFrom(destination).ToString(), destination);
        }

        if (LifeSteamEnable)
        {
            var homeDataCenterName = Framework.Service<IPlayer>().HomeDataCenterName;
            var exelWorldDc = DataManager.GetExcelSheet<WorldDCGroupType>();
            var exelWorlds = DataManager.GetExcelSheet<World>();
            byte? regionByte = null;

            foreach (var dcRow in exelWorldDc) {
                if (dcRow.Name == homeDataCenterName) {
                    regionByte = dcRow.Region;
                    break;
                }
            }
            
            List<uint> dataCenterIds = new List<uint>();
            
            foreach (var dcRow in exelWorldDc) {
                if (dcRow.Region == regionByte) {
                    dataCenterIds.Add(dcRow.RowId);
                }
            }

            if (dataCenterIds.Count > 0) {
                foreach (var world in exelWorlds) {
                    if (!dataCenterIds.Contains(world.DataCenter.RowId) || !world.IsPublic)
                        continue;
                    
                    var dcName = world.DataCenter.Value.Name.ExtractText();

                    if (!_worlds.TryGetValue(dcName, out List<TeleportWorld>? worlds)) {
                        worlds = [];
                        _worlds.TryAdd(dcName, worlds);
                    }

                    worlds.Add(new () {
                        DcName = dcName,
                        Name = world.Name.ToString(),
                    });
                }
            }
        }
    }

    private bool IsAetherytePlayerHousing(IAetheryteEntry entry)
    {
        EstateAetheryteIds ??= DataManager.GetExcelSheet<Aetheryte>()
            .Where(aetheryte => aetheryte.PlaceName.RowId is 1145 or 1160)
            .Select(aetheryte => aetheryte.RowId)
            .ToList();

        return entry.IsSharedHouse
            || entry.IsApartment
            || entry.Plot > 0
            || entry.Ward > 0
            || EstateAetheryteIds.Contains(entry.AetheryteId);
    }

    private TeleportDestinationData GetDataFrom(TeleportDestination destination)
    {
        return new (destination.AetheryteId, destination.SubIndex);
    }

    private TeleportMiscellaneousData GetDataFrom(MainMenuItem item)
    {
        return new (item);
    }

    private TeleportWorldData GetDataFrom(TeleportWorld world)
    {
        return new (world.DcName, world.Name);
    }

    // gotten from Client::UI::Agent::AgentTeleport_Show -> sub_140C04360 -> sub_140C043D0 -> sub_140C06860
    // sig: E8 ?? ?? ?? ?? 49 8D 4E F8 8B D8
    // was added as a function with the new expansion so possibly unstable
    private int GetPartId(uint region, uint territory)
    {
        return territory switch {
            819          => 8,
            820          => 9,
            958          => 11,
            1186 or 1191 => 14,
            _ => region switch {
                0  => 0,
                1  => 1,
                2  => 2,
                3  => 4,
                6  => 6,
                7  => 7,
                10 => 5,
                12 => 10,
                13 => 12,
                _  => region - 16 > 1 ? 3 : 13
            }
        };
    }

    // gotten from Client::UI::Agent::AgentTeleport_Show -> sub_140C04360 -> sub_140C043D0 -> sub_140C064F0
    // sig: 48 83 EC 28 0F B7 4A 08
    private uint GetRegionFromRegionNamePlace(uint placeNameRegion) =>
        placeNameRegion switch {
            22   => 0,
            23   => 1,
            24   => 2,
            25   => 3,
            497  => 4,
            498  => 5,
            26   => 8,
            2400 => 6,
            2402 => 7,
            2401 => 9,
            2950 => 11,
            3703 => 12,
            3702 => 13,
            3704 => 14,
            3705 => 15,
            4500 => 16,
            4501 => 17,
            4502 => 18,
            _    => 19
        };

    [Serializable]
    internal class TeleportData(string type)
    {
        public string T { get; } = type;
        
        
        public string? CustomName { get; set; }
        public uint? CustomIcon { get; set; }
        public uint CustomColor { get; set; } = 0xFFFFFFFF;
        

        public TeleportData() : this(null!)
        { }

        public override string ToString()
        {
            return T;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not TeleportData data) return false;
        
            return Equals(data);
        }
        
        protected bool Equals(TeleportData other)
        {
            return T == other.T;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(T);
        }
    }

    [Serializable]
    internal class TeleportDestinationData(uint aetheryteId, byte subIndex) : TeleportData("Dest")
    {
        public uint   AetheryteId { get; set; } = aetheryteId;
        public byte   SubIndex    { get; set; } = subIndex;
        
        public TeleportDestinationData() : this(0, 0)
        { }

        public override string ToString()
        {
            return base.ToString() + $"-{AetheryteId}-{SubIndex}";
        }

        public override bool Equals(object? obj)
        {
            if (!base.Equals(obj)) return false;
            if (obj is not TeleportDestinationData data) return false;
        
            return Equals(data);
        }
        
        protected bool Equals(TeleportDestinationData other)
        {
            return AetheryteId == other.AetheryteId && SubIndex == other.SubIndex;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(T, AetheryteId, SubIndex);
        }
    }

    [Serializable]
    internal class TeleportMiscellaneousData(string id) : TeleportData("Misc")
    {
        public string Id { get; set; } = id;

        [NonSerialized]
        private MainMenuItem? _item;

        public TeleportMiscellaneousData() : this("")
        { }

        public TeleportMiscellaneousData(MainMenuItem item) : this(item.Id)
        {
            _item  = item;
        }
        
        public MainMenuItem GetItem() {
            if (_item == null) {
                _item = MainMenuRepository.FindById(Id);
            }
            return _item!;
        }

        public override string ToString()
        {
            return base.ToString() + $"-{Id}";
        }

        public override bool Equals(object? obj)
        {
            if (!base.Equals(obj)) return false;
            if (obj is not TeleportMiscellaneousData data) return false;
        
            return Equals(data);
        }
        
        protected bool Equals(TeleportMiscellaneousData other)
        {
            return Id == other.Id;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(T, Id);
        }
    }

    [Serializable]
    internal class TeleportWorldData(string dcName, string name) : TeleportData("Wrld")
    {
        public string DcName      { get; set; } = dcName;
        public string Name { get; set; } = name;
        
        public TeleportWorldData() : this(null!, null!)
        { }

        public override string ToString()
        {
            return base.ToString() + $"-{DcName}-{Name}";
        }

        public override bool Equals(object? obj)
        {
            if (!base.Equals(obj)) return false;
            if (obj is not TeleportWorldData data) return false;
        
            return Equals(data);
        }
        
        protected bool Equals(TeleportWorldData other)
        {
            return DcName == other.DcName && Name == other.Name;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(T, DcName, Name);
        }
    }

    [Serializable]
    internal class TeleportLifeSteamData(string command) : TeleportData("Life")
    {
        public string Cmd { get; set; } = command;

        public TeleportLifeSteamData() : this(null!)
        { }

        public override string ToString()
        {
            return base.ToString() + $"-{Cmd.Replace(" ", "_")}";
        }

        public override bool Equals(object? obj)
        {
            if (!base.Equals(obj)) return false;
            if (obj is not TeleportLifeSteamData data) return false;
        
            return Equals(data);
        }
        
        protected bool Equals(TeleportLifeSteamData other)
        {
            return Cmd == other.Cmd;
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(T, Cmd);
        }
    }
}

internal struct TeleportExpansion
{
    public string                             NodeId    { get; set; }
    public string                             Name      { get; set; }
    public int                                SortIndex { get; set; }
    public Dictionary<string, TeleportRegion> Regions   { get; set; }
}

internal struct TeleportRegion
{
    public string NodeId { get; set; }
    public string Name   { get; set; }

    public Dictionary<string, TeleportMap> Maps { get; set; }
}

internal struct TeleportMap
{
    public string NodeId { get; set; }
    public string Name   { get; set; }

    public Dictionary<string, TeleportDestination> Destinations { get; set; }
}

internal struct TeleportDestination
{
    public string NodeId      { get; set; }
    public string Name        { get; set; }
    public uint   AetheryteId { get; set; }
    public byte   SubIndex    { get; set; }
    public uint   GilCost     { get; set; }
    public int    SortIndex   { get; set; }
    public int    UldPartId   { get; set; }
    public uint   MapId       { get; set; }
    public uint   TerritoryId { get; set; }
}

internal struct TeleportWorld
{
    public string DcName      { get; set; }
    public string Name        { get; set; }
}
