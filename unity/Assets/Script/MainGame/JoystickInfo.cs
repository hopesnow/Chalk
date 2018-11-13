using UnityEditor;
using UnityEngine;

public class JoystickInfo : ScriptableObject
{
    public const string AssetPath = "Assets/Data/Joystick/JoystickInfo.asset";

    public string jumpButton;
    public string horizontalButton;
    public string verticalButton;

    [MenuItem("Joystick/Create JoystickInfo")]
    static void CreateInfo()
    {
        var asset = CreateInstance<JoystickInfo>();

        AssetDatabase.CreateAsset(asset, AssetPath);
        AssetDatabase.Refresh();
    }
}
