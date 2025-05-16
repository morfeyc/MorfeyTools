namespace MorfeyTools.Editor
{
  public static class MBootstrapEditorStrings
  {
    // Log Prefix
    public const string LogPrefix = "[MBootstrapEditor] ";

    // CandidateBootstrapScene related messages
    public const string CandidateSceneReadyStatus = "<color=#7fff00><b>Ready:</b> Scene is in Build Settings and enabled. Click button below to set.</color>";
    public const string CandidateSceneWarningDisabledStatus = "<color=orange><b>Warning:</b> Scene is in Build Settings but <b>disabled</b>. It must be enabled to be set as bootstrap.</color>";
    public const string CandidateSceneActionRequiredNotInBuildStatus = "<color=red><b>Action Required:</b> Scene is <b>NOT in Build Settings</b>. Add and enable it to set as bootstrap.</color>";

    public const string BootstrapSceneNotSelectedError = "No candidate bootstrap scene selected.";
    public const string CouldNotGetPathForSceneErrorFormat = "Could not get path for scene: {0}";

    // SetAndInitializeBootstrapScene Button
    public const string SetAsPlayModeBootstrapButton = "Set as Play Mode Bootstrap Scene";

    // Logs after setting scene
    public const string PreferredStartSceneSetLogFormat = "Preferred start scene name set to: '{0}'. Calling TryInit() on PlayModeStartScene to apply changes.";
    public const string SystemAutoEnabledLog = "PlayModeStartScene system was disabled, automatically enabling it now.";
    public const string SystemCouldNotAutoEnableWarning = "PlayModeStartScene system is disabled, but could not auto-enable as no valid bootstrap scene was found by the system after setting the name.";

    // System Status Display
    public const string ConfiguredBootstrapScenePathLabel = "Configured Bootstrap Scene";
    public const string ConfiguredBootstrapScenePathTooltip = "The scene PlayModeStartScene is configured to look for, based on its name setting. Path shown if found in Build Settings.";

    public const string ConfiguredScenePathOnlyFormat = "'{0}'";
    public const string ConfiguredScenePathNotFoundFormat = "Identifier '{0}' not found in Build Settings scenes.";
    public const string DefaultSceneLabel = "Default: ";

    public const string SystemEnabledRichText = "<color=#7fff00><size=14><b>SYSTEM ENABLED</b></size></color>";
    public const string SystemDisabledRichText = "<color=#ffa500><size=14><b>SYSTEM DISABLED</b></size></color>";

    public const string EffectiveSceneLabel = "Effective Play Start Scene (Current)";
    public const string EffectiveSceneInfoBox = "This is the scene that will load if you press Play NOW, assuming the system is enabled and the scene is valid.";

    // System Actions
    public const string ReInitializeButton = "Re-Initialize PlayModeStartScene System";
    public const string ReInitializeLog = "PlayModeStartScene.TryInit() called.";

    public const string ToggleButtonCannotEnable = "Cannot Enable (No valid & enabled Bootstrap Scene in Build Settings)";
    public const string ToggleButtonDisable = "DISABLE Bootstrap Start";
    public const string ToggleButtonEnable = "ENABLE Bootstrap Start";

    // Utilities
    public const string OpenBuildSettingsButton = "Open Build Settings";
    public const string OpenEffectiveSceneButton = "Open Effective Play Start Scene";
  }
}