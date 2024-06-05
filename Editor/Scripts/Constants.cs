#if UNITY_EDITOR
using System.IO;

namespace MegaPint.Editor.Scripts
{

/// <summary> Partial lookup table for constants containing screenshot values  </summary>
internal static partial class Constants
{
    public static class Screenshot
    {
        public static class Links
        {
            public const string WindowCapture = "MegaPint/Packages/Screenshot/Window Capture";
            public const string ShortcutCapture = "MegaPint/Packages/Screenshot/Shortcut Capture";
            public const string CaptureNow = "MegaPint/Packages/Screenshot/Capture Now";
        }

        public static class UserInterface
        {
            private static readonly string s_windows = Path.Combine(s_userInterface, "Windows");
            public static readonly string ShortcutCapture = Path.Combine(s_windows, "Shortcut Capture");
            public static readonly string ShortcutCaptureItem = Path.Combine(ShortcutCapture, "Item");

            public static readonly string TransparencyWizard = Path.Combine(s_windows, "Transparency Wizard");
            public static readonly string WindowCapture = Path.Combine(s_windows, "Window Capture");
            public static readonly string CameraCapture = Path.Combine(s_windows, "Camera Capture");
            public static readonly string MultiplePipelines = Path.Combine(s_windows, "Multiple Pipelines");
        }

        private static readonly string s_base = Path.Combine("MegaPint", "Screenshot");
        private static readonly string s_userInterface = Path.Combine(s_base, "User Interface");
    }
}

}
#endif
