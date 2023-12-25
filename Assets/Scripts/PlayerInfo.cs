using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInfo : MonoBehaviour
{
    //플레이어 정보가 담긴 클래스
    [SerializeField] private TMP_Text playerName;
    public string steamName;
    public ulong steamId;
    public GameObject readyImage;
    public bool isReady;

    // Start is called before the first frame update
    void Start()
    {
        readyImage.SetActive(false);
        playerName.text = steamName;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
