using Editor.Scripts.Settings;
#if UNITY_EDITOR && USING_URP
using UnityEditor;
#endif

#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

#if UNITY_EDITOR
namespace Editor.Scripts
{

internal static class ScreenshotData
{
    private static MegaPintSettingsBase s_settings;
    
    private static MegaPintSettingsBase _Settings => s_settings ??= MegaPintSettings.instance.GetSetting("Screenshot");

    public static string LastEditorWindowPath
    {
        get => _Settings.GetValue("lastEditorWindowPath", "Assets");
        set => _Settings.SetValue("lastEditorWindowPath", value);
    }

    public static string RenderPipelineAssetPath
    {
        get => _Settings.GetValue("pipelineAsset", "");
        set => _Settings.SetValue("pipelineAsset", value);
    }

    public static string RendererDataPath
    {
        get => _Settings.GetValue("rendererData", "");
        set => _Settings.SetValue("rendererData", value);
    }

    public static bool ExternalExport
    {
        get => _Settings.GetValue("externalExport", false);
        set => _Settings.SetValue("externalExport", value);
    }

    #region Public Methods

#if USING_URP
    public static UniversalRenderPipelineAsset RenderPipelineAsset()
    {
        return AssetDatabase.
            LoadAssetAtPath <UniversalRenderPipelineAsset>(RenderPipelineAssetPath);
    }
#endif

    #endregion
}

}
#endif
