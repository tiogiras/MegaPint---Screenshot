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

    // [MenuItem("Assets/LogGUID")]
    // private static void LogGuid()
    // {
    //     Object[] objs = Selection.objects;
    //
    //     foreach (Object o in objs)
    //     {
    //         Debug.Log($"{o.name}: [{AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(o))}]");
    //     }
    // }

#if USING_URP
    [MenuItem(MenuItemPackages + "/Screenshot/Transparency Wizard", false, 140)]
    private static void TransparencyWizard()
    {
        TryOpen <TransparencyWizard>(true);
    }
#endif

    [MenuItem(MenuItemPackages + "/Screenshot/Capture Now", false, 100)]
    private static void CaptureNow()
    {
        List <CameraCapture> cams = Object.FindObjectsOfType <CameraCapture>().ToList();

        if (cams.Count == 0)
            return;

        List <CameraCapture> activeCams = cams.Where(cam => cam.listenToShortcut).ToList();

        if (activeCams is not {Count: > 0})
        {
            EditorUtility.DisplayDialog("Missing Cameras", "No CameraCapture components selected for rendering.", "Ok");

            return;
        }

        foreach (CameraCapture cam in activeCams)
        {
            var timestamp = $"{DateTime.Now:yy-MM-dd})({DateTime.Now:HH-mm-ss}";
            var camName = $"/{cam.gameObject.name}[{cam.GetHashCode()}]({timestamp}).png";
            var path = $"{cam.lastPath}{camName}";
            cam.RenderAndSave(path);
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

    [MenuItem("Window/MegaPint/Test", false, 0)]
    private static void Test()
    {
        Debug.Log("EXECUTE TEST");
    }

    #endregion
}

}
#endif
