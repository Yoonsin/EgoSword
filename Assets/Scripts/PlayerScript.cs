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

    //OnNetworkSpawn���� ���� �ҷ����ϱ� ������ Start�� �ƴ� Awake���� �����Ҵ�
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
            //�Ƹ� �����ڸ� ������ �÷��̾�� �������̰� �Ϸ��� �̷��� �ѵ�?
            player.enabled = false;
            this.enabled = false;
        }
        else
        {
            myCamera.Priority = 100;
            //���� ��ġ �������� ������ �÷��̾�� ������ ������ �ִ� ��
            player.enabled = false;
            this.transform.position = new Vector3(0, 0, 0);
            player.enabled = true;
        }
    }

    private bool IsCheckGrounded()
    {
        // CharacterController.IsGrounded�� true��� Raycast�� ������� �ʰ� ���� ����
        if (player.isGrounded) return true;
        // �߻��ϴ� ������ �ʱ� ��ġ�� ����
        // �ణ ��ü�� ���� �ִ� ��ġ�κ��� �߻����� ������ ����� ������ �� ���� ���� �ִ�.
        var ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
        // Ž�� �Ÿ�
        var maxDistance = 0.5f;
        // ���� ����� �뵵
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * maxDistance, Color.red);
        // Raycast�� hit ���η� ����
        // ���󿡸� �浹�� ���̾ ����
        return Physics.Raycast(ray, maxDistance, layerMask);
    }
}
