using Newtonsoft.Json;
using Umbra.Common.Utility;

namespace Umbra.BetterWidget.Widgets.BetterTeleport;

internal sealed partial class TeleportWidgetPopup
{
    private string             _favoritesData = "[]";
    private List<TeleportData> Favorites { get; } = [];

    /// <summary>
    /// Adds the given destination to the favorites list.
    /// </summary>
    private void AddFavorite(TeleportData destination)
    {
        if (Favorites.Contains(destination)) return;

        Favorites.Add(destination);
        PersistFavorites();

        BuildFavoritesButton(destination);
    }

    /// <summary>
    /// Removes the given destination from the favorites list.
    /// </summary>
    private void RemoveFavorite(TeleportData destination)
    {
        if (!Favorites.Remove(destination)) return;
        
        RemoveFavoritesButton(destination);
        PersistFavorites();
    }

    /// <summary>
    /// Returns true if the given destination is a favorite.
    /// </summary>
    private bool IsFavorite(TeleportData destination)
    {
        return Favorites.Contains(destination);
    }

    /// <summary>
    /// Loads the favorites from the config.
    /// </summary>
    private void DecodeLoadFavorites(string data)
    {
        if (string.IsNullOrEmpty(data)) return;

        _favoritesData = data;
    
        Favorites.Clear();

        var decompress = Compression.Decompress(_favoritesData);
        Favorites.AddRange(JsonConvert.DeserializeObject<List<TeleportData>>(decompress != "{}" ? decompress : "[]", TeleportConverter.DefaultSettings)?.ToList() ?? []);
    }

    /// <summary>
    /// Persist the favorites list to the user config.
    /// </summary>
    private void PersistFavorites()
    {
        string oldData = _favoritesData;
        _favoritesData = Compression.Compress(JsonConvert.SerializeObject(Favorites, TeleportConverter.DefaultSettings));
        if (oldData != _favoritesData)
            OnFavoritesChanged?.Invoke(_favoritesData);
    }
}
