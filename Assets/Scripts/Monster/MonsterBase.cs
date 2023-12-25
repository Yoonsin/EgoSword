using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class MonsterBase : NetworkBehaviour
{

    public float health;

    public float speed;

    public int isRight;

    public Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        isRight = 1;
    }

   
}
