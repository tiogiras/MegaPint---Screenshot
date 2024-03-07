#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts
{

[CustomEditor(typeof(CameraCapture))]
internal class CameraCaptureDrawer : UnityEditor.Editor
{
    private const string Path = "Screenshot/User Interface/CameraCapture";
    private ColorField _backgroundColor;
    private ObjectField _backgroundImage;

    private EnumField _backgroundType;
    private Button _btnRender;
    private Button _btnSave;
    
    private Button _btnExportPath;
    private Label _path;
    
    private DropdownField _depth;
    private IntegerField _height;
    private Label _imageResolution;

    private DropdownField _imageType;
    private FloatField _pixelPerUnit;
    private AspectRatioPanel _preview;

    private Texture2D _render;

    private CameraCapture _target;

    private IntegerField _width;

    #region Public Methods

    public override VisualElement CreateInspectorGUI()
    {
        var template = Resources.Load <VisualTreeAsset>(Path);
        TemplateContainer root = template.Instantiate();

        _preview = root.Q <AspectRatioPanel>("Preview");
        _btnRender = root.Q <Button>("BTN_Render");
        _btnSave = root.Q <Button>("BTN_Save");
        
        _btnExportPath = root.Q <Button>("BTN_ExportPath");
        _path = root.Q <Label>("Path");

        _width = root.Q <IntegerField>("Width");
        _height = root.Q <IntegerField>("Height");
        _depth = root.Q <DropdownField>("Depth");

        _backgroundType = root.Q <EnumField>("BackgroundType");
        _backgroundColor = root.Q <ColorField>("BackgroundColor");
        _backgroundImage = root.Q <ObjectField>("BackgroundImage");
        _imageResolution = root.Q <Label>("ImageResolution");

        _imageType = root.Q <DropdownField>("ImageType");
        _pixelPerUnit = root.Q <FloatField>("PixelPerUnit");

        _target = (CameraCapture)target;

        _width.value = _target.width;
        _height.value = _target.height;
        _depth.value = _target.depth.ToString();
        _backgroundType.value = _target.backgroundType;
        _backgroundColor.value = _target.backgroundColor;
        _backgroundImage.value = _target.backgroundImage;
        _imageType.value = _target.imageType;
        _pixelPerUnit.value = _target.pixelPerUnit;

        _btnSave.style.display = DisplayStyle.None;

        UpdatePath();
        UpdateBackgroundColor();
        UpdateBackgroundImage();

        RegisterCallbacks();

        return root;
    }

    private void UpdatePath()
    {
        _path.text = _target.lastPath;
        _path.tooltip = _target.lastPath;
    }

    #endregion

    #region Private Methods

    private void RegisterCallbacks()
    {
        _btnRender.clicked += Render;
        _btnSave.clickable = new Clickable(() =>
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Screenshot", "", "png", "", _target.lastPath);
            _target.Save(_render, path);
        });

        _width.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                    _width.SetValueWithoutNotify(0);

                _target.width = evt.newValue;
            });

        _height.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                    _height.SetValueWithoutNotify(0);

                _target.height = evt.newValue;
            });

        _depth.RegisterValueChangedCallback(evt => {_target.depth = int.Parse(evt.newValue);});

        _backgroundType.RegisterValueChangedCallback(
            evt =>
            {
                _target.backgroundType = (BackgroundType)evt.newValue;
                UpdateBackgroundColor();
                UpdateBackgroundImage();
            });

        _backgroundColor.RegisterValueChangedCallback(evt => {_target.backgroundColor = evt.newValue;});

        _imageType.RegisterValueChangedCallback(
            evt =>
            {
                _target.imageType = evt.newValue;
                UpdateBackgroundImage();
            });

        _pixelPerUnit.RegisterValueChangedCallback(
            evt =>
            {
                if (evt.newValue < 0)
                    _pixelPerUnit.SetValueWithoutNotify(0);

                _target.pixelPerUnit = evt.newValue;
            });

        _backgroundImage.RegisterValueChangedCallback(
            evt =>
            {
                _target.backgroundImage = (Sprite)evt.newValue;
                UpdateBackgroundImage();
            });

        _btnExportPath.clicked += ChangePath;
    }

    private void ChangePath()
    {
        var path = EditorUtility.OpenFolderPanel("Choose Path", _target.lastPath, "");

        if (!path.StartsWith(Application.dataPath))
        {
            EditorUtility.DisplayDialog("Folder not in project", "The folder must be within the Assets folder", "ok");
            return;   
        }

        _target.lastPath = path.Remove(0, Application.dataPath.Length - 6);
        UpdatePath();
    }

    private void Render()
    {
        var width = _target.width;
        var height = _target.height;

        var gcd = Utility.Gcd((ulong)width, (ulong)height);

        _render = _target.Render();

        _preview.style.backgroundImage = _render;
        _preview.aspectRatioX = width / gcd;
        _preview.aspectRatioY = height / gcd;
        _preview.FitToParent();

        _btnSave.style.display = DisplayStyle.Flex;
    }

    private void UpdateBackgroundColor()
    {
        _backgroundColor.style.display = _target.backgroundType == BackgroundType.SolidColor ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void UpdateBackgroundImage()
    {
        var isImage = _target.backgroundType == BackgroundType.Image;
        _backgroundImage.style.display = isImage ? DisplayStyle.Flex : DisplayStyle.None;
        _imageType.style.display = isImage ? DisplayStyle.Flex : DisplayStyle.None;

        Sprite image = _target.backgroundImage;
        _imageResolution.style.display = isImage && image != null ? DisplayStyle.Flex : DisplayStyle.None;
        _pixelPerUnit.style.display = isImage && _target.imageType.Equals("Tiled") ? DisplayStyle.Flex : DisplayStyle.None;

        _imageResolution.text = _target.backgroundImage == null ? "" : $"{image.rect.width} x {image.rect.height}";
    }

    #endregion
}

}
#endif
