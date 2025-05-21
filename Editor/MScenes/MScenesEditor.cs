#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MorfeyTools.Editor.Data;
using MorfeyTools.Editor.DataManagement;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MorfeyTools.Editor.MScenes
{
  [Serializable]
  public partial class MScenesEditor : IDisposable
  {
    private Dictionary<string, SceneEntry> _allScenesCache = new();
    [NonSerialized] private bool _isRefreshing;
    private readonly MEditor _parentEditorWindow;
    private SceneExplorerData _data;

    public MScenesEditor(MEditor parentEditor)
    {
      _parentEditorWindow = parentEditor;
      LoadOrCreateDataAsset();
      InitializeViewModelLists();
      RefreshSceneListInternal();
      EditorApplication.projectChanged += OnProjectChanged;
    }

    [Title("Favorite Scenes")]
    [ListDrawerSettings(DraggableItems = true, ShowFoldout = false, HideAddButton = true, HideRemoveButton = true, ShowItemCount = false, DefaultExpandedState = true)]
    [ShowIf("@_favoriteScenes.Count > 0")]
    [SerializeField]
    [OnCollectionChanged(null, nameof(HandleFavoriteScenesListChangedAfter))]
    private List<SceneEntry> _favoriteScenes = new();

    [ListDrawerSettings(
      IsReadOnly = true, ShowFoldout = false, HideAddButton = true, HideRemoveButton = true,
      ShowItemCount = false, DefaultExpandedState = true,
      OnTitleBarGUI = nameof(DrawOtherScenesTitleBarGUI))]
    [Searchable]
    [SerializeField]
    private List<SceneEntry> _otherScenes = new();

    private static string GetPluginRootPath()
    {
      string[] guids = AssetDatabase.FindAssets("t:Script MScenesEditor");
      if (guids.Length == 0)
      {
        Debug.LogError("[MScenesEditor] Cannot find MScenesEditor script to determine plugin path. Defaulting to 'Assets/MorfeyTools/Editor'.");
        return "Assets/MorfeyTools/Editor";
      }

      string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
      string pluginRoot = Path.GetDirectoryName(Path.GetDirectoryName(scriptPath));
      return pluginRoot;
    }

    private void LoadOrCreateDataAsset()
    {
      _data = PackageDataManager.LoadOrCreateDataAsset<SceneExplorerData>();
    }

    private void InitializeViewModelLists()
    {
      _favoriteScenes ??= new List<SceneEntry>();
      _otherScenes ??= new List<SceneEntry>();
      _allScenesCache ??= new Dictionary<string, SceneEntry>();
    }

    [OnInspectorDispose]
    public void Dispose()
    {
      EditorApplication.projectChanged -= OnProjectChanged;
    }

    private void OnProjectChanged()
    {
      RefreshSceneListInternal();
    }

    private void DrawOtherScenesTitleBarGUI()
    {
      GUILayout.FlexibleSpace();

      if (SirenixEditorGUI.IconButton(EditorIcons.Refresh, 24, (int)EditorGUIUtility.singleLineHeight + 2, "Refresh scene list"))
        RefreshSceneListInternal();
    }

    private void RefreshSceneListInternal()
    {
      if (_isRefreshing) return;
      _isRefreshing = true;
      try
      {
        if (_allScenesCache == null)
          InitializeViewModelLists();

        // ReSharper disable once PossibleNullReferenceException
        _allScenesCache.Clear();

        var tempFavoriteSceneEntries = new List<SceneEntry>();
        var tempOtherSceneEntries = new List<SceneEntry>();
        var currentFavoriteGuidsFromData = new List<string>(_data.FavoriteSceneGuids);

        string[] sceneGuidsInProject = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in sceneGuidsInProject)
        {
          string path = AssetDatabase.GUIDToAssetPath(guid);
          if (string.IsNullOrEmpty(path) || !path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) continue;

          SceneEntry entry = new(guid, path, this);
          _allScenesCache[guid] = entry;
          entry.IsFavorite = currentFavoriteGuidsFromData.Contains(guid);

          if (entry.IsFavorite) tempFavoriteSceneEntries.Add(entry);
          else tempOtherSceneEntries.Add(entry);
        }

        List<SceneEntry> newFavoriteScenesViewModel = tempFavoriteSceneEntries.OrderBy(entry => currentFavoriteGuidsFromData.IndexOf(entry.Guid)).ToList();
        bool favViewModelChanged = !_favoriteScenes.SequenceEqual(newFavoriteScenesViewModel) || _favoriteScenes.Count != newFavoriteScenesViewModel.Count;

        if (favViewModelChanged)
          _favoriteScenes = newFavoriteScenesViewModel;

        _otherScenes = tempOtherSceneEntries
          .OrderBy(entry => !IsSceneInPrioritizedFolder(entry.Path))
          .ThenBy(entry => entry.Name)
          .ToList();

        var currentProjectGuidsSet = new HashSet<string>(sceneGuidsInProject);
        int originalDataFavCount = _data.FavoriteSceneGuids.Count;
        _data.FavoriteSceneGuids.RemoveAll(guid => !currentProjectGuidsSet.Contains(guid));
        if (_data.FavoriteSceneGuids.Count != originalDataFavCount)
        {
          EditorUtility.SetDirty(_data);
        }

        if (favViewModelChanged || _data.FavoriteSceneGuids.Count != originalDataFavCount) // Maybe just set true
        {
          EditorApplication.delayCall += ParentForceMenuTreeRebuild;
        }
      }
      finally
      {
        _isRefreshing = false;
      }
    }

    private bool IsSceneInPrioritizedFolder(string scenePath)
    {
      string normalizedPath = scenePath.Replace("\\", "/");
      return normalizedPath.StartsWith("Assets/Scenes/", StringComparison.OrdinalIgnoreCase) ||
             normalizedPath.Contains("/Scenes/");
    }

    internal void ToggleFavoriteInLogic(SceneEntry entryClicked)
    {
      if (_isRefreshing || _data == null)
        return;

      bool isCurrentlyFavoriteInData = _data.FavoriteSceneGuids.Contains(entryClicked.Guid);
      bool desiredFavoriteState = !isCurrentlyFavoriteInData;

      entryClicked.IsFavorite = desiredFavoriteState;

      if (desiredFavoriteState)
      {
        _data.AddFavorite(entryClicked.Guid);
        _otherScenes.RemoveAll(e => e.Guid == entryClicked.Guid);

        List<SceneEntry> tempFavs = _favoriteScenes.Where(e => e.Guid != entryClicked.Guid).ToList();
        tempFavs.Insert(0, entryClicked);
        _favoriteScenes = tempFavs;
      }
      else
      {
        _data.RemoveFavorite(entryClicked.Guid);
        _favoriteScenes = _favoriteScenes.Where(e => e.Guid != entryClicked.Guid).ToList();

        // ReSharper disable once SimplifyLinqExpressionUseAll
        if (!_otherScenes.Any(e => e.Guid == entryClicked.Guid))
        {
          _otherScenes.Add(entryClicked);
          _otherScenes = _otherScenes
            .OrderBy(entry => !IsSceneInPrioritizedFolder(entry.Path))
            .ThenBy(entry => entry.Name)
            .ToList();
        }
      }

      EditorApplication.delayCall += ParentForceMenuTreeRebuild;
    }

    private void ParentForceMenuTreeRebuild()
    {
      if (_parentEditorWindow != null)
        _parentEditorWindow.ForceMenuTreeRebuild();
    }

    public void HandleFavoriteScenesListChangedAfter(CollectionChangeInfo info)
    {
      if (_isRefreshing || _data == null) return;

      List<string> newOrderedGuids = _favoriteScenes.Select(e => e.Guid).ToList();
      _data.SetFavoriteOrder(newOrderedGuids);
    }
  }
}
#endif