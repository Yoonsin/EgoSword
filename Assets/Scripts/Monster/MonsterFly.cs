using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterFly : MonsterBase
{
    public SpriteRenderer spriteRenderer;

    float flipTime = 2.0f;
    float ampitude = 300f;
    float x = 0.0f;
    float y = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        health = 100f;
        speed = 500f;
        rb = this.GetComponent<Rigidbody2D>();
        StartCoroutine(FlipFly());
        x = transform.position.x;
        y= transform.position.y;   
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Vector2 move = GetSinPos(x);
        */

        if (isRight == 1)
        {
            x += Time.deltaTime * speed;
        }
        else if(isRight == -1)
        {
            x -= Time.deltaTime * speed;
        }

        transform.position = new Vector3(x , y+ampitude*Mathf.Sin(x/300), 0);

        //Vector3 moveVelocity = new Vector3(x*speed*Time.deltaTime, Mathf.Sin(x)*speed*Time.deltaTime);
        //rb.velocity = moveVelocity;


        //RequestMoveServerRPC(moveVelocity);
    }

    IEnumerator FlipFly()
    {
        while (true)
        {
            isRight = isRight == 1 ? -1 : 1;
            spriteRenderer.flipX = isRight == 1 ? false : true;
            yield return new WaitForSeconds(flipTime);
        }

    }
}
