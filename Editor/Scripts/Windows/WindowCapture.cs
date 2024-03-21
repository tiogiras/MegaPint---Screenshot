#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts.Windows
{

/// <summary>
///     Window based on the <see cref="MegaPintEditorWindowBase" /> to display and handle the rendering of editor windows
/// </summary>
internal class WindowCapture : MegaPintEditorWindowBase
{
    private const string FolderBasePath = "Screenshot/User Interface/";

    private readonly List <EditorWindow> _windowRefs = new();

    private VisualTreeAsset _baseWindow;

    private Button _btnRefresh;
    private Button _btnRender;
    private Button _btnSave;

    private AspectRatioPanel _preview;

    private Texture2D _render;

    private DropdownField _windows;
    #region Public Methods

    /// <summary> Show the window </summary>
    /// <returns> Window instance </returns>
    public override MegaPintEditorWindowBase ShowWindow()
    {
        titleContent.text = "Window Capture";

        return this;
    }

    #endregion
    #region Protected Methods

    protected override string BasePath()
    {
        return FolderBasePath + "WindowCapture";
    }

    protected override async void CreateGUI()
    {
        base.CreateGUI();

        VisualElement root = rootVisualElement;

        VisualElement content = _baseWindow.Instantiate();
        content.style.flexGrow = 1;
        content.style.flexShrink = 1;

        _preview = content.Q <AspectRatioPanel>("Preview");
        _windows = content.Q <DropdownField>("Windows");

        _btnRefresh = content.Q <Button>("BTN_Refresh");
        _btnRender = content.Q <Button>("BTN_Render");
        _btnSave = content.Q <Button>("BTN_Save");

        _btnRender.style.display = DisplayStyle.None;
        _btnSave.style.display = DisplayStyle.None;

        RegisterCallbacks();

        root.Add(content);

        await Task.Delay(100);

        RefreshWindows();
    }

    protected override bool LoadResources()
    {
        _baseWindow = Resources.Load <VisualTreeAsset>(BasePath());

        return _baseWindow != null;
    }

    protected override void RegisterCallbacks()
    {
        _btnRefresh.clicked += RefreshWindows;
        _btnRender.clicked += Render;
        _btnSave.clicked += Save;

        _windows.RegisterValueChangedCallback(WindowSelected);
    }

    protected override void UnRegisterCallbacks()
    {
        _btnRefresh.clicked -= RefreshWindows;
        _btnRender.clicked -= Render;
        _btnSave.clicked -= Save;

        _windows.UnregisterValueChangedCallback(WindowSelected);
    }

    #endregion
    #region Private Methods

    private void RefreshWindows()
    {
        EditorWindow[] windows = Resources.FindObjectsOfTypeAll <EditorWindow>();

        _windowRefs.Clear();
        _windows.choices.Clear();

        foreach (EditorWindow window in windows)
        {
            _windowRefs.Add(window);
            _windows.choices.Add(window.titleContent.ToString());
        }

        _windows.SetValueWithoutNotify("");

        _btnRender.style.display = DisplayStyle.None;
        _btnSave.style.display = DisplayStyle.None;
        _preview.style.backgroundImage = null;
    }

    private async void Render()
    {
        EditorWindow target = _windowRefs[_windows.index];

        if (target == null)
            return;

        if (!target.hasFocus)
        {
            target.Focus();
            await Task.Delay(100);
        }

        Vector2 pos = target.position.position;
        var width = target.position.width;
        var height = target.position.height;

        Color[] colors = InternalEditorUtility.ReadScreenPixel(pos, (int)width, (int)height);

        var result = new Texture2D((int)width, (int)height);
        result.SetPixels(colors);
        result.Apply();

        var gcd = ScreenshotUtility.Gcd((ulong)width, (ulong)height);

        _render = result;

        _preview.style.backgroundImage = _render;
        _preview.aspectRatioX = (int)width / gcd;
        _preview.aspectRatioY = (int)height / gcd;
        _preview.FitToParent();

        _btnSave.style.display = DisplayStyle.Flex;
    }

    private void Save()
    {
        var path = EditorUtility.SaveFilePanel(
            "Save Screenshot",
            ScreenshotData.LastEditorWindowPath,
            "",
            "png");

        if (string.IsNullOrEmpty(path))
            return;

        if (!path.IsPathInProject(out var _) && !ScreenshotData.ExternalExport)
        {
            EditorUtility.DisplayDialog(
                "Path not in project",
                "The path must be within the Assets folder",
                "ok");
            
            return;
        }

        ScreenshotData.LastEditorWindowPath = path;
        ScreenshotUtility.SaveTexture(_render, path);
    }

    private void WindowSelected(ChangeEvent <string> evt)
    {
        _btnRender.style.display = DisplayStyle.Flex;
    }

    #endregion
}

}
#endif
