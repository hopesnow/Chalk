#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
 
/// <summary>
/// InputManagerを自動的に設定してくれるクラス
/// </summary>
public class InputManagerSetter {
 
 
    /// <summary>
    /// インプットマネージャーを再設定します。
    /// </summary>
    [MenuItem("Util/Reset InputManager")]
    public static void ResetInputManager()
    {
        Debug.Log("インプットマネージャーの設定を開始します。");
        InputManagerGenerator inputManagerGenerator = new InputManagerGenerator();
 
        Debug.Log("設定を全てクリアします。");
        inputManagerGenerator.Clear();
 
        Debug.Log("プレイヤーごとの設定を追加します。");
        for(int i = 0; i < 4; i++)
        {
            AddPlayerInputSettings(inputManagerGenerator, i);
        }
        
        Debug.Log("グローバル設定を追加します。");
        AddGlobalInputSettings(inputManagerGenerator);
 
        Debug.Log("インプットマネージャーの設定が完了しました。");
    }
 
    /// <summary>
    /// グローバルな入力設定を追加する（OK、キャンセルなど）
    /// </summary>
    /// <param name="inputManagerGenerator">Input manager generator.</param>
    private static void AddGlobalInputSettings(InputManagerGenerator inputManagerGenerator)
    {
 
        // 横方向
        {
            var name = "Horizontal";
            inputManagerGenerator.AddAxis(InputAxis.CreatePadAxis(name, 0, 1));
            inputManagerGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "a", "d", "left", "right"));
        }
 
        // 縦方向
        {
            var name = "Vertical";
            inputManagerGenerator.AddAxis(InputAxis.CreatePadAxis(name, 0, 2));
            inputManagerGenerator.AddAxis(InputAxis.CreateKeyAxis(name, "s", "w", "down", "up"));
        }
 
        // 決定
        {
            var name = "OK";
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "z", "joystick button 0"));
        }
 
        // キャンセル
        {
            var name = "Cancel";
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "x", "joystick button 1"));
        }
 
        // ポーズ
        {
            var name = "Pause";
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "escape", "joystick button 7"));
        }
    }
 
    /// <summary>
    /// プレイヤーごとの入力設定を追加する
    /// </summary>
    /// <param name="inputManagerGenerator">Input manager generator.</param>
    /// <param name="playerIndex">Player index.</param>
    private static void AddPlayerInputSettings(InputManagerGenerator inputManagerGenerator, int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 3) Debug.LogError("プレイヤーインデックスの値が不正です。");
        string upKey = "", downKey = "", leftKey = "", rightKey = "", jumpKey = "", characterKey = "", eraserKey = "", chalkKey = "";
        GetAxisKey(out upKey, out downKey, out leftKey, out rightKey, out jumpKey, out characterKey, out eraserKey, out chalkKey, playerIndex);
 
        int joystickNum = playerIndex + 1;
        
        // 横方向
        {
            var name = string.Format("Player{0} Horizontal", playerIndex);
            inputManagerGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 1));
            inputManagerGenerator.AddAxis(InputAxis.CreateKeyAxis(name, leftKey, rightKey, "", ""));
        }
 
        // 縦方向
        {
            var name = string.Format("Player{0} Vertical", playerIndex);
            inputManagerGenerator.AddAxis(InputAxis.CreatePadAxis(name, joystickNum, 2));
            inputManagerGenerator.AddAxis(InputAxis.CreateKeyAxis(name, downKey, upKey, "", ""));
        }
 
 
        // チョーク
        {
            var axis = new InputAxis();
            var name = string.Format("Player{0} Chalk", playerIndex);
            var button = string.Format("joystick {0} button 0", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, chalkKey));
        }
        // 黒板消し
        {
            var axis = new InputAxis();
            var name = string.Format("Player{0} Eraser", playerIndex);
            var button = string.Format("joystick {0} button 1", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, eraserKey));
        }
        // キャラクター
        {
            var axis = new InputAxis();
            var name = string.Format("Player{0} Character", playerIndex);
            var button = string.Format("joystick {0} button 2", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, characterKey));
        }
        // ジャンプ
        {
            var axis = new InputAxis();
            var name = string.Format("Player{0} Jump", playerIndex);
            var button = string.Format("joystick {0} button 3", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, jumpKey));
        }
    }
 
    /// <summary>
    /// キーボードでプレイした場合、割り当たっているキーを取得する
    /// </summary>
    /// <param name="upKey">Up key.</param>
    /// <param name="downKey">Down key.</param>
    /// <param name="leftKey">Left key.</param>
    /// <param name="rightKey">Right key.</param>
    /// <param name="jumpKey">Attack key.</param>
    /// <param name="playerIndex">Player index.</param>
    private static void GetAxisKey(out string upKey, out string downKey, out string leftKey, out string rightKey, out string jumpKey, out string characterKey, out string eraserKey, out string chalkKey, int playerIndex)
    {
        upKey = "";
        downKey = "";
        leftKey = "";
        rightKey = "";
        jumpKey = "";
 
        switch(playerIndex)
        {
        case 0:
            upKey = "w";
            downKey = "s";
            leftKey = "a";
            rightKey = "d";
            jumpKey = "x";
            characterKey = "c";
            eraserKey = "e";
            chalkKey = "q";
            break;
        case 1:
            upKey = "i";
            downKey = "k";
            leftKey = "j";
            rightKey = "l";
            jumpKey = ",";
            characterKey = ".";
            eraserKey = "o";
            chalkKey = "u";
            break;
        case 2:
            upKey = "up";
            downKey = "down";
            leftKey = "left";
            rightKey = "right";
            jumpKey = "/";
            characterKey = "_";
            eraserKey = "]";
            chalkKey = ":";
            break;
        case 3:
            upKey = "[8]";
            downKey = "[5]";
            leftKey = "[4]";
            rightKey = "[6]";
            jumpKey = "[9]";
            characterKey = "c";
            eraserKey = "e";
            chalkKey = "q";
            break;
        default:
            Debug.LogError("プレイヤーインデックスの値が不正です。");
            upKey = "";
            downKey = "";
            leftKey = "";
            rightKey = "";
            jumpKey = "";
            characterKey = "";
            eraserKey = "";
            chalkKey = "";
            break;
        }
    }
}
 
#endif