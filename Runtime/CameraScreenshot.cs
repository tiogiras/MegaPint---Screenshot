using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScreenshot : MonoBehaviour
{
    [HideInInspector]
    public int width = 1920;
    
    [HideInInspector]
    public int height = 1080;
    
    [HideInInspector]
    public int depth = 100;

    [HideInInspector]
    public BackgroundType backgroundType;

    [HideInInspector]
    public string lastPath;
}