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
    [SerializeField] private Vector2 backwardForce = new Vector2(-4.5f, 5.4f);
    [SerializeField] private JoystickInfo joystick;

    // 地面オブジェクトはどのLayerか
    [SerializeField] private LayerMask whatIsGround;

    // チョークオブジェクト
    [SerializeField] private Transform chalk;

    private Animator mAnimator;
    private BoxCollider2D mBoxcollier2D;
    private Rigidbody2D mRigidbody2D;
    private bool mIsGround;
    private const float mCenterY = 1.5f;

    private InputState inputState = InputState.Character;       // 入力したときの状態
    private CharacterState charaState = CharacterState.Normal;  // キャラクターの動作に関わる状態
    private bool canJump2nd = true;                             // 2段ジャンプ可能か

    private Vector3 initPos = Vector3.zero; // リセット時の初期座標

    // ゴールしたフラグ
    public ReactiveProperty<bool> IsGoal = new ReactiveProperty<bool>();

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
        this.inputState = InputState.Character;
        // this.inputState = InputState.Chalk;  // DEBUG
    }

    /** ********************************************************************************
     * @summary 更新処理
     ***********************************************************************************/
    private void Update()
    {
        // 操作状態
        switch (this.inputState)
        {
            // キャラクターの操作
            case InputState.Character:

                // ダメージを受けていないかどうか
                if (charaState != CharacterState.Damaged)
                {
                    float x = 0;
                    bool jump = false;

                    if (!IsGoal.Value)
                    {
                        x = Input.GetAxis("Horizontal");
                        jump = Input.GetButtonDown("Jump");
                    }

                    Move(x, jump);
                }

                break;

            // チョークの操作
            case InputState.Chalk:

                break;

            // 黒板消しの操作
            case InputState.Eraser:

                break;

            default:
                
                break;
        }
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


}
