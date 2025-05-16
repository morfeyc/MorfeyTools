using System.IO;
using System.Linq;
using Plugins.MorfeyTools.Editor.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
// ReSharper disable InconsistentNaming
// Add this for SceneManager

namespace Plugins.MorfeyTools.Editor.MBootsrtap
{
  [InitializeOnLoad]
  public static class PlayModeStartScene
  {
    public const string DefaultBootstrapSceneName = "Bootstrap";
    private const string EnableMenuName = "Tools/MorfeyTools/Load Bootstrap Scene/Enabled";

    private const string SettingsAssetName = "BootstrapSettings.asset";
    private const string SettingsSOFolderPath = "Assets/Plugins/MorfeyTools/Editor/Data";
    private const string FullSettingsAssetPath = SettingsSOFolderPath + "/" + SettingsAssetName;

    private static BootstrapSettingsData _settingsCache;

    private static SceneAsset _bootstrapSceneAsset;
    private static SceneAsset _lastOpenedSceneAsset;
    private static bool _isBootstrapSceneAssetValid;

    static PlayModeStartScene()
    {
      LoadOrCreateSettings();
      TryInit();
    }

    private static void LoadOrCreateSettings()
    {
      if (_settingsCache != null) return;
      _settingsCache = AssetDatabase.LoadAssetAtPath<BootstrapSettingsData>(FullSettingsAssetPath);

      if (_settingsCache == null)
      {
        Debug.LogWarning($"[{nameof(PlayModeStartScene)}] {SettingsAssetName} not found at {FullSettingsAssetPath}. Attempting to create a new one.");
        if (!Directory.Exists(SettingsSOFolderPath))
        {
          Directory.CreateDirectory(SettingsSOFolderPath);
          AssetDatabase.Refresh();
        }

        _settingsCache = ScriptableObject.CreateInstance<BootstrapSettingsData>();
        AssetDatabase.CreateAsset(_settingsCache, FullSettingsAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[{nameof(PlayModeStartScene)}] Created {SettingsAssetName} at {FullSettingsAssetPath}. Please review its default settings.");
      }
    }

    private static void MarkSettingsDirty()
    {
      if (_settingsCache != null)
      {
        EditorUtility.SetDirty(_settingsCache);
      }
      else
      {
        Debug.LogError($"[{nameof(PlayModeStartScene)}] Attempted to mark settings dirty, but settings data asset is not loaded!");
      }
    }

    public static bool IsEnabled
    {
      get => Settings().IsSystemEnabled;
      set
      {
        if (Settings().IsSystemEnabled != value)
        {
          Settings().IsSystemEnabled = value;
          MarkSettingsDirty();
          ChangePlayModeStartScene();
        }
      }
    }

    public static string GetStartSceneName() => Settings().ConfiguredStartSceneName;

    public static void SetStartSceneName(string name)
    {
      if (Settings().ConfiguredStartSceneName != name)
      {
        Settings().ConfiguredStartSceneName = name;
        MarkSettingsDirty();
      }
    }

    public static string GetLastOpenedScenePath() => Settings().LastOpenedEditorScenePath;

    private static void UpdateLastOpenedScenePathInSO(string path)
    {
      if (Settings().LastOpenedEditorScenePath != path)
      {
        Settings().LastOpenedEditorScenePath = path;
        MarkSettingsDirty();
      }
    }

    private static BootstrapSettingsData Settings()
    {
      if (_settingsCache == null)
      {
        LoadOrCreateSettings();
      }

      return _settingsCache;
    }

    public static bool TryInit()
    {
      _bootstrapSceneAsset = null;
      _isBootstrapSceneAssetValid = false;
      _lastOpenedSceneAsset = null;

      LoadBootstrapSceneAssetFromConfig();
      LoadLastOpenedSceneAssetFromSO();

      EditorSceneManager.sceneOpened -= OnSceneOpened;
      EditorSceneManager.sceneOpened += OnSceneOpened;
      
      if (SceneManager.loadedSceneCount > 0 && _lastOpenedSceneAsset == null && string.IsNullOrEmpty(GetLastOpenedScenePath()))
      {
        Scene currentActiveScene = SceneManager.GetActiveScene();
        if (currentActiveScene.IsValid() && !string.IsNullOrEmpty(currentActiveScene.path))
        {
          OnSceneOpened(currentActiveScene, OpenSceneMode.Single);
        }
      }

      ChangePlayModeStartScene();

      if (LogInitialization())
      {
        string effectiveStart = "None (System Disabled or Scene Invalid)";
        if (IsEnabled && _bootstrapSceneAsset != null && IsConfiguredSceneActuallyEnabledAndInBuild())
        {
          effectiveStart = _bootstrapSceneAsset.name;
        }
        else if (!IsEnabled && _lastOpenedSceneAsset != null)
        {
          effectiveStart = _lastOpenedSceneAsset.name;
        }

        Debug.Log($"[{ScriptName()}] Initialized. Status: {GetSystemStatusText()}. Effective Play Start: {effectiveStart}");
      }

      return IsEnabled && _isBootstrapSceneAssetValid && IsConfiguredSceneActuallyEnabledAndInBuild();
    }

    private static bool LogInitialization() => true;

    [MenuItem(EnableMenuName, false, 0)]
    public static void ToggleEnable()
    {
      if (!IsEnabled && !IsConfiguredSceneActuallyEnabledAndInBuild())
      {
        Debug.LogWarning($"[{ScriptName()}] Cannot enable. No valid (enabled in Build Settings) bootstrap scene found for configuration: '{GetEffectiveConfiguredSceneIdentifier()}'.");
        if (Settings().IsSystemEnabled)
        {
          Settings().IsSystemEnabled = false;
          MarkSettingsDirty();
        }

        return;
      }

      IsEnabled = !IsEnabled;
      Debug.Log($"[{ScriptName()}] System is now {GetSystemStatusText()}.");
    }

    [MenuItem(EnableMenuName, true)]
    private static bool ToggleEnableValidate()
    {
      Menu.SetChecked(EnableMenuName, IsEnabled);
      return IsEnabled || (!IsEnabled && IsConfiguredSceneActuallyEnabledAndInBuild());
    }

    private static void LoadBootstrapSceneAssetFromConfig()
    {
      _bootstrapSceneAsset = null;
      _isBootstrapSceneAssetValid = false;
      string sceneIdentifier = GetEffectiveConfiguredSceneIdentifier();

      EditorBuildSettingsScene foundBuildScene = EditorBuildSettings.scenes
        .FirstOrDefault(s => s.path.Contains(sceneIdentifier));

      if (foundBuildScene != null && !string.IsNullOrEmpty(foundBuildScene.path))
      {
        _bootstrapSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(foundBuildScene.path);
        _isBootstrapSceneAssetValid = _bootstrapSceneAsset != null;
      }
      else
      {
        if (!string.IsNullOrEmpty(GetStartSceneName()) && GetStartSceneName() != DefaultBootstrapSceneName) 
          Debug.LogWarning($"[{ScriptName()}] Configured bootstrap scene identifier '{GetStartSceneName()}' not found in any Build Settings scene path.");
      }
    }

    public static string GetEffectiveConfiguredSceneIdentifier()
    {
      string configuredName = GetStartSceneName();
      return string.IsNullOrEmpty(configuredName) ? DefaultBootstrapSceneName : configuredName;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
      if (scene.IsValid() && !string.IsNullOrEmpty(scene.path))
      {
        _lastOpenedSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
        UpdateLastOpenedScenePathInSO(scene.path);
      }
    }

    private static void LoadLastOpenedSceneAssetFromSO()
    {
      string lastOpenedPath = GetLastOpenedScenePath();
      if (!string.IsNullOrEmpty(lastOpenedPath))
      {
        _lastOpenedSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(lastOpenedPath);
        if (_lastOpenedSceneAsset == null && File.Exists(lastOpenedPath))
        {
          Debug.LogWarning($"[{ScriptName()}] Last opened scene path '{lastOpenedPath}' exists but couldn't be loaded as SceneAsset.");
        }
        else if (_lastOpenedSceneAsset == null && !File.Exists(lastOpenedPath))
        {
          UpdateLastOpenedScenePathInSO("");
        }
      }
    }

    private static void ChangePlayModeStartScene()
    {
      if (IsEnabled && _bootstrapSceneAsset != null && IsConfiguredSceneActuallyEnabledAndInBuild())
      {
        EditorSceneManager.playModeStartScene = _bootstrapSceneAsset;
      }
      else
      {
        EditorSceneManager.playModeStartScene = _lastOpenedSceneAsset;
      }
    }

    public static bool IsConfiguredSceneActuallyEnabledAndInBuild()
    {
      string sceneIdentifier = GetEffectiveConfiguredSceneIdentifier();
      EditorBuildSettingsScene sceneEntry = EditorBuildSettings.scenes
        .FirstOrDefault(s => s.enabled && s.path.Contains(sceneIdentifier));
      return sceneEntry != null;
    }

    public static SceneAsset GetEffectiveBootstrapSceneAsset()
    {
      if (IsEnabled && _bootstrapSceneAsset != null && IsConfiguredSceneActuallyEnabledAndInBuild())
        return _bootstrapSceneAsset;

      return null;
    }

    private static string ScriptName() 
      => $"<b>{nameof(PlayModeStartScene)}</b>";
    
    private static string GetSystemStatusText() 
      => IsEnabled ? "<color=#7fff00>enabled</color>" : "<color=#ffa500>disabled</color>";
  }
}