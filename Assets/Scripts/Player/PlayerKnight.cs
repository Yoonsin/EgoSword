using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerKnight : PlayerBase
{
    public float shakeAmount = 10.0f;
    public float shakeTime = 1.0f;

    public float jumpPower;
    public float kickPower;
    public SpriteRenderer spriteRenderer;
    public Animator anim;
    public NetworkAnimator networkAnimator;
    public RectTransform HealthUI;
    public GameObject cameraHolder;


    [HideInInspector]
    public NetworkVariable<float> health = new NetworkVariable<float>();

    bool isJumping = false;
    bool isKickWall = false;
    bool isKickWallJump = false;
    bool isDamaged = false;

    

    float slidingSpeed = 0.5f;
    float monsterDamage = 20f;

    public Text text;


    private void Start()
    {
   
        //나머지는 움직일 때 어차피 서버에서 처리해 주니까
        //굳이 NetworkVariable로 안만들어도 됨
        speed = 13000f * 3;
        jumpPower = 600f;
        kickPower = 500f;
        rb = this.GetComponent<Rigidbody2D>();
       
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        health.Value = 100f;


        if (!this.GetComponent<NetworkObject>().IsOwner)
        {
            //소유자를 제외한 플레이어는 이 오브젝트를 움직일 권한이 없다
            this.enabled = false;
        }
        else
        {
            this.enabled = true;
        }

        //체력 UI 가져오기
        HealthUI = GameObject.Find("Health").GetComponent<RectTransform>();

    }

    protected override void FixedUpdate()
    {
        //base.FixedUpdate();
        if (!isKickWallJump&&!isDamaged)
        {
            Move();
        }

    }

    private void Update()
    {
        text.text = "IsOwner : " + GetComponent<NetworkObject>().IsOwner + "\nIsOwnedByServer : "
          + GetComponent<NetworkObject>().IsOwnedByServer + "\nIsLocalPlayer : " + GetComponent<NetworkObject>().IsLocalPlayer;



        if (!isKickWall)
        {
            if (inputX!=0)
            {
                anim.SetBool("isRun", true);
                RequestFlipPlayerServerRPC(isRight);
            }else anim.SetBool("isRun", false);
        }

        //점프
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
            isJumping = true;
        }

        //주위에 벽치기를 할 수 있는 벽 확인
        Vector2 kickDir = new Vector2(isRight, 0);
        Vector2 vc2 = new Vector2(rb.position.x + kickDir.x * 30f, rb.position.y-10f);
        Debug.DrawRay(vc2, kickDir * 10f, new Color(1, 0, 0));
        RaycastHit2D rayHit = Physics2D.Raycast(vc2, kickDir, 10, LayerMask.GetMask("Wall"));
        if (rayHit.collider != null&&rayHit.collider.CompareTag("NormalWall")) isKickWall = true;
        else isKickWall = false;
      
        if (isKickWall)
        {
            anim.SetBool("isRun", false);
            isKickWallJump = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * slidingSpeed);

            //벽치기
            if (Input.GetKeyDown(KeyCode.C))
            {
                //체공중이고 벽 가까이 붙어있을 때  
                Debug.Log("Kick");
                RequestKickWallServerRPC(kickDir);
            }

        }

        //상호작용 ((전) 밀치기)
        if (Input.GetKeyDown(KeyCode.F))
        {
            networkAnimator.SetTrigger("doBack"); //점핑 애니메이션 트리거
            RequestInteractionServerRPC();
        }

        //점프 가능여부 체크(체공중일 때만 걸림)
        if (rb.velocity.y < 0)
        {
            //플레이어는 레이캐스트에 잡히면 안됨
            vc2 = new Vector2(rb.position.x, rb.position.y - 50f);
            Debug.DrawRay(vc2, Vector3.down * 50f, new Color(0, 1, 0));

            rayHit = Physics2D.Raycast(vc2, Vector3.down, 50, LayerMask.GetMask("Ground") | LayerMask.GetMask("Wall") | LayerMask.GetMask("Player"));
            if (rayHit.collider != null)
            {
                isJumping = false;
                anim.SetBool("isJump", false);
            }
        }

    }

    void Jump()
    {
        if (isJumping) return;
        networkAnimator.SetTrigger("doJumping");
        //anim.SetTrigger("doJumping"); //점핑 애니메이션 트리거
        anim.SetBool("isJump", true); //점핑 플래그
        RequestJumpServerRPC();
    }

    [ServerRpc]
    void RequestJumpServerRPC()
    {
        Vector2 jumpVelocity = new Vector2(0, jumpPower);
        rb.AddForce(jumpVelocity, ForceMode2D.Impulse);
    }

    [ServerRpc]
    void RequestInteractionServerRPC()
    {

    }

    [ServerRpc]
    void RequestKickWallServerRPC(Vector2 kickDir)
    {
        if (isKickWallJump) return;
        isKickWallJump = true;
        Invoke("FreezeKickX", 0.3f);
        Vector2 kickVelocity = new Vector3(-kickDir.x * kickPower, 0.9f*kickPower);
        rb.AddForce(kickVelocity, ForceMode2D.Impulse);
        isRight = -1 * isRight;
        RequestFlipPlayerServerRPC(isRight);
        
    }
    

    //이동 정지
    void FreezeKickX()
    {
        isKickWallJump = false;
    }

    void FreezeMoveX()
    {
        isDamaged = false;
    }

    [ServerRpc]
    //방향 전환
    void RequestFlipPlayerServerRPC(int isRight)
    {
        RequestFlipPlayerClientRPC(isRight);
    }

    [ClientRpc]
    void RequestFlipPlayerClientRPC(int isRight)
    {
        spriteRenderer.flipX = isRight == 1 ? false : true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject col = collision.gameObject;

        //적 또는 가시에 충돌했을 시
        if (col.CompareTag("Monster")||col.CompareTag("Spear"))
        {
            //데미지
            OnDamaged(col);
            return;
        }
        Debug.Log(col.tag);
        //포탈에 충돌했을 시
        if (col.CompareTag("Portal"))
        {
            //다음 맵으로 이동
            InGameManager.instance.GoNextMapServerRPC();

            return;
        }
    }

    void OnDamaged(GameObject col)
    {

        RequestSetHealServerRPC(-monsterDamage);
        isDamaged = true;
        Invoke("FreezeMoveX", 1.0f);

        StartCoroutine(Shake(shakeAmount,shakeTime));
        Vector2 attackedVelocity = Vector2.zero;

        //넉백
        attackedVelocity = new Vector2(-(isRight*1f), 1f);
        rb.AddForce(attackedVelocity * kickPower , ForceMode2D.Impulse);
    }

    [ServerRpc]
    void RequestSetHealServerRPC(float adjustVal)
    {
        try
        {
            health.Value += adjustVal;
        }
        catch
        {

        }
       
        HealthUI.transform.localScale = new Vector3(health.Value / 100f, 1, 1);
    }

    IEnumerator Shake(float ShakeAmount, float ShakeTime)
    {
        float timer = 0;

        while (timer <= ShakeTime)
        {
            cameraHolder.transform.position = cameraHolder.transform.position + ((Vector3)UnityEngine.Random.insideUnitCircle * shakeAmount) ;
            timer += Time.deltaTime;
            yield return null;
        }
        
    }



}
