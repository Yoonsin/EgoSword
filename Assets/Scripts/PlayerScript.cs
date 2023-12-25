using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class PlayerScript : NetworkBehaviour
{
    
    public CinemachineFreeLook myCamera;
    public float speed, jumpSpeed, gravity;

    CharacterController player;
    Transform cam;
    Vector3 MoveDir;
    int layerMask;



    // Start is called before the first frame update

    //OnNetworkSpawn보다 먼저 불려야하기 때문에 Start가 아닌 Awake에서 변수할당
    void Awake()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
        player = GetComponent<CharacterController>();
        MoveDir = Vector3.zero;
        layerMask = 1 << LayerMask.NameToLayer("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        if(IsCheckGrounded())
        {
            MoveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            MoveDir = transform.TransformDirection(MoveDir);
            MoveDir *= speed;
        }

        if (Input.GetButton("Jump"))
        {
            MoveDir.y = jumpSpeed;
        }

        Debug.Log(MoveDir * Time.deltaTime);
        MoveDir.y -= gravity * Time.deltaTime;
        player.Move(MoveDir * Time.deltaTime);

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            //아마 소유자를 제외한 플레이어는 못움직이게 하려고 이렇게 한듯?
            player.enabled = false;
            this.enabled = false;
        }
        else
        {
            myCamera.Priority = 100;
            //먼저 위치 조정해준 다음에 플레이어에게 움직일 권한을 주는 듯
            player.enabled = false;
            this.transform.position = new Vector3(0, 0, 0);
            player.enabled = true;
        }
    }

    private bool IsCheckGrounded()
    {
        // CharacterController.IsGrounded가 true라면 Raycast를 사용하지 않고 판정 종료
        if (player.isGrounded) return true;
        // 발사하는 광선의 초기 위치와 방향
        // 약간 신체에 박혀 있는 위치로부터 발사하지 않으면 제대로 판정할 수 없을 때가 있다.
        var ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
        // 탐색 거리
        var maxDistance = 0.5f;
        // 광선 디버그 용도
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * maxDistance, Color.red);
        // Raycast의 hit 여부로 판정
        // 지상에만 충돌로 레이어를 지정
        return Physics.Raycast(ray, maxDistance, layerMask);
    }
}
