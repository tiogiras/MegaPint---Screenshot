#if UNITY_EDITOR
using MegaPint.Editor.Scripts.Settings;
#if USING_URP
using UnityEngine.Rendering.Universal;
#endif

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial class storing saveData values (Screenshot) </summary>
internal static partial class SaveValues
{
    public static class Screenshot
    {
        private static CacheValue <string> s_lastEditorWindowPath = new() {defaultValue = "Assets"};
        private static CacheValue <string> s_pipelineAsset = new() {defaultValue = ""};
        private static CacheValue <string> s_rendererData = new() {defaultValue = ""};
        private static CacheValue <bool> s_externalExport = new() {defaultValue = false};

        private static CacheValue <bool> s_applyPSShortcutWindow = new() {defaultValue = false};
        private static CacheValue <bool> s_applyPSTransparencyWizard = new() {defaultValue = false};
        private static CacheValue <bool> s_applyPSWindowCapture = new() {defaultValue = false};

        private static SettingsBase s_settings;

        public static string LastEditorWindowPath
        {
            get => ValueProperty.Get("lastEditorWindowPath", ref s_lastEditorWindowPath, _Settings);
            set => ValueProperty.Set("lastEditorWindowPath", value, ref s_lastEditorWindowPath, _Settings);
        }

        public static string RenderPipelineAssetPath
        {
            get => ValueProperty.Get("pipelineAsset", ref s_pipelineAsset, _Settings);
            set => ValueProperty.Set("pipelineAsset", value, ref s_pipelineAsset, _Settings);
        }

        public static string RendererDataPath
        {
            get => ValueProperty.Get("rendererData", ref s_rendererData, _Settings);
            set => ValueProperty.Set("rendererData", value, ref s_rendererData, _Settings);
        }

        public static bool ExternalExport
        {
            get => ValueProperty.Get("externalExport", ref s_externalExport, _Settings);
            set => ValueProperty.Set("externalExport", value, ref s_externalExport, _Settings);
        }

        public static bool ApplyPSShortcutWindow
        {
            get => ValueProperty.Get("applyPS_ShortCutWindow", ref s_applyPSShortcutWindow, _Settings);
            set => ValueProperty.Set("applyPS_ShortCutWindow", value, ref s_applyPSShortcutWindow, _Settings);
        }

        public static bool ApplyPSTransparencyWizard
        {
            get => ValueProperty.Get("applyPS_TransparencyWizard", ref s_applyPSTransparencyWizard, _Settings);
            set => ValueProperty.Set("applyPS_TransparencyWizard", value, ref s_applyPSTransparencyWizard, _Settings);
        }

        public static bool ApplyPSWindowCapture
        {
            get => ValueProperty.Get("applyPS_WindowCapture", ref s_applyPSWindowCapture, _Settings);
            set => ValueProperty.Set("applyPS_WindowCapture", value, ref s_applyPSWindowCapture, _Settings);
        }

        private static SettingsBase _Settings
        {
            get
            {
                if (MegaPintSettings.Exists())
                    return s_settings ??= MegaPintSettings.instance.GetSetting("Screenshot");

                return null;
            }
        }

#if USING_URP
    public static UniversalRenderPipelineAsset RenderPipelineAsset()
    {
        return AssetDatabase.
            LoadAssetAtPath <UniversalRenderPipelineAsset>(RenderPipelineAssetPath);
    }
#endif
    }
}

}
#endif
