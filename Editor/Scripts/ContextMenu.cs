#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Scripts.Windows;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Scripts
{

internal static partial class ContextMenu
{
    #region Private Methods

    [MenuItem(MenuItemPackages + "/Screenshot/Capture Now", false, 100)]
    private static void CaptureNow()
    {
        List <CameraCapture> cams = Object.FindObjectsOfType <CameraCapture>().ToList();

        if (cams.Count == 0)
            return;

        List <CameraCapture> activeCams = cams.Where(cam => cam.listenToShortcut).ToList();

        if (activeCams is not {Count: > 0})
        {
            EditorUtility.DisplayDialog(
                "Missing Cameras",
                "No CameraCapture components selected for rendering.",
                "Ok");

            return;
        }

        foreach (CameraCapture cam in activeCams)
        {
            var timestamp = $"{DateTime.Now:yy-MM-dd})({DateTime.Now:HH-mm-ss}";
            var camName = $"/{cam.gameObject.name}[{cam.GetHashCode()}]({timestamp}).png";
            var path = $"{cam.lastPath}{camName}";

#if USING_URP
            cam.RenderAndSaveUrp(path, ScreenshotData.RenderPipelineAssetPath,
                AssetDatabase.GUIDFromAssetPath(ScreenshotData.RendererDataPath));
#else
            cam.RenderAndSave(path);
#endif
        }

        Debug.Log($"{activeCams.Count} CameraCapture components rendered.");
    }

    [MenuItem(MenuItemPackages + "/Screenshot/Shortcut Capture", false, 121)]
    private static void OpenShortcutCapture()
    {
        TryOpen <ShortcutCapture>(false);
    }

    [MenuItem(MenuItemPackages + "/Screenshot/Window Capture", false, 120)]
    private static void OpenWindowCapture()
    {
        TryOpen <WindowCapture>(false);
    }

#if USING_URP
    [MenuItem(MenuItemPackages + "/Screenshot/Transparency Wizard", false, 140)]
    private static void TransparencyWizard()
    {
        TryOpen <TransparencyWizard>(true);
    }
#endif

    #endregion
}

}
#endif
