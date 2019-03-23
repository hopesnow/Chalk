using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private CharacterController chara;     // キャラクター制御クラス
    [SerializeField] private DrawPhysicsLine line;          // 書く線の制御クラス
    [SerializeField] private SpriteRenderer charaSprite;    // キャラクターの画像
    [SerializeField] private Animator charaAnimator;        // キャラクターのアニメーション制御
    [SerializeField] private SpriteRenderer chalkSprite;   // プレイヤーごとのチョーク
    [SerializeField] private SpriteRenderer eraserSprite;   // プレイヤーごとの黒板消し

    [SerializeField] private Color[] charaColors = new Color[4];
    [SerializeField] private Sprite[] charaSprites = new Sprite[4];
    [SerializeField] private RuntimeAnimatorController[] charaAnimators = new RuntimeAnimatorController[4];
    [SerializeField] private Sprite[] eraserSprites = new Sprite[4];

    public CharacterController Characer { get { return this.chara; } }

    /** ********************************************************************************
     * @summary 初期化処理
     ***********************************************************************************/
    public void Init(int playerNo, int charaNo, float bottomOffset)
    {
        // プレイヤーNoごとの色付け
        var color = this.charaColors[playerNo];
        this.chara.Init(playerNo, color);
        this.line.SetColor(color);
        this.charaSprite.color = color;
        this.chalkSprite.color = color;
        this.eraserSprite.sprite = this.eraserSprites[playerNo];

        // キャラクターNoごとのリソース差し替え
        this.ChangeChara(charaNo);

        // その他設定
        this.chara.SetBottomOffset(bottomOffset);
    }

    /** ********************************************************************************
     * @summary キャラクタのみ変更処理
     ***********************************************************************************/
    public void ChangeChara(int charaNo)
    {
        this.charaSprite.sprite = this.charaSprites[charaNo];
        this.charaAnimator.runtimeAnimatorController = this.charaAnimators[charaNo];
    }
}
