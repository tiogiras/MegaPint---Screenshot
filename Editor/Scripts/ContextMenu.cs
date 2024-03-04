#if UNITY_EDITOR
using Editor.Scripts.Windows;
using UnityEditor;

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
        
    }

    #endregion
}

}
#endif
