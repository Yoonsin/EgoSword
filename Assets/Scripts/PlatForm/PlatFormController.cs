using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatFormController : MonoBehaviour
{
    public Tilemap tileMap;
    public Tile spearTile; //���� Ÿ��
    public Tile cliffTile; //���� Ÿ��
    public Tile rockTile; //���� Ÿ��

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
