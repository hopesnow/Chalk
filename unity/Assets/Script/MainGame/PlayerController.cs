using UnityEngine;
using UniRx;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
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

    [SerializeField] private int playerNo;

    private Animator mAnimator;
    private BoxCollider2D mBoxcollier2D;
    private Rigidbody2D mRigidbody2D;
    private bool mIsGround;
    private const float mCenterY = 1.5f;

    private InputState inputState = InputState.None;       // 入力したときの状態
    private CharacterState charaState = CharacterState.Normal;  // キャラクターの動作に関わる状態
    private bool canJump2nd = true;                             // 2段ジャンプ可能か

    private Vector3 initPos = Vector3.zero; // リセット時の初期座標

    // ゴールしたフラグ
    public ReactiveProperty<bool> IsGoal = new ReactiveProperty<bool>();

    public int PlayerNo { get { return this.playerNo; } set { this.playerNo = value; } }

    /** ********************************************************************************
     * @summary 初期化処理
     ***********************************************************************************/
    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
        mBoxcollier2D = GetComponent<BoxCollider2D>();
        mRigidbody2D = GetComponent<Rigidbody2D>();

        this.initPos = this.transform.localPosition;
        IsGoal.Value = false;

        ChangeState(InputState.Character);
    }

    /** ********************************************************************************
     * @summary リセット処理
     ***********************************************************************************/
    public void Reset()
    {
        // 座標の初期化
        this.transform.localPosition = this.initPos;
        this.canJump2nd = true;

        // 加速度の初期化
        mRigidbody2D.velocity = Vector2.zero;

        // Animator
        this.mAnimator.applyRootMotion = false;

        // ゴールフラグの初期化
        IsGoal.Value = false;
        this.charaState = CharacterState.Normal;
        this.mAnimator.SetTrigger("Reset");

        // 入力方法の初期化
        // this.inputState = InputState.Character;
        ChangeState(InputState.Character);
    }

    /** ********************************************************************************
     * @summary 更新処理
     ***********************************************************************************/
    private void Update()
    {
        float moveVec = 0;
        bool jump = false;

        // 操作切り替え
        if (Input.GetButtonDown(string.Format("Player{0} Chalk", playerNo)))
        {
            // 操作が切り替わる場合は切り替えてそこでUpdate処理終了
            if (ChangeState(InputState.Chalk))
            {
                return;
            }
        }
        else if (Input.GetButtonDown(string.Format("Player{0} Character", playerNo)))
        {
            // 操作が切り替わる場合は切り替えてそこでUpdate処理終了
            if (ChangeState(InputState.Character))
            {
                return;
            }
        }
        else if (Input.GetButtonDown(string.Format("Player{0} Eraser", playerNo)))
        {
            // 操作が切り替わる場合は切り替えてそこでUpdate処理終了
            if (ChangeState(InputState.Eraser))
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

                // ダメージを受けていないかどうか
                if (charaState != CharacterState.Damaged)
                {
                    if (!IsGoal.Value)
                    {
                        moveVec = Input.GetAxis(string.Format("Player{0} Horizontal", playerNo));
                        jump = Input.GetButtonDown(string.Format("Player{0} Jump", playerNo));
                    }
                }

                break;

            /****************************************************************************************************/
            // チョークの操作
            case InputState.Chalk:
                if (Input.GetButtonDown(string.Format("Player{0} Chalk", playerNo)))
                {
                    this.drawLine.SetStartPos(this.chalk.localPosition);
                }

                // 移動具合をみる
                float chalkX = Input.GetAxis(string.Format("Player{0} Horizontal", playerNo));  // 入力値 1 〜 0
                float chalkY = Input.GetAxis(string.Format("Player{0} Vertical", playerNo));    // 入力値 1 〜 0

                // 両方共数値が入っていればmagnitudeが1になるように計算する
                float calcX = 0;
                float calcY = 0;
                if (Mathf.Abs(chalkX) > 0 && Mathf.Abs(chalkY) > 0)
                {
                    float tmp = Mathf.Sqrt(chalkX * chalkX + chalkY * chalkY);
                    calcX = chalkX / tmp;
                    calcY = chalkY / tmp;
                }
                else
                {
                    calcX = chalkX;
                    calcY = chalkY;
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

                // 変化がなければ行わない処理
                if (calcX != 0f || calcY != 0f)
                {
                    var isDrawing = Input.GetButton(string.Format("Player{0} Chalk", playerNo));

                    // 座標移動
                    float power = isDrawing ? this.chalkDrawSpeePower : 1.0f;   // 書いてるときは移動速度倍率をかける
                    this.chalk.localPosition = this.chalk.localPosition + new Vector3(calcX * chalkSpeed * power, calcY * chalkSpeed * power);

                    if(isDrawing)
                    {
                        // 線を引く
                        this.drawLine.DragLine(this.chalk.localPosition);
                    }
                }

                break;

            /****************************************************************************************************/
            // 黒板消しの操作
            case InputState.Eraser:

                break;

            default:

                break;

            /****************************************************************************************************/
        }

        // 移動系処理
        Move(moveVec, jump);
    }

    /** ********************************************************************************
     * @summary 移動処理
     ***********************************************************************************/
    private void Move(float move, bool jump)
    {
        if (Mathf.Abs(move) > 0)
        {
            Quaternion rot = transform.rotation;
            transform.rotation = Quaternion.Euler(rot.x, Mathf.Sign(move) == 1 ? 0 : 180, rot.z);
        }

        mRigidbody2D.velocity = new Vector2(move * maxSpeed, mRigidbody2D.velocity.y);

        mAnimator.SetFloat("Horizontal", move);
        mAnimator.SetFloat("Vertical", mRigidbody2D.velocity.y);
        mAnimator.SetBool("isGround", mIsGround);

        // ジャンプ可能か
        if (jump && (mIsGround || canJump2nd))
        {
            mAnimator.SetTrigger("Jump");
            SendMessage("Jump", SendMessageOptions.DontRequireReceiver);

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
            this.mAnimator.SetTrigger("Clear");
        }
    }

    /** ********************************************************************************
     * @summary ダメージ処理
     ***********************************************************************************/
    private IEnumerator INTERNAL_OnDamage()
    {
        mAnimator.Play(mIsGround ? "Damage" : "AirDamage");
        mAnimator.Play("Idle");

        SendMessage("OnDamage", SendMessageOptions.DontRequireReceiver);

        mRigidbody2D.velocity = new Vector2(transform.right.x * backwardForce.x, transform.up.y * backwardForce.y);

        yield return new WaitForSeconds(.2f);

        while (mIsGround == false)
        {
            yield return new WaitForFixedUpdate();
        }
        mAnimator.SetTrigger("Invincible Mode");
        charaState = CharacterState.Invincible;
    }

    /** ********************************************************************************
     * @summary 無敵終了処理
     ***********************************************************************************/
    private void OnFinishedInvincibleMode()
    {
        charaState = CharacterState.Normal;
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
                    prevColor = this.characterSprite.color;
                    this.characterSprite.color = new Color(prevColor.r, prevColor.g, prevColor.b, 0.5f);

                    break;

                case InputState.Character:
                    this.chalk.gameObject.SetActive(false);
                    prevColor = this.characterSprite.color;
                    this.characterSprite.color = new Color(prevColor.r, prevColor.g, prevColor.b, 1.0f);

                    break;

                case InputState.Eraser:
                    this.chalk.gameObject.SetActive(false);
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
