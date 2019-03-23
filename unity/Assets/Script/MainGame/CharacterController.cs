#define UNABLE_DOUBLE_JUMP

using DG.Tweening;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class CharacterController : MonoBehaviour
{
    private enum InputState
    {
        Character,
        Chalk,
        Eraser,
        None,
    }

    private enum CharacterState
    {
        Normal,     // 通常状態
        Damaged,    // ダメージ中
        Invincible, // 無敵中
        Goal,       // ゴールした
    }

    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float jumpPower = 500f;
    [SerializeField] private float chalkSpeed = 0.1f;
    [SerializeField] private float chalkDrawSpeePower = 0.25f;   // チョーク書き込み中の移動速度倍率
    [SerializeField] private Vector2 backwardForce = new Vector2(-4.5f, 5.4f);
    [SerializeField] private SpriteRenderer characterSprite;

    // 地面オブジェクトはどのLayerか
    [SerializeField] private LayerMask whatIsGround;

    // チョークオブジェクト
    [SerializeField] private Transform chalk;
    [SerializeField] private DrawPhysicsLine drawLine;
    [SerializeField] private Transform chalkMask;

    // 黒板消しオブジェクト
    [SerializeField] private ErasePhysicsLine eraser;
    [SerializeField] private Animator eraserAnim;

    [SerializeField] private int playerNo;

    private Animator mAnimator;
    private BoxCollider2D mBoxcollier2D;
    private Rigidbody2D mRigidbody2D;
    private bool mIsGround;
    private bool mIsVerticalNeutral = false;    // スティックをニュートラルに戻してるか
    private const float StickDownPower = 0.9f;
    private const float mCenterY = 1.5f;

    private InputState inputState = InputState.None;       // 入力したときの状態
    private CharacterState charaState = CharacterState.Normal;  // キャラクターの動作に関わる状態
    private bool canJump2nd = true;                             // 2段ジャンプ可能か

    private Vector3 initPos = Vector3.zero; // リセット時の初期座標

    // チョーク情報
    private float chalkAmount = 0f;                 // 残量
    private bool canDrawing = false;                // 書き直し用フラグ
    private const float LimitChalkAmount = 200f;    // チョーク量の上限
    private const float ChargeChalkAmount = 2f;   // チョークの補充量
    private const float UseChalkAmount = 1f;        // チョークの使用量
    private const float MinimumChalkAmount = 0f;   // 最低限必要なチョーク量
    private bool isAvailable = true;

    // 画面内判定用
    private float screenHeight = 3.6f;
    private float screenWidth = 6.4f;
    private float bottomOffset = 0;

    // 当たり判定変更用
    private Vector2 collOffset;
    private Vector2 collSize;

    // ゴールしたフラグ
    public ReactiveProperty<bool> IsGoal = new ReactiveProperty<bool>();

    public int PlayerNo { get { return this.playerNo; } set { this.playerNo = value; } }
    public Animator GetAnimator { get { return this.mAnimator; } }

    /** ********************************************************************************
     * @summary 初期化処理
     ***********************************************************************************/
    private void Awake()
    {
        this.mAnimator = GetComponent<Animator>();
        this.mBoxcollier2D = GetComponent<BoxCollider2D>();
        this.mRigidbody2D = GetComponent<Rigidbody2D>();

        this.initPos = this.transform.localPosition;
        IsGoal.Value = false;

        this.chalkAmount = LimitChalkAmount;  // チョーク量初期値に

        ChangeState(InputState.Character);

        // 当たり判定の保存
        this.collOffset = this.mBoxcollier2D.offset;
        this.collSize = this.mBoxcollier2D.size;

        // 画面端の計算
        screenHeight = Camera.main.orthographicSize;
        screenWidth = screenHeight / Screen.height * Screen.width;
    }

    /** ********************************************************************************
     * @summary 初期設定
     ***********************************************************************************/
    public void Init(int no, Color color)
    {
        this.playerNo = no;
        this.characterSprite.color = color;
    }

    /** ********************************************************************************
     * @summary 初期座標設定(座標はlocalを使わない)
     ***********************************************************************************/
    public void SetInitPos(Vector3 pos)
    {
        this.transform.position = pos;
        this.initPos = this.transform.localPosition;
    }

    /** ********************************************************************************
     * @summary オフセット設定
     ***********************************************************************************/
    public void SetBottomOffset(float offset)
    {
        this.bottomOffset = offset;
    }

    /** ********************************************************************************
     * @summary リセット処理
     ***********************************************************************************/
    public void Reset()
    {
        // 座標の初期化
        this.transform.localPosition = this.initPos;
        this.canJump2nd = true;

        // 書いたオブジェクトの初期化
        foreach (Transform line in this.drawLine.transform)
        {
            Destroy(line.gameObject);
        }    

        // 加速度の初期化
        mRigidbody2D.velocity = Vector2.zero;

        // Animator
        this.mAnimator.applyRootMotion = false;
        this.mAnimator.Play("Idle");

        // ゴールフラグの初期化
        IsGoal.Value = false;
        this.charaState = CharacterState.Normal;

        // 入力方法の初期化
        ChangeState(InputState.Character);
    }

    /** ********************************************************************************
     * @summary 更新処理
     ***********************************************************************************/
    private void Update()
    {
        float moveVec = 0;
        bool jump = false;  // ジャンプ
        bool squat = false; // しゃがみ

        // 書いてる最中は回復しないように
        if (!this.canDrawing)
        {
            // チョーク残量回復処理
            this.chalkAmount += ChargeChalkAmount;
            if (this.chalkAmount > LimitChalkAmount)
            {
                this.chalkAmount = LimitChalkAmount;
            }
        }

        // スティックの状態更新
        if (Mathf.Abs(Input.GetAxis(string.Format("Player{0} Vertical", playerNo))) <= 0.3f)
        {
            this.mIsVerticalNeutral = true;
        }

        // 操作不能のとき
        if (!this.isAvailable)
        {
            return;
        }

        // 操作切り替え
        if (Input.GetButtonDown(string.Format("Player{0} ChangeState", this.playerNo)))
        {
            bool changed = false;
            // 順繰りになるように
            switch (this.inputState)
            {
                case InputState.Character:
                    changed = ChangeState(InputState.Chalk);
                    break;

                case InputState.Chalk:
                    // 書き途中の線を引き切る
                    if (this.canDrawing)
                    {
                        this.canDrawing = false;
                        drawLine.CheckLines();
                    }
                    
                    // changed = ChangeState(InputState.Eraser);
                    changed = ChangeState(InputState.Character);
                    break;

                case InputState.Eraser:
                    // 選択状態を解除する
                    this.eraser.ResetSelectAll();

                    changed = ChangeState(InputState.Character);
                    break;
                // 念の為
                default:
                    changed = ChangeState(InputState.Character);
                    break;
            }

            // 切り替わったとき
            if (changed)
            {
                return;
            }
        }

        // 黒板消しはワンボタンで変更できるように
        if (Input.GetButtonDown(string.Format("Player{0} Eraser", this.playerNo)))
        {
            bool changed = false;
            changed = ChangeState(InputState.Eraser);
            if (changed)
            {
                return;
            }
        }

        // 操作状態
        switch (this.inputState)
        {
            /****************************************************************************************************/
            // キャラクターの操作
            case InputState.Character:

                // ダメージを受けていないかどうか, ゴールしていないかどうか
                if (charaState == CharacterState.Normal && !IsGoal.Value)
                {
                    moveVec = Input.GetAxis(string.Format("Player{0} Horizontal", playerNo));
                    jump = Input.GetButtonDown(string.Format("Player{0} Jump", playerNo));

                    // 下スティックでしゃがみ処理
                    if (Input.GetAxis(string.Format("Player{0} Vertical", playerNo)) <= -StickDownPower)
                    {
                        squat = true;
                    }

                    // 上スティックでもジャンプする処理
                    if (this.mIsVerticalNeutral && Input.GetAxis(string.Format("Player{0} Vertical", playerNo)) >= StickDownPower)
                    {
                        this.mIsVerticalNeutral = false;
                        jump = true;
                    }
                }

                break;

            /****************************************************************************************************/
            // チョークの操作
            case InputState.Chalk:
                if (Input.GetButtonDown(string.Format("Player{0} Action", playerNo)))
                {
                    // 線のひきはじめ処理
                    this.drawLine.SetStartPos(this.chalk.localPosition);
                    this.canDrawing = true;
                }
                else if(Input.GetButtonUp(string.Format("Player{0} Action", playerNo)))
                {
                    // 線のひきおわり処理
                    this.canDrawing = false;
                    this.drawLine.CheckLines();
                }

                var calcChalk = CalculateToolMove();

                // 変化がなければ行わない処理
                bool isDrawing = false;
                if (calcChalk.x != 0f || calcChalk.y != 0f)
                {
                    isDrawing = Input.GetButton(string.Format("Player{0} Action", playerNo));

                    // 座標移動
                    float power = isDrawing ? this.chalkDrawSpeePower : 1.0f;   // 書いてるときは移動速度倍率をかける

                    this.chalk.localPosition = CalculateScreenEnd(this.chalk.localPosition + new Vector3(calcChalk.x * chalkSpeed * power, calcChalk.y * chalkSpeed * power));
                    this.eraser.transform.localPosition = this.chalk.localPosition;

                    // 線をひくか、完了するかのチェック
                    if (isDrawing && this.chalkAmount >= MinimumChalkAmount && canDrawing)
                    {
                        // 線を引く
                        this.chalkAmount -= UseChalkAmount;
                        this.drawLine.DragLine(this.chalk.localPosition);
                    }
                    else
                    {
                        // 燃料切れだったりしたら再度ボタンを押し直すまでかけないようにする
                        this.canDrawing = false;
                        this.drawLine.CheckLines();
                    }
                }

                break;

            /****************************************************************************************************/
            // 黒板消しの操作
            case InputState.Eraser:
                var calcEraser = CalculateToolMove();

                // 変化がなければ行わない
                if (calcEraser.x != 0f || calcEraser.y != 0f)
                {
                    this.eraser.transform.localPosition = CalculateScreenEnd(this.eraser.transform.localPosition + new Vector3(calcEraser.x * chalkSpeed, calcEraser.y * chalkSpeed));
                    this.chalk.localPosition = this.eraser.transform.localPosition;
                }

                // アクションが押されたかチェック
                if (Input.GetButtonDown(string.Format("Player{0} Action", playerNo)))
                {
                    // 消すアニメーション可能かチェック
                    if (this.eraser.IsErasable)
                    {
                        this.isAvailable = false;

                        // オブジェクトを消す処理
                        this.eraser.DeleteLine();
                        this.eraserAnim.PlayAsObservable("Delete").Subscribe(_ =>
                        {
                            // アニメーション終了時の処理
                            this.isAvailable = true;
                        });
                    }
                }

                break;

            default:

                break;

                /****************************************************************************************************/
        }

        // 移動系処理
        Move(moveVec, jump, squat);

        // 表示の更新
        this.chalkMask.localScale = new Vector3(1f, Mathf.Clamp(this.chalkAmount / LimitChalkAmount, 0f, 1f), 1f);
    }

    /** ********************************************************************************
     * @summary チョークなどの移動処理
     ***********************************************************************************/
    private Vector2 CalculateToolMove()
    {
        // 移動具合をみる
        float orgX = Input.GetAxis(string.Format("Player{0} Horizontal", playerNo));  // 入力値 1 〜 0
        float orgY = Input.GetAxis(string.Format("Player{0} Vertical", playerNo));    // 入力値 1 〜 0

        // 両方共数値が入っていればmagnitudeが1になるように計算する
        float calcX = 0;
        float calcY = 0;
        if (Mathf.Abs(orgX) > 0 && Mathf.Abs(orgY) > 0)
        {
            float tmp = Mathf.Sqrt(orgX * orgX + orgY * orgY);
            calcX = orgX / tmp;
            calcY = orgY / tmp;
        }
        else
        {
            calcX = orgX;
            calcY = orgY;
        }

        // 小さい数字は丸める
        if (Mathf.Abs(calcX) < 0.02f)
        {
            calcX = 0;
        }

        if (Mathf.Abs(calcY) < 0.02f)
        {
            calcY = 0;
        }

        return new Vector2(calcX, calcY);
    }

    /** ********************************************************************************
     * @summary 画面端計算
     ***********************************************************************************/
    private Vector3 CalculateScreenEnd(Vector3 pos)
    {
        var x = pos.x;
        var y = pos.y;
        var z = pos.z;

        // 右端判定
        if (x > screenWidth)
            x = screenWidth;
        // 左端判定
        if (x < -screenWidth)
            x = -screenWidth;
        // 上端判定
        if (y > screenHeight)
            y = screenHeight;
        // 下端判定 (画面端の書けない部分を考慮)
        if (y < -screenHeight + this.bottomOffset)
            y = -screenHeight + this.bottomOffset;

        return new Vector3(x, y, z);
    }

    /** ********************************************************************************
     * @summary 移動処理
     ***********************************************************************************/
    private void Move(float move, bool jump, bool squat)
    {
#if UNABLE_DOUBLE_JUMP
        this.canJump2nd = false;
#endif

        if (Mathf.Abs(move) > 0)
        {
            Quaternion rot = transform.rotation;
            transform.rotation = Quaternion.Euler(rot.x, Mathf.Sign(move) == 1 ? 0 : 180, rot.z);
        }

        mRigidbody2D.velocity = new Vector2(move * maxSpeed, mRigidbody2D.velocity.y);

        // ゴールしたときは処理しない
        if (!IsGoal.Value)
        {
            this.mAnimator.SetBool("isGround", mIsGround);
            this.mAnimator.SetFloat("Horizontal", move);
        }

        this.mAnimator.SetFloat("Vertical", mRigidbody2D.velocity.y);
        this.mAnimator.SetBool("isSquat", squat);

        // しゃがみ中の当たり判定の切り替え
        if (this.mIsGround && squat)
        {
            this.mBoxcollier2D.offset = new Vector2(this.collOffset.x, this.collOffset.y - this.collSize.y / 4);
            this.mBoxcollier2D.size = new Vector2(this.collSize.x, this.collSize.y / 2);
        }
        else
        {
            this.mBoxcollier2D.offset = this.collOffset;
            this.mBoxcollier2D.size = this.collSize;
        }

        // ジャンプ可能か
        if (jump && (mIsGround || canJump2nd) && !squat)
        {
            mAnimator.SetTrigger("Jump");
            // SendMessage("Jump", SendMessageOptions.DontRequireReceiver);

            mRigidbody2D.velocity = new Vector2(mRigidbody2D.velocity.x, 0);    // 落下速度リセット
            mRigidbody2D.AddForce(Vector2.up * jumpPower);
            if (!mIsGround)
            {
                this.canJump2nd = false;
            }
        }
    }

    /** ********************************************************************************
     * @summary 更新処理
     ***********************************************************************************/
    private void FixedUpdate()
    {
        Vector2 pos = transform.position;
        Vector2 groundCheck = new Vector2(pos.x, pos.y - (mCenterY * transform.localScale.y));
        Vector2 groundArea = new Vector2(mBoxcollier2D.size.x * 0.49f, 0.05f);

        mIsGround = Physics2D.OverlapArea(groundCheck + groundArea, groundCheck - groundArea, whatIsGround);
        mAnimator.SetBool("isGround", mIsGround);

        // 2段ジャンプ可能になったかどうか
        if (!canJump2nd && mIsGround)
        {
            canJump2nd = true;
        }
    }

    /** ********************************************************************************
     * @summary 当たり判定処理(その場所にいるかどうか)
     ***********************************************************************************/
    private void OnTriggerStay2D(Collider2D other)
    {
        // あたってるオブジェクトがDamageObjectで通常ステートのときのみ当たり判定処理をおこなう
        if (other.tag == "DamageObject" && charaState == CharacterState.Normal)
        {
            charaState = CharacterState.Damaged;
            StartCoroutine(INTERNAL_OnDamage());
        }
        else if (other.tag == "Goal" && this.charaState != CharacterState.Goal)
        {
            // TODO: ゴール処理
            IsGoal.Value = true;
            this.charaState = CharacterState.Goal;
        }
    }

    /** ********************************************************************************
     * @summary ダメージ処理
     ***********************************************************************************/
    private IEnumerator INTERNAL_OnDamage()
    {
        this.mAnimator.Play("Dead");

        // 飛び跳ね処理
        mRigidbody2D.velocity = new Vector2(transform.forward.x * backwardForce.x, transform.up.y * backwardForce.y);

        yield return new WaitForSeconds(0.2f);

        // 着地するまで待つ
        while (mIsGround == false)
        {
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(0.2f);

        Restart();
    }

    /** ********************************************************************************
     * @summary リスタート処理
     ***********************************************************************************/
    private void Restart()
    {
        // 座標の初期化
        this.transform.localPosition = this.initPos;
        this.canJump2nd = true;

        // 加速度の初期化
        mRigidbody2D.velocity = Vector2.zero;

        // Animator
        this.mAnimator.applyRootMotion = false;
        this.mAnimator.Play("Idle");

        // キャラクターの状態初期化
        this.charaState = CharacterState.Normal;
    }

    /** ********************************************************************************
     * @summary 操作の切り替え
     ***********************************************************************************/
    private bool ChangeState(InputState newState)
    {
        // 違う場合のみ切り替える
        if (this.inputState != newState)
        {
            this.inputState = newState;
            Color prevColor;
            switch (this.inputState)
            {
                case InputState.Chalk:
                    this.chalk.gameObject.SetActive(true);
                    this.eraser.gameObject.SetActive(false);
                    prevColor = this.characterSprite.color;
                    this.characterSprite.color = new Color(prevColor.r, prevColor.g, prevColor.b, 0.5f);

                    break;

                case InputState.Character:
                    this.chalk.gameObject.SetActive(false);
                    this.eraser.gameObject.SetActive(false);
                    prevColor = this.characterSprite.color;
                    this.characterSprite.color = new Color(prevColor.r, prevColor.g, prevColor.b, 1.0f);

                    break;

                case InputState.Eraser:
                    this.chalk.gameObject.SetActive(false);
                    this.eraser.gameObject.SetActive(true);
                    prevColor = this.characterSprite.color;
                    this.characterSprite.color = new Color(prevColor.r, prevColor.g, prevColor.b, 0.5f);

                    break;
            }

            // 切り替わったときはtrueで返す
            return true;
        }

        return false;
    }
}
