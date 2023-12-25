using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;


public class PlayerSword : PlayerBase
{


    bool isEntered = false; //������ ���� ��������
    bool isCliff = false; //��ó�� ������ �ִ���
    bool isFreeze = false; //������ �� �ִ���
    bool isFire = false; //���ư��� ��������



    [HideInInspector]
    public NetworkVariable<float> useGauge = new NetworkVariable<float>();

    public RectTransform UseUI;
    public float FirePower;
    public SpriteRenderer spriteRenderer;
    public Animator anim;
    public NetworkAnimator networkAnimator;
    public Transform[] HitPoint;

    public Tilemap tilemap;

    public Text text;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        speed = 500f;
        FirePower = 700f;

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        useGauge.Value = 100f;
        tilemap = GameObject.Find("Grid").transform.Find("Rock").gameObject.GetComponent<Tilemap>();


        Debug.Log(transform.GetComponentInParent<NetworkObject>().IsOwner);
        if (!transform.GetComponentInParent<NetworkObject>().IsOwner)
        {
            //�����ڸ� ������ �÷��̾�� �� ������Ʈ�� ������ ������ ����
            this.enabled = false;
        }
        else
        {
            this.enabled = true;
        }


    }

  

    protected override void FixedUpdate()
    {
        if (!isEntered && !isFreeze)
        {
           Move();
        }

       
    }


    // Update is called once per frame
    void Update()
    {
        //�����
        text.text = "IsOwner : " + transform.GetComponentInParent<NetworkObject>().IsOwner + "\nIsOwnedByServer : "
           + transform.GetComponentInParent<NetworkObject>().IsOwnedByServer + "\nIsLocalPlayer : " + transform.GetComponentInParent<NetworkObject>().IsLocalPlayer;

        //���� �� �ִ� ���� �ִ��� Ȯ��
        if (!isEntered) {
            Vector2 enterDir = new Vector2(isRight, 0);
            Vector2 vc2 = new Vector2(rb.position.x + enterDir.x * 20f, rb.position.y - 10f);
            Debug.DrawRay(vc2, enterDir * 20f, new Color(1, 0, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(vc2, enterDir, 20, LayerMask.GetMask("Wall"));

            //if (rayHit.collider != null) Debug.Log(rayHit.collider.tag);

            if (rayHit.collider != null && rayHit.collider.CompareTag("Cliff")) isCliff = true;
            else isCliff = false;
        }
       
        //���� �ڱ�
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.LogFormat("isCliff : {0} / isEntered : {1}", isCliff, isEntered);
           
            if (isCliff&&!isEntered)
            {
                //���� �ڱ� ����
                isEntered = true;
                //���� ������ ���߱�
                RequestMoveServerRPC(Vector3.zero);

               
                anim.SetBool("isEnter", true);
                RequestOnEnterServerRPC(isRight);

            }
            else if(isEntered)
            {
                //���� �ڱ� ����
                isEntered = false;

                anim.SetBool("isEnter", false);
                RequestStopEnterServerRPC(isRight);

            }
        }

        //�߻�
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!isEntered&&!isFire)
            {
                OnFire();
            }
        }
            

        
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOnEnterServerRPC(int isRight)
    {

        rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
        rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;

      
        RequestRotate(isRight, false);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestStopEnterServerRPC(int isRight)
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        RequestRotate(isRight, true);
       
    }

    void RequestRotate(int isRight,bool reverse)
    {
        if (reverse)
        {
            transform.Rotate(0, 0, 90 * isRight * -1);
        }
        else
        {
            transform.Rotate(0, 0, 90 * isRight);
        }
    }

    void OnFire()
    {
        RequestFireServerRPC(isRight);
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestFireServerRPC(int isRight)
    {
        //���� ������ ���߱�
        RequestMoveServerRPC(Vector3.zero);

        RequestFreezeMoveXClientRPC();

        //90���� ȸ����ä ���ư�����
        RequestRotate(isRight, false); 
        //���� �浹���� ���� �ִٸ� �׳� ���ֱ�
        DestroyRock();

        //x �Ӽ��� ����ְ� �������� ����
        rb.constraints = ~RigidbodyConstraints2D.FreezePositionX;

        RequestUpdateFireVelocityClientRPC();
        Invoke("RequestFreezeStopMoveXClientRPC", 1.0f);
       
       
    }

    [ClientRpc]
    void RequestUpdateFireVelocityClientRPC()
    {
      
        Vector3 FireVelocity = new Vector3(isRight * FirePower , 0);
        RequestMoveServerRPC(FireVelocity);
    }
    [ClientRpc]
    void RequestFreezeMoveXClientRPC()
    {
        isFreeze = true;
        isFire = true;
    }

    [ClientRpc]
    void RequestFreezeStopMoveXClientRPC()
    {
        RequestMoveServerRPC(Vector3.zero);

        isFreeze = false;
        isFire = false;

        //90���� ȸ�� ����
        transform.rotation = Quaternion.identity;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

    }


    protected override void Move()
    {
        //���� �����¿� �̵��� ����� �ϱ� ������ �������̵�
        if (!IsLocalPlayer) return;

        Vector3 moveVelocity = Vector3.zero;

        float xMove = Input.GetAxisRaw("Horizontal");
        float yMove = Input.GetAxisRaw("Vertical");

        if (xMove > 0)
        {
            moveVelocity += Vector3.right;
            isRight = 1;
        }
        else if(xMove<0)
        {
            moveVelocity += Vector3.left;
            isRight = -1;
        }

        if (yMove > 0) {
            moveVelocity += Vector3.up;
        } else if (yMove<0) 
        {
            moveVelocity += Vector3.down;
        }

        if (moveVelocity.x != 0) RequestFlipPlayerServerRPC(isRight);

        

        moveVelocity = new Vector2(moveVelocity.x, moveVelocity.y) * speed;
        RequestMoveServerRPC(moveVelocity);

    }

    [ServerRpc(RequireOwnership = false)]
    //���� ��ȯ
    void RequestFlipPlayerServerRPC(int isRight)
    {
        RequestFlipPlayerClientRPC(isRight);
    }

    [ClientRpc]
    void RequestFlipPlayerClientRPC(int isRight)
    {
        spriteRenderer.flipX = isRight == 1 ? false : true;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        bool destroyFlag = false;
        //�΋H���� ���̸� �ı�
        if (isFire&&col.gameObject.CompareTag("Rock"))
        {
            for(int i = 0; i < HitPoint.Length; i++)
            {
                //col.transform.transform.position
                
                Vector3Int cellPos = tilemap.WorldToCell(HitPoint[i].position);
                RequestDestroyRockServerRPC(cellPos);
                destroyFlag = true;
            }
           
        }

        //���� �ı��� ���� �ִٸ�..
        //���� ������ ���߱�
        if (destroyFlag)
        { 
            RequestMoveServerRPC(Vector3.zero);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestDestroyRockServerRPC(Vector3Int cellPos)
    {
        RequestDestroyRockrClientRPC(cellPos);
    }


    [ClientRpc]
    void RequestDestroyRockrClientRPC(Vector3Int cellPos)
    {
        tilemap.SetTile(cellPos, null);
    }

    private void DestroyRock()
    {
        bool destroyFlag = false;
        if (isFire )
        {
            for (int i = 0; i < HitPoint.Length; i++)
            {
                Collider2D col = Physics2D.OverlapCircle(HitPoint[i].position, 0.01f, LayerMask.GetMask("Wall"));
                if(col != null && col.gameObject.CompareTag("Rock"))
                {
                    Vector3Int cellPos = tilemap.WorldToCell(HitPoint[i].position);
                    RequestDestroyRockServerRPC(cellPos);
                     destroyFlag = true;
                   
                }
            }

        }

        if (destroyFlag)
        {//���� ������ ���߱�
           RequestMoveServerRPC(Vector3.zero);
        }
    }


}
