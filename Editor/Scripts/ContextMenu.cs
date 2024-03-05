#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Editor.Scripts.Windows;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{

internal static partial class ContextMenu
{
    #region Private Methods

    [MenuItem(MenuItemPackages + "/Screenshot/Shortcut Capture", false, 100)]
    private static void OpenShortcutCapture()
    {
        TryOpen <ShortcutCapture>(false);
    }
    
    [MenuItem(MenuItemPackages + "/Screenshot/Capture Now", false, 101)]
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
            cam.Capture();
        }
    }

    #endregion
}

}
#endif
