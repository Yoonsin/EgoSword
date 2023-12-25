using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using Cinemachine;
using System;

public enum STATE
{
    ABLE,
    DAMAGED,
    USING,
    USED
};


public abstract class PlayerBase : NetworkBehaviour
{
    
    public float speed;

    protected NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public Rigidbody2D rb;

    public int isRight;

    protected float inputX;

    protected STATE state;

    

    Vector3 oldMoveVelocity;
  
    //Transform cam;
    int layerMask;

    void Awake()
    {
        layerMask = 1 << LayerMask.NameToLayer("Ground");
        isRight = 1;
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //DontDestroyOnLoad(this.gameObject);


    }

    protected virtual void FixedUpdate()
    {
        //서버측 변경 내용 클라이언트에 적용
    }

    protected virtual void Move()
    {
        if (!IsLocalPlayer) return;
        
        Vector3 moveVelocity = Vector3.zero;

        inputX = Input.GetAxisRaw("Horizontal");
        if (inputX < 0)
        {
            moveVelocity = Vector3.left;
            isRight = -1;
        }
        else if (inputX > 0)
        {
            moveVelocity = Vector3.right;
            isRight = 1;
        }

        moveVelocity = new Vector3(moveVelocity.x *speed * Time.deltaTime, rb.velocity.y);
        RequestMoveServerRPC(moveVelocity);

    }


    [ServerRpc(RequireOwnership = false)] //server owns this object but client can request a spawn
    protected void RequestMoveServerRPC(Vector3 RequestTransform)
    {
        rb.velocity = RequestTransform;
        //모든 클라이언트에게 알려주기

        Position.Value = rb.velocity;
  
    }

}
