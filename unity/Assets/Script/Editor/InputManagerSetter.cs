using UnityEngine;
using UnityEditor;

/** ********************************************************************************
 * @summary InputManagerを自動的に設定してくれるクラス
 ***********************************************************************************/
public class InputManagerSetter
{
    /** ********************************************************************************
     * @summary インプットマネージャーを再設定する
     ***********************************************************************************/
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

    /** ********************************************************************************
     * @summary グローバルな入力設定を追加する（OK、キャンセルなど）
     ***********************************************************************************/
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
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "z", "joystick button 3"));
        }
 
        // キャンセル
        {
            var name = "Cancel";
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "x", "joystick button 2"));
        }
 
        // ポーズ
        {
            var name = "Pause";
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "return", "joystick button 11"));
        }

        // デバッグ用リセット
        {
            var name = "DebugReset";
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, "escape", "joystick button 10"));
        }
    }

    /** ********************************************************************************
     * @summary プレイヤーごとの入力設定を追加する
     ***********************************************************************************/
    private static void AddPlayerInputSettings(InputManagerGenerator inputManagerGenerator, int playerIndex)
    {
        if (playerIndex < 0 || playerIndex > 3) Debug.LogError("プレイヤーインデックスの値が不正です。");
        string upKey = "", downKey = "", leftKey = "", rightKey = "", jumpKey = "", characterKey = "", eraserKey = "", chalkKey = "", changeStateKey = "", actionKey = "";
        GetAxisKey(out upKey, out downKey, out leftKey, out rightKey, out jumpKey, out characterKey, out eraserKey, out chalkKey, out changeStateKey, out actionKey, playerIndex);
 
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
        /*
        {
            var axis = new InputAxis();
            var name = string.Format("Player{0} Chalk", playerIndex);
            var button = string.Format("joystick {0} button 0", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, chalkKey));
        }
        */

        // 黒板消し
        /*
        {
            var axis = new InputAxis();
            var name = string.Format("Player{0} Eraser", playerIndex);
            var button = string.Format("joystick {0} button 1", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, eraserKey));
        }
        */

        // キャラクター
        /*
        {
            var axis = new InputAxis();
            var name = string.Format("Player{0} Character", playerIndex);
            var button = string.Format("joystick {0} button 3", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, characterKey));
        }
        */

        // ステート切り替え
        {
            var name = string.Format("Player{0} ChangeState", playerIndex);
            var button = string.Format("joystick {0} button 5", joystickNum);   // 5: R1
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, changeStateKey));
        }

        // アクション
        {
            var name = string.Format("Player{0} Action", playerIndex);
            var button = string.Format("joystick {0} button 0", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, actionKey));
        }

        // ジャンプ
        {
            var axis = new InputAxis();
            var name = string.Format("Player{0} Jump", playerIndex);
            var button = string.Format("joystick {0} button 2", joystickNum);
            inputManagerGenerator.AddAxis(InputAxis.CreateButton(name, button, jumpKey));
        }
    }

    /** ********************************************************************************
     * @summary キーボードでプレイした場合、割り当たっているキーを取得する
     ***********************************************************************************/
    private static void GetAxisKey(out string upKey, out string downKey, out string leftKey, out string rightKey, out string jumpKey, out string characterKey, out string eraserKey, out string chalkKey, out string changeStateKey, out string actionKey, int playerIndex)
    {
        upKey = "";
        downKey = "";
        leftKey = "";
        rightKey = "";
        jumpKey = "";

        switch (playerIndex)
        {
            case 0:
                upKey = "w";
                downKey = "s";
                leftKey = "a";
                rightKey = "d";
                jumpKey = "f";
                characterKey = "e";
                eraserKey = "q";
                chalkKey = "r";
                changeStateKey = "e";
                actionKey = "r";
                break;
            case 1:
                upKey = "i";
                downKey = "k";
                leftKey = "j";
                rightKey = "l";
                jumpKey = ";";
                characterKey = "o";
                eraserKey = "u";
                chalkKey = "p";
                changeStateKey = "o";
                actionKey = "p";
                break;
            case 2:
                upKey = "g";
                downKey = "b";
                leftKey = "v";
                rightKey = "n";
                jumpKey = "space";
                characterKey = "_";
                eraserKey = "]";
                chalkKey = ":";
                changeStateKey = "h";
                actionKey = "m";
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
                changeStateKey = "c";
                actionKey = "q";
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
                changeStateKey = "";
                actionKey = "";
                break;
        }
    }
}