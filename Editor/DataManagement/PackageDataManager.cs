using System.IO;
using UnityEditor;
using UnityEngine;

namespace MorfeyTools.Editor.DataManagement
{
  /// <summary>
  /// Helper class to manage ScriptableObject data assets for MorfeyTools editor tools.
  /// Data is stored in the project's Assets folder at a fixed, predefined path.
  /// </summary>
  public static class PackageDataManager
  {
    private const string HardcodedEditorDataFolderPath = "Assets/Plugins/MorfeyTools/Editor/Data";

    /// <summary>
    /// Loads an existing ScriptableObject data asset of type TData or creates a new one if it doesn't exist.
    /// The asset will be stored in "Assets/Plugins/MorfeyTools/Editor/Data".
    /// By default, the asset will be named based on the type TData (e.g., "MySettingsData.asset").
    /// </summary>
    /// <typeparam name="TData">The type of ScriptableObject to load or create.</typeparam>
    /// <param name="customAssetNameWithExtension">Optional: A custom asset name including extension (e.g., "MyCustomName.asset").
    /// If null or empty, typeof(TData).Name + ".asset" is used.</param>
    /// <returns>The loaded or newly created TData asset, or null if creation failed.</returns>
    public static TData LoadOrCreateDataAsset<TData>(string customAssetNameWithExtension = null) where TData : ScriptableObject
    {
      string assetName = string.IsNullOrEmpty(customAssetNameWithExtension)
        ? typeof(TData).Name + ".asset"
        : customAssetNameWithExtension;

      if (!string.IsNullOrEmpty(customAssetNameWithExtension) && !customAssetNameWithExtension.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase))
      {
        assetName += ".asset";
        Debug.LogWarning($"[{nameof(PackageDataManager)}] Custom asset name '{customAssetNameWithExtension}' did not have '.asset' extension. Appended it: '{assetName}'");
      }

      string fullAssetPath = Path.Combine(HardcodedEditorDataFolderPath, assetName);
      TData dataAsset = AssetDatabase.LoadAssetAtPath<TData>(fullAssetPath);

      if (dataAsset == null)
      {
        Debug.Log($"[{nameof(PackageDataManager)}] Asset '{assetName}' of type '{typeof(TData).Name}' not found at '{fullAssetPath}'. Attempting to create a new one.");

        if (!Directory.Exists(HardcodedEditorDataFolderPath))
        {
          try
          {
            Directory.CreateDirectory(HardcodedEditorDataFolderPath);
          }
          catch (System.Exception e)
          {
            Debug.LogError($"[{nameof(PackageDataManager)}] Failed to create directory '{HardcodedEditorDataFolderPath}'. Error: {e.Message}");
            return null;
          }
        }

        dataAsset = ScriptableObject.CreateInstance<TData>();
        if (dataAsset == null)
        {
          Debug.LogError($"[{nameof(PackageDataManager)}] Failed to create an instance of ScriptableObject type '{typeof(TData).Name}'. " +
                         $"Ensure the type is not abstract and has a parameterless constructor.");
          return null;
        }

        try
        {
          AssetDatabase.CreateAsset(dataAsset, fullAssetPath);
          AssetDatabase.SaveAssets();
          AssetDatabase.Refresh();

          Debug.Log($"[{nameof(PackageDataManager)}] Successfully created and saved asset '{fullAssetPath}'.");
        }
        catch (System.Exception e)
        {
          Debug.LogError($"[{nameof(PackageDataManager)}] Failed to create asset file at '{fullAssetPath}'. Error: {e.Message}");
          Object.DestroyImmediate(dataAsset);
          return null;
        }
      }

      return dataAsset;
    }
  }
}