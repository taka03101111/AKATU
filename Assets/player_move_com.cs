using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class player_move_com : NetworkBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 120.0f;
    public float gravity = -9.8f;

    [Header("視点設定")]
    public Transform viewCamera;
    public float cameraLookSpeed = 80.0f;
    public float minCameraAngle = -30.0f;
    public float maxCameraAngle = 45.0f;

    [Header("アニメーション時間")]
    public float attackTime = 1.0f;
    public float damageTime = 0.8f;

    private Animator animator;
    private CharacterController controller;

    private int damageCount = 0;
    private bool isAction = false;
    private bool isDead = false;

    private float cameraPitch = 0.0f;
    private float verticalVelocity = 0.0f;

    private int currentAnimId = -1;

    // ネットワークで同期するアニメーション番号
    [Networked] private int NetworkAnimId { get; set; }

    // アニメーションID
    private const int ANIM_IDLE = 0;
    private const int ANIM_RUN_FORWARD = 1;
    private const int ANIM_RUN_BACK = 2;
    private const int ANIM_RUN_LEFT = 3;
    private const int ANIM_RUN_RIGHT = 4;
    private const int ANIM_ATTACK_STAB = 5;
    private const int ANIM_ATTACK_FULL = 6;
    private const int ANIM_DAMAGE = 7;
    private const int ANIM_DEATH = 8;

    // Animatorのステート名
    private const string IDLE = "Combat (1)";
    private const string RUN_FORWARD = "前に走る";
    private const string RUN_BACK = "後ろに走る";
    private const string RUN_LEFT = "左に走る";
    private const string RUN_RIGHT = "右に走る";
    private const string ATTACK_STAB = "剣をさす攻撃";
    private const string ATTACK_FULL = "剣をフル攻撃";
    private const string DAMAGE = "Take Damage (1)";
    private const string DEATH = "Death 01";

    private bool IsMine
    {
        get
        {
            if (Object == null) return false;

            // Shared Modeでは基本的に自分がSpawnしたものがStateAuthorityを持つ
            // Host系でも動きやすいようにInputAuthorityも見ておく
            return Object.HasStateAuthority || Object.HasInputAuthority;
        }
    }

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        SetupCamera();

        if (IsMine)
        {
            ChangeAnim(ANIM_IDLE);
        }
    }

    void Update()
    {
        // 自分のキャラ以外は操作しない
        if (!IsMine)
        {
            return;
        }

        // 死亡したら操作できない
        if (isDead)
        {
            return;
        }

        // Pでダメージ
        if (Input.GetKeyDown(KeyCode.P))
        {
            TakeDamage();
            return;
        }

        // 攻撃中・ダメージ中は他の操作を止める
        if (isAction)
        {
            return;
        }

        // Jで剣をさす攻撃
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(ActionAnimation(ANIM_ATTACK_STAB, attackTime));
            return;
        }

        // Kで剣をフル攻撃
        if (Input.GetKeyDown(KeyCode.K))
        {
            StartCoroutine(ActionAnimation(ANIM_ATTACK_FULL, attackTime));
            return;
        }

        LookControl();
        MoveControl();
    }

    public override void Render()
    {
        // 他の人の画面でもアニメーションを反映する
        PlayAnimById(NetworkAnimId);
    }

    void MoveControl()
    {
        Vector3 move = Vector3.zero;
        int nextAnim = ANIM_IDLE;

        if (Input.GetKey(KeyCode.W))
        {
            move += transform.forward;
            nextAnim = ANIM_RUN_FORWARD;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            move -= transform.forward;
            nextAnim = ANIM_RUN_BACK;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            move -= transform.right;
            nextAnim = ANIM_RUN_LEFT;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            move += transform.right;
            nextAnim = ANIM_RUN_RIGHT;
        }

        if (move != Vector3.zero)
        {
            move.Normalize();
        }

        // 重力処理
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1.0f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = move * moveSpeed;
        velocity.y = verticalVelocity;

        // 壁を貫通しにくくするためCharacterControllerで動かす
        controller.Move(velocity * Time.deltaTime);

        ChangeAnim(nextAnim);
    }

    void LookControl()
    {
        // ← → でキャラを左右回転
        float horizontal = 0.0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal = -1.0f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            horizontal = 1.0f;
        }

        transform.Rotate(0, horizontal * rotateSpeed * Time.deltaTime, 0);

        // ↑ ↓ でカメラ上下
        if (viewCamera != null)
        {
            float vertical = 0.0f;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                vertical = -1.0f;
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                vertical = 1.0f;
            }

            cameraPitch += vertical * cameraLookSpeed * Time.deltaTime;
            cameraPitch = Mathf.Clamp(cameraPitch, minCameraAngle, maxCameraAngle);

            viewCamera.localEulerAngles = new Vector3(cameraPitch, 0, 0);
        }
    }

    void TakeDamage()
    {
        damageCount++;

        if (damageCount >= 3)
        {
            isDead = true;
            isAction = true;
            ChangeAnim(ANIM_DEATH);
        }
        else
        {
            StartCoroutine(ActionAnimation(ANIM_DAMAGE, damageTime));
        }
    }

    IEnumerator ActionAnimation(int animId, float waitTime)
    {
        isAction = true;

        ChangeAnim(animId);

        yield return new WaitForSeconds(waitTime);

        if (isDead)
        {
            yield break;
        }

        isAction = false;
        ChangeAnim(ANIM_IDLE);
    }

    void ChangeAnim(int animId)
    {
        // 自分が権限を持っているときだけ同期用の値を変える
        if (Object != null && Object.HasStateAuthority)
        {
            NetworkAnimId = animId;
        }

        PlayAnimById(animId);
    }

    void PlayAnimById(int animId)
    {
        if (animator == null) return;

        // 同じアニメーションなら再生し直さない
        if (currentAnimId == animId) return;

        currentAnimId = animId;

        string animName = GetAnimName(animId);
        animator.CrossFade(animName, 0.1f);
    }

    string GetAnimName(int animId)
    {
        switch (animId)
        {
            case ANIM_RUN_FORWARD:
                return RUN_FORWARD;

            case ANIM_RUN_BACK:
                return RUN_BACK;

            case ANIM_RUN_LEFT:
                return RUN_LEFT;

            case ANIM_RUN_RIGHT:
                return RUN_RIGHT;

            case ANIM_ATTACK_STAB:
                return ATTACK_STAB;

            case ANIM_ATTACK_FULL:
                return ATTACK_FULL;

            case ANIM_DAMAGE:
                return DAMAGE;

            case ANIM_DEATH:
                return DEATH;

            case ANIM_IDLE:
            default:
                return IDLE;
        }
    }

    void SetupCamera()
    {
        // 自分のキャラではない場合、プレイヤーPrefab内のカメラを無効化
        if (!IsMine)
        {
            Camera[] cameras = GetComponentsInChildren<Camera>(true);
            foreach (Camera cam in cameras)
            {
                cam.gameObject.SetActive(false);
            }

            AudioListener[] listeners = GetComponentsInChildren<AudioListener>(true);
            foreach (AudioListener listener in listeners)
            {
                listener.enabled = false;
            }

            return;
        }

        // 自分のキャラならカメラを設定
        if (viewCamera == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();

            if (childCamera != null)
            {
                viewCamera = childCamera.transform;
            }
            else if (Camera.main != null)
            {
                viewCamera = Camera.main.transform;
            }
        }
    }
}