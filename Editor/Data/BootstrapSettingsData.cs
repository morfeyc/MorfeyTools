using UnityEngine;

namespace MorfeyTools.Editor.Data
{
  public class BootstrapSettingsData : ScriptableObject
  {
    public bool IsSystemEnabled = false;
    public string ConfiguredStartSceneName = "";
    public string LastOpenedEditorScenePath = "";
  }
}