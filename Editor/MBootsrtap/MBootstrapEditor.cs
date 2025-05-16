#if ODIN_INSPECTOR
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MorfeyTools.Editor
{
  public class MBootstrapEditor
  {
    // --- Select Play Mode Start Scene Group ---
    [BoxGroup("Select Play Mode Start Scene", Order = 0)]
    [AssetsOnly]
    [OnValueChanged(nameof(UpdateCandidateSceneStatusInternal))]
    [InfoBox("$_candidateSceneStatusMessageInternal", InfoMessageType.None, VisibleIf = nameof(ShowCandidateStatusInfoBox))]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnassignedField.Global
    public SceneAsset CandidateBootstrapScene;

    [BoxGroup("Select Play Mode Start Scene")]
    [Button(MBootstrapEditorStrings.SetAsPlayModeBootstrapButton, ButtonSizes.Large)]
    [EnableIf(nameof(IsCandidateSceneValidAndEnabledForSetButton))]
    [GUIColor(0.2f, 1f, 0.5f)]
    public void SetAndInitializeBootstrapScene()
    {
      if (CandidateBootstrapScene == null)
      {
        Debug.LogError(MBootstrapEditorStrings.LogPrefix + MBootstrapEditorStrings.BootstrapSceneNotSelectedError);
        return;
      }

      string scenePath = AssetDatabase.GetAssetPath(CandidateBootstrapScene);
      if (string.IsNullOrEmpty(scenePath))
      {
        Debug.LogError(MBootstrapEditorStrings.LogPrefix + string.Format(MBootstrapEditorStrings.CouldNotGetPathForSceneErrorFormat, CandidateBootstrapScene.name));
        return;
      }

      string sceneNameOnly = CandidateBootstrapScene.name;
      PlayModeStartScene.SetStartSceneName(sceneNameOnly);
      Debug.Log(MBootstrapEditorStrings.LogPrefix + string.Format(MBootstrapEditorStrings.PreferredStartSceneSetLogFormat, sceneNameOnly));

      PlayModeStartScene.TryInit();

      if (!PlayModeStartScene.IsEnabled)
      {
        if (PlayModeStartScene.IsConfiguredSceneActuallyEnabledAndInBuild())
        {
          PlayModeStartScene.IsEnabled = true;
          Debug.Log(MBootstrapEditorStrings.LogPrefix + MBootstrapEditorStrings.SystemAutoEnabledLog);
        }
        else
        {
          Debug.LogWarning(MBootstrapEditorStrings.LogPrefix + MBootstrapEditorStrings.SystemCouldNotAutoEnableWarning);
        }
      }

      GUI.changed = true;
      UpdateCandidateSceneStatusInternal();
    }

    // --- System Status Section ---
    [BoxGroup("System Status", Order = 1)]
    [ShowInInspector, PropertyOrder(0)]
    [HideLabel]
    [DisplayAsString(TextAlignment.Left, true)]
    public string SystemCurrentStatusForDisplay => PlayModeStartScene.IsEnabled
      ? MBootstrapEditorStrings.SystemEnabledRichText
      : MBootstrapEditorStrings.SystemDisabledRichText;

    [BoxGroup("System Status")]
    [ShowInInspector, ReadOnly, PropertyOrder(1)]
    [LabelText(MBootstrapEditorStrings.ConfiguredBootstrapScenePathLabel)]
    [PropertyTooltip(MBootstrapEditorStrings.ConfiguredBootstrapScenePathTooltip)]
    public string DisplayedConfiguredBootstrapScenePath
    {
      get
      {
        string sceneIdentifier = PlayModeStartScene.GetEffectiveConfiguredSceneIdentifier();
        string prefix = sceneIdentifier == PlayModeStartScene.DefaultBootstrapSceneName && !string.IsNullOrEmpty(PlayModeStartScene.GetStartSceneName()) && PlayModeStartScene.GetStartSceneName() == PlayModeStartScene.DefaultBootstrapSceneName
          ? ""
          : (string.IsNullOrEmpty(PlayModeStartScene.GetStartSceneName()) ? MBootstrapEditorStrings.DefaultSceneLabel : "");

        EditorBuildSettingsScene foundBuildSceneEntry = null;
        
        string[] guids = AssetDatabase.FindAssets($"t:SceneAsset {Path.GetFileNameWithoutExtension(sceneIdentifier)}");
        if (guids.Length > 0)
        {
          string potentialPath = AssetDatabase.GUIDToAssetPath(guids[0]);
          foundBuildSceneEntry = EditorBuildSettings.scenes.FirstOrDefault(s => s.path == potentialPath);
        }
        
        foundBuildSceneEntry ??= EditorBuildSettings.scenes.FirstOrDefault(s => s.path.Contains(sceneIdentifier));


        if (foundBuildSceneEntry != null)
        {
          return prefix + string.Format(MBootstrapEditorStrings.ConfiguredScenePathOnlyFormat, foundBuildSceneEntry.path);
        }
        else
        {
          return prefix + string.Format(MBootstrapEditorStrings.ConfiguredScenePathNotFoundFormat, sceneIdentifier);
        }
      }
    }

    [BoxGroup("System Status")]
    [ShowInInspector, ReadOnly, PropertyOrder(3)]
    [LabelText(MBootstrapEditorStrings.EffectiveSceneLabel)]
    [InfoBox(MBootstrapEditorStrings.EffectiveSceneInfoBox, InfoMessageType.None)]
    public SceneAsset EffectivePlayStartSceneBasedOnSystemState
    {
      get
      {
        if (PlayModeStartScene.IsEnabled)
        {
          return PlayModeStartScene.GetEffectiveBootstrapSceneAsset();
        }

        string lastOpenedPath = PlayModeStartScene.GetLastOpenedScenePath();
        return !string.IsNullOrEmpty(lastOpenedPath)
          ? AssetDatabase.LoadAssetAtPath<SceneAsset>(lastOpenedPath)
          : null;
      }
    }

    // --- System Actions Section ---
    [BoxGroup("System Actions", Order = 2)]
    [Button(MBootstrapEditorStrings.ReInitializeButton, ButtonSizes.Large)]
    [PropertyOrder(10)]
    [GUIColor(0.4f, 0.8f, 1f)]
    private void InitializeOrRefreshSystem()
    {
      PlayModeStartScene.TryInit();
      Debug.Log(MBootstrapEditorStrings.LogPrefix + MBootstrapEditorStrings.ReInitializeLog);
      GUI.changed = true;
    }

    [BoxGroup("System Actions")]
    [Button("@GetToggleEnableButtonText()", ButtonSizes.Large)]
    [PropertyOrder(11)]
    [GUIColor(nameof(GetToggleEnableButtonColor))]
    [EnableIf(nameof(CanTogglePlayModeStartSystem))]
    private void ToggleBootstrapEnable()
    {
      PlayModeStartScene.ToggleEnable();
      GUI.changed = true;
    }

    // --- Utilities Group ---
    [BoxGroup("Utilities", ShowLabel = true, Order = 3)]
    [HorizontalGroup("Utilities/Buttons")]
    [Button(MBootstrapEditorStrings.OpenBuildSettingsButton, ButtonSizes.Medium)]
    private void OpenBuildSettings()
    {
      EditorWindow.GetWindow<BuildPlayerWindow>(false, "Build Settings", true);
    }

    [HorizontalGroup("Utilities/Buttons")]
    [Button(MBootstrapEditorStrings.OpenEffectiveSceneButton, ButtonSizes.Medium)]
    [EnableIf(nameof(EffectivePlayStartSceneBasedOnSystemState))]
    private void OpenEffectivePlayStartSceneFile()
    {
      if (EffectivePlayStartSceneBasedOnSystemState != null)
      {
        AssetDatabase.OpenAsset(EffectivePlayStartSceneBasedOnSystemState);
        EditorGUIUtility.PingObject(EffectivePlayStartSceneBasedOnSystemState);
      }
    }

    // --- Private Helper Fields & Methods ---
    private string _candidateSceneStatusMessageInternal = "";

    private bool IsCandidateSceneValidAndEnabledForSetButton()
    {
      if (CandidateBootstrapScene == null) return false;
      string scenePath = AssetDatabase.GetAssetPath(CandidateBootstrapScene);
      if (string.IsNullOrEmpty(scenePath)) return false;
      EditorBuildSettingsScene sceneInBuild = EditorBuildSettings.scenes.FirstOrDefault(s => s.path == scenePath);
      return sceneInBuild is { enabled: true };
    }

    private bool ShowCandidateStatusInfoBox()
      => CandidateBootstrapScene != null && !string.IsNullOrEmpty(_candidateSceneStatusMessageInternal);

    private void UpdateCandidateSceneStatusInternal()
    {
      if (CandidateBootstrapScene == null)
      {
        _candidateSceneStatusMessageInternal = "";
        GUI.changed = true;
        return;
      }

      string scenePath = AssetDatabase.GetAssetPath(CandidateBootstrapScene);
      EditorBuildSettingsScene sceneInBuild = EditorBuildSettings.scenes.FirstOrDefault(s => s.path == scenePath);

      if (sceneInBuild != null)
      {
        _candidateSceneStatusMessageInternal = sceneInBuild.enabled
          ? MBootstrapEditorStrings.CandidateSceneReadyStatus
          : MBootstrapEditorStrings.CandidateSceneWarningDisabledStatus;
      }
      else
      {
        _candidateSceneStatusMessageInternal = MBootstrapEditorStrings.CandidateSceneActionRequiredNotInBuildStatus;
      }

      GUI.changed = true;
    }

    private bool CanTogglePlayModeStartSystem()
    {
      if (PlayModeStartScene.IsEnabled) return true;
      return PlayModeStartScene.IsConfiguredSceneActuallyEnabledAndInBuild();
    }

    // ReSharper disable once UnusedMember.Local
    private string GetToggleEnableButtonText()
    {
      if (!PlayModeStartScene.IsEnabled && !PlayModeStartScene.IsConfiguredSceneActuallyEnabledAndInBuild())
      {
        return MBootstrapEditorStrings.ToggleButtonCannotEnable;
      }

      return PlayModeStartScene.IsEnabled
        ? MBootstrapEditorStrings.ToggleButtonDisable
        : MBootstrapEditorStrings.ToggleButtonEnable;
    }

    private Color GetToggleEnableButtonColor()
    {
      if (!PlayModeStartScene.IsEnabled && !PlayModeStartScene.IsConfiguredSceneActuallyEnabledAndInBuild())
      {
        return Color.gray;
      }

      return PlayModeStartScene.IsEnabled
        ? new Color(1f, 0.6f, 0.6f)
        : new Color(0.6f, 1f, 0.6f);
    }
  }
}
#endif