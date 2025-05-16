using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Plugins.MorfeyTools.Editor.MScenes
{
  public class SceneEntryDrawer : OdinValueDrawer<MScenesEditor.SceneEntry>
  {
    private static Texture2D _starFavoriteTexture;
    private static Texture2D _starNonFavoriteTexture;

    private static void EnsureTextures()
    {
      if (_starFavoriteTexture == null)
      {
        _starFavoriteTexture = SdfIcons.CreateTransparentIconTexture(SdfIconType.StarFill, Color.yellow, 18, 18, 1);
        CleanupUtility.DestroyObjectOnAssemblyReload(_starFavoriteTexture);
      }

      if (_starNonFavoriteTexture == null)
      {
        _starNonFavoriteTexture = SdfIcons.CreateTransparentIconTexture(SdfIconType.Star, Color.white, 18, 18, 1);
        CleanupUtility.DestroyObjectOnAssemblyReload(_starNonFavoriteTexture);
      }
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
      EnsureTextures();

      MScenesEditor.SceneEntry sceneEntry = this.ValueEntry.SmartValue;

      if (sceneEntry == null)
      {
        SirenixEditorGUI.ErrorMessageBox("SceneEntry data is null. Cannot draw element.");
        return;
      }

      SirenixEditorGUI.BeginBox();
      {
        EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(EditorGUIUtility.singleLineHeight));
        {
          EditorGUILayout.LabelField(new GUIContent(sceneEntry.Name, sceneEntry.Path), EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
          Texture2D starTex = sceneEntry.IsFavorite ? _starFavoriteTexture : _starNonFavoriteTexture;
          GUIContent starButtonContent = new(starTex, sceneEntry.IsFavorite ? "Unfavorite" : "Favorite");

          GUIStyle iconButtonStyle = new(GUI.skin.button)
          {
            padding = new RectOffset(2, 2, 2, 2),
            fixedWidth = 28,
            fixedHeight = 22
          };

          if (GUILayout.Button(starButtonContent, iconButtonStyle))
            sceneEntry.ToggleFavorite();

          if (GUILayout.Button(new GUIContent("Open", "Open Scene"), GUILayout.Width(60)))
            sceneEntry.OpenScene();

          if (GUILayout.Button(new GUIContent("Location", "Reveal in Project"), GUILayout.Width(75)))
            sceneEntry.OpenSceneLocation();
        }
        EditorGUILayout.EndHorizontal();
      }
      SirenixEditorGUI.EndBox();
      GUILayout.Space(2);
    }
  }
}