using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Plugins.MorfeyTools.Editor.MScenes
{
  public partial class MScenesEditor
  {
    [Serializable]
    public class SceneEntry
    {
      [HideInInspector] public string Guid;
      [HideInInspector] public string Path;
      [HideInInspector] public string Name { get; private set; }
      [HideInInspector] public bool IsFavorite;

      internal MScenesEditor Editor;

      public SceneEntry(string guid, string path, MScenesEditor editor)
      {
        Guid = guid;
        Path = path;
        Name = System.IO.Path.GetFileNameWithoutExtension(path);
        Editor = editor;
      }

      public void ToggleFavorite()
      {
        if (Editor?._data != null)
        {
          Undo.RecordObject(Editor._data, "Toggle Scene Favorite");
        }

        Editor?.ToggleFavoriteInLogic(this);
      }

      public void OpenScene()
      {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
          EditorSceneManager.OpenScene(Path, OpenSceneMode.Single);
        }
      }

      public void OpenSceneLocation()
      {
        UnityEngine.Object sceneAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(Path);
        if (sceneAsset != null)
        {
          EditorGUIUtility.PingObject(sceneAsset);
          Selection.activeObject = sceneAsset;
        }
      }
    }
  }
}