/** ********************************************************************************
 * @file    ClipboardAssetPath.cs
 * @summary アセットのパスをコピーする
 * @author  Ryosuke Hatase
 ***********************************************************************************/
using UnityEditor;

public static class ClipboardAssetPath
{
    // １つ以上選択したオブジェクトが存在する場合
    private static bool IsSelectAsset { get { return Selection.objects != null && 0 < Selection.objects.Length; } }

    /** ********************************************************************************
     * @summary パスをコピーする(最初に選んだ一つだけ)
     ***********************************************************************************/
    [MenuItem("Assets/Copy Path %g", false)]
    private static void CopyAssetName()
    {
        EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(Selection.objects[0]);
    }

    [MenuItem("Assets/Copy Path %g", true)]
    private static bool ValidateName()
    {
        return IsSelectAsset;
    }

    /** ********************************************************************************
     * @summary パスをコピーする(選んだパス全て改行してつなげる)
     ***********************************************************************************/
    [MenuItem("Assets/Copies Path %#g", false)]
    private static void CopyAssetNames()
    {
        string list = string.Empty;
        foreach (var str in Selection.objects)
        {
            list += AssetDatabase.GetAssetPath(str) + "\n";
        }

        EditorGUIUtility.systemCopyBuffer = list;
    }

    [MenuItem("Assets/Copies Path %#g", true)]
    private static bool ValidateNames()
    {
        return IsSelectAsset;
    }
}