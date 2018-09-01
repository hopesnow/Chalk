using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float jumpPower = 1000f;
    [SerializeField] private Vector2 backwardForce = new Vector2(-4.5f, 5.4f);

    // 地面オブジェクトはどのLayerか
    [SerializeField] private LayerMask whatIsGround;

    private Animator mAnimator;
    private BoxCollider2D mBoxcollier2D;
    private Rigidbody2D mRigidbody2D;
    private bool mIsGround;
    private const float mCenterY = 1.5f;

    private State mState = State.Normal;

    // 初期化処理
    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
        mBoxcollier2D = GetComponent<BoxCollider2D>();
        mRigidbody2D = GetComponent<Rigidbody2D>();
    }

    // リセット処理
    private void Reset()
    {
        Awake();

        // 初期設定
        this.maxSpeed = 10f;
        this.jumpPower = 1000;
        this.backwardForce = new Vector2(-4.5f, 5.4f);
        this.whatIsGround = 1 << LayerMask.NameToLayer("Ground");

        // Transformの初期化
        this.transform.localScale = new Vector3(1, 1, 1);

        // Rigidbody2D
        this.mRigidbody2D.gravityScale = 3.5f;
        this.mRigidbody2D.fixedAngle = true;

        // BoxCollider2D
        this.mBoxcollier2D.size = new Vector2(1, 2.5f);
        this.mBoxcollier2D.offset = new Vector2(0, -0.25f);

        // Animator
        this.mAnimator.applyRootMotion = false;
    }

    // 更新処理
    private void Update()
    {
        if (mState != State.Damaged)
        {
            float x = Input.GetAxis("Horizontal");
            bool jump = Input.GetButtonDown("Jump");
            Move(x, jump);
        }
    }

    // 移動処理
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

        if (jump && mIsGround)
        {
            mAnimator.SetTrigger("Jump");
            SendMessage("Jump", SendMessageOptions.DontRequireReceiver);
            mRigidbody2D.AddForce(Vector2.up * jumpPower);
        }
    }

    // 更新処理
    private void FixedUpdate()
    {
        Vector2 pos = transform.position;
        Vector2 groundCheck = new Vector2(pos.x, pos.y - (mCenterY * transform.localScale.y));
        Vector2 groundArea = new Vector2(mBoxcollier2D.size.x * 0.49f, 0.05f);

        mIsGround = Physics2D.OverlapArea(groundCheck + groundArea, groundCheck - groundArea, whatIsGround);
        mAnimator.SetBool("isGround", mIsGround);
    }

    // 当たり判定処理(その場所にいるかどうか)
    private void OnTriggerStay2D(Collider2D other)
    {
        // あたってるオブジェクトがDamageObjectで通常ステートのときのみ当たり判定処理をおこなう
        if (other.tag == "DamageObject" && mState == State.Normal)
        {
            mState = State.Damaged;
            StartCoroutine(INTERNAL_OnDamage());
        }
    }

    // 
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
        mState = State.Invincible;
    }

    private void OnFinishedInvincibleMode()
    {
        mState = State.Normal;
    }

    private enum State
    {
        Normal,
        Damaged,
        Invincible,
    }
}
