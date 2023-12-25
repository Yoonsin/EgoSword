using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class MonsterGoblin : MonsterBase
{
    public float flipTime = 2.0f;
    public SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        health = 100f;
        speed =9000f;
        rb = this.GetComponent<Rigidbody2D>();
        StartCoroutine(FlipGoblin());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 moveVelocity = Vector3.zero;
        if (isRight == -1 )
        {
            moveVelocity = Vector3.left;
           
        }
        else if (isRight == 1 )
        {
            moveVelocity = Vector3.right;
            
        }

        moveVelocity = new Vector3(moveVelocity.x * speed * Time.deltaTime, rb.velocity.y);
        rb.velocity = moveVelocity;
        //RequestMoveServerRPC(moveVelocity);
    }

    IEnumerator FlipGoblin()
    {
        while(true)
        {
            isRight = isRight == 1 ? -1 : 1;
            spriteRenderer.flipX = isRight == 1 ? false : true;
            yield return new WaitForSeconds(flipTime);
        }
       
    }
}
