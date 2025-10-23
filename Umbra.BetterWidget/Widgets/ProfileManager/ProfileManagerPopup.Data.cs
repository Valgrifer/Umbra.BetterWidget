using Newtonsoft.Json;

namespace Umbra.BetterWidget.Widgets.ProfileManager;

internal sealed partial class ProfileManagerPopup
{
    private ProfileExtraData GetEntry(ProfileWrapper profile)
    {
        ProfileExtraData? extraData = Entries.FirstOrDefault(entry => profile.Guid == entry.ProfileId);

        if (extraData == null)
        {
            extraData =  new () { ProfileId = profile.Guid };
            Entries.Add(extraData);
            OnEntriesChanged?.Invoke();
        }

        return extraData;
    }

    private void CleanEntries(List<Guid> usedIds)
    {
        foreach (var entry in Entries.ToList())
        {
            if (usedIds.Contains(entry.ProfileId)) continue;
            
            Entries.Remove(entry);
        }
    }

    private void MoveItemUp(ProfileExtraData entry)
    {
        var index = Entries.IndexOf(entry);
        if (index == 0) return;

        Entries.RemoveAt(index);
        Entries.Insert(index - 1, entry);

        Framework.DalamudFramework.Run(RebuildMenu);
        OnEntriesChanged?.Invoke();
    }

    private void MoveItemDown(ProfileExtraData entry)
    {
        var index = Entries.IndexOf(entry);
        if (index == Entries.Count - 1) return;

        Entries.RemoveAt(index);
        Entries.Insert(index + 1, entry);

        Framework.DalamudFramework.Run(RebuildMenu);
        OnEntriesChanged?.Invoke();
    }

    private bool CanMoveItemUp(ProfileExtraData entry)
    {
        return Entries.IndexOf(entry) > 0;
    }

    private bool CanMoveItemDown(ProfileExtraData entry)
    {
        return Entries.IndexOf(entry) < Entries.Count - 1;
    }

    private ProfileWrapper? GetProfile(ProfileExtraData entry)
    {
        return GetProfiles().FirstOrDefault(profile => profile.Guid == entry.ProfileId);
    }

    // Contains short property names to reduce the size of the JSON data.
    [Serializable]
    public class ProfileExtraData
    {
        /// <summary>
        /// The profile Guid.
        /// </summary>
        [JsonProperty("Pi")] public Guid ProfileId { get; init; }

        /// <summary>
        /// Entry is Disable.
        /// </summary>
        [JsonProperty("Di")] public bool Disable { get; set; }

        /// <summary>
        /// The item label.
        /// </summary>
        [JsonProperty("Cl")] public string CustomLabel { get; set; } = "";

        /// <summary>
        /// The item sub-label.
        /// </summary>
        [JsonProperty("Sl")] public string SubLabel { get; set; } = "";

        /// <summary>
        /// The item icon ID.
        /// </summary>
        [JsonProperty("Ii")] public uint IconId { get; set; } = 0;
        /// <summary>
        /// The item icon color.
        /// </summary>
        [JsonProperty("Ic")] public uint IconColor { get; set; } = 0xFFFFFFFF;
    }
}
