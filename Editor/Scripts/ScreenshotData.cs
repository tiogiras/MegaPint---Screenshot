﻿using Editor.Scripts.Settings;
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

    public static string LastEditorWindowPath
    {
        get
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Screenshot");

            return s_settings.GetValue("lastEditorWindowPath", "Assets");
        }
        set
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Screenshot");

            s_settings.SetValue("lastEditorWindowPath", value);
        }
    }

    public static string RenderPipelineAssetPath
    {
        get
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Screenshot");

            return s_settings.GetValue("pipelineAsset", "");
        }
        set
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Screenshot");

            s_settings.SetValue("pipelineAsset", value);
        }
    }

    public static string RendererDataPath
    {
        get
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Screenshot");

            return s_settings.GetValue("rendererData", "");
        }
        set
        {
            s_settings ??= MegaPintSettings.instance.GetSetting("Screenshot");

            s_settings.SetValue("rendererData", value);
        }
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
