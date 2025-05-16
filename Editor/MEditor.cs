using Plugins.MorfeyTools.Editor.MBootsrtap;
using Plugins.MorfeyTools.Editor.MConfigs;
using Plugins.MorfeyTools.Editor.MScenes;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Plugins.MorfeyTools.Editor
{
  public class MEditor : OdinMenuEditorWindow
  {
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