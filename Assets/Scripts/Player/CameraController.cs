using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class CameraController : NetworkBehaviour
{

    public GameObject cameraHolder;
    public Vector3 offset;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        /* 현재 로컬에 2개 플레이어 다 있어서 주석 처리.
         * 추후 정식 출시때 해제 예정*/
        if (IsOwner) cameraHolder.SetActive(true);
        else cameraHolder.SetActive(false);
        
    }


    public void Update()
    {
        
        cameraHolder.transform.localPosition = transform.localPosition + offset;
        //Debug.Log(cameraHolder.transform.localPosition);
        
        
    }
}
