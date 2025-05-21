using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MorfeyTools.Editor.Data
{
  public class SceneExplorerData : ScriptableObject
  {
    [Tooltip("Stores the GUIDs of favorite scenes in their display order.")]
    [SerializeField]
    private List<string> _favoriteSceneGuids = new();

    public List<string> FavoriteSceneGuids
    {
      get
      {
        _favoriteSceneGuids ??= new List<string>();
        return _favoriteSceneGuids;
      }
    }

    public void AddFavorite(string guid)
    {
      FavoriteSceneGuids.Remove(guid);
      FavoriteSceneGuids.Insert(0, guid);
      EditorUtility.SetDirty(this);
    }

    public void RemoveFavorite(string guid)
    {
      if (FavoriteSceneGuids.Remove(guid))
      {
        EditorUtility.SetDirty(this);
      }
    }

    public void SetFavoriteOrder(List<string> orderedGuids)
    {
      if (!_favoriteSceneGuids.SequenceEqual(orderedGuids))
      {
        _favoriteSceneGuids = new List<string>(orderedGuids);
        EditorUtility.SetDirty(this);
      }
    }

    public void ClearAllFavorites()
    {
      if (_favoriteSceneGuids.Count > 0)
      {
        _favoriteSceneGuids.Clear();
        EditorUtility.SetDirty(this);
      }
    }
  }
}