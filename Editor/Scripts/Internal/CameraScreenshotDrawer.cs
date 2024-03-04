#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Scripts.Internal
{

[CustomEditor(typeof(CameraScreenshot))]
internal class CameraScreenshotDrawer : UnityEditor.Editor
{
    private const string Path = "Screenshot/User Interface/CameraScreenshot";

    private Camera _camera;
    private AspectRatioPanel _preview;
    private Button _btnRender;
    private Button _btnSave;

    private IntegerField _width;
    private IntegerField _height;
    private DropdownField _depth;

    private EnumField _backgroundType;
    private ColorField _backgroundColor;

    private CameraScreenshot _target;

    private Texture2D _render;

    #region Public Methods

    public override VisualElement CreateInspectorGUI()
    {
        var template = Resources.Load <VisualTreeAsset>(Path);
        TemplateContainer root = template.Instantiate();

        _preview = root.Q <AspectRatioPanel>("Preview");
        _btnRender = root.Q <Button>("BTN_Render");
        _btnSave = root.Q <Button>("BTN_Save");

        _width = root.Q <IntegerField>("Width");
        _height = root.Q <IntegerField>("Height");
        _depth = root.Q <DropdownField>("Depth");

        _backgroundType = root.Q <EnumField>("BackgroundType");
        _backgroundColor = root.Q <ColorField>("BackgroundColor");

        _target = (CameraScreenshot)target;
        
        _camera = _target.gameObject.GetComponent <Camera>();

        _width.value = _target.width;
        _height.value = _target.height;
        _depth.value = _target.depth.ToString();
        _backgroundType.value = _target.backgroundType;
        _backgroundColor.value = _target.backgroundColor;

        _btnSave.style.display = DisplayStyle.None;

        UpdateBackgroundColor();
        
        RegisterCallbacks();

        return root;
    }

    private void UpdateBackgroundColor()
    {
        _backgroundColor.style.display = _target.backgroundType == BackgroundType.SolidColor ? DisplayStyle.Flex : DisplayStyle.None;
    }

    #endregion

    private void RegisterCallbacks()
    {
        _btnRender.clicked += Render;
        _btnSave.clicked += Save;
        
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

        _backgroundType.RegisterValueChangedCallback(evt =>
        {
            _target.backgroundType = (BackgroundType)evt.newValue;
            UpdateBackgroundColor();
        });

        _backgroundColor.RegisterValueChangedCallback(evt => {_target.backgroundColor = evt.newValue;});
    }

    private static void SaveTexture(Texture2D texture, string filePath) {
        var bytes = texture.EncodeToPNG();
        var stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
        var writer = new BinaryWriter(stream);
        
        foreach (var t in bytes)
        {
            writer.Write(t);
        }
        
        writer.Close();
        stream.Close();
        
        AssetDatabase.Refresh();
    }

    private void Save()
    {
        var path = EditorUtility.SaveFilePanelInProject("Save Screenshot", "", "png", "", _target.lastPath);
        
        if (string.IsNullOrEmpty(path))
            return;
        
        _target.lastPath = path[..path.LastIndexOf("/", StringComparison.Ordinal)];
        SaveTexture(_render, path);
    }

    private void Render()
    {
        PrepareCamera(out Color backgroundColor, out CameraClearFlags flags);

        var width = _target.width;
        var height = _target.height;

        var gcd = Utility.Gcd((ulong)width, (ulong)height);

        _render = Utility.RenderCamera(_camera, width, height, _target.depth);
        _preview.style.backgroundImage = _render;
        _preview.aspectRatioX = width / gcd;
        _preview.aspectRatioY = height / gcd;
        _preview.FitToParent();

        _btnSave.style.display = DisplayStyle.Flex;

        ResetCamera(backgroundColor, flags);
    }

    private void PrepareCamera(out Color backgroundColor, out CameraClearFlags flags)
    {
        backgroundColor = _camera.backgroundColor;
        flags = _camera.clearFlags;

        switch (_target.backgroundType)
        {
            case BackgroundType.None:
                _camera.clearFlags = CameraClearFlags.Skybox;
                break;
            
            case BackgroundType.SolidColor:
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.backgroundColor = _target.backgroundColor;
                break;
            
            case BackgroundType.Transparent: 
                _camera.clearFlags = CameraClearFlags.Depth;
                break;
            
            case BackgroundType.Image: 
                _camera.clearFlags = CameraClearFlags.Depth;
                break;
            
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void ResetCamera(Color backgroundColor, CameraClearFlags flags)
    {
        _camera.backgroundColor = backgroundColor;
        _camera.clearFlags = flags;
    }
}

}
#endif
