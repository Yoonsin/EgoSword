using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlatFormController : MonoBehaviour
{
    public Tilemap tileMap;
    public Tile spearTile; //가시 타일
    public Tile cliffTile; //절벽 타일
    public Tile rockTile; //바위 타일

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
