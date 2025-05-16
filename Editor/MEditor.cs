#if ODIN_INSPECTOR
using MorfeyTools.Editor.Plugins.MorfeyTools.Editor.MConfigs;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace MorfeyTools.Editor
{
  public class MEditor : OdinMenuEditorWindow
  {
    private const float DefaultWindowWidth = 1100;
    private const float DefaultWindowHeight = 800;
    
    [MenuItem("Tools/Toggle Editor _`")]
    private static void ToggleWindow()
    {
      MEditor[] windows = Resources.FindObjectsOfTypeAll<MEditor>();
      bool windowExists = windows is { Length: > 0 };

      if (windowExists)
      {
        windows[0].Close();
      }
      else
      {
        MEditor window = GetWindow<MEditor>(title: "M Editor", utility: false);
        RectInt mainScreen = Screen.mainWindowDisplayInfo.workArea;
        float xPos = (mainScreen.width - DefaultWindowWidth) / 2f + mainScreen.x;
        float yPos = (mainScreen.height - DefaultWindowHeight) / 2f + mainScreen.y;
        xPos = Mathf.Max(mainScreen.x, xPos);
        yPos = Mathf.Max(mainScreen.y, yPos);
        window.position = new Rect(xPos, yPos, DefaultWindowWidth, DefaultWindowHeight);
        window.Show();
        window.Focus();
      }
    }
    
    private MConfigsEditor _configsEditor;
    private MScenesEditor _scenesEditor;
    private MBootstrapEditor _bootstrapEditor;


    protected override OdinMenuTree BuildMenuTree()
    {
      // ReSharper disable once UseObjectOrCollectionInitializer
      OdinMenuTree tree = new(); 
      tree.Selection.SupportsMultiSelect = false; 
      
      _configsEditor ??= new MConfigsEditor();
      _scenesEditor ??= new MScenesEditor(this);
      _bootstrapEditor ??= new MBootstrapEditor();

      tree.Add("Configs", _configsEditor);
      tree.Add("Scenes", _scenesEditor); 
      tree.Add("Bootstrap", _bootstrapEditor);

      return tree;
    }
  }
}
#endif