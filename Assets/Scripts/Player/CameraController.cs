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

        /* ���� ���ÿ� 2�� �÷��̾� �� �־ �ּ� ó��.
         * ���� ���� ��ö� ���� ����*/
        if (IsOwner) cameraHolder.SetActive(true);
        else cameraHolder.SetActive(false);
        
    }


    public void Update()
    {
        
        cameraHolder.transform.localPosition = transform.localPosition + offset;
        //Debug.Log(cameraHolder.transform.localPosition);
        
        
    }
}
