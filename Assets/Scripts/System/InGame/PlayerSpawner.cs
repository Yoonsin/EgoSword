using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System;


public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject KnightPlayer;
    [SerializeField]
    private GameObject SwordPlayer;

    private GameObject KnightPlayerRef;
    private GameObject SwordPlayerRef;
  

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoaded;


        if (IsHost) {
            //ȣ��Ʈ�� ��� ������Ʈ�� �÷���
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 0);
            //Debug.Log(NetworkManager.Singleton.LocalClientId);
        }
        else {

            //�Խ�Ʈ�� �� ������Ʈ�� �÷���
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 1);
 
        }
    }

    private void SceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        //ȣ��Ʈ���� ������Ʈ ���� ��ġ ����
        if (IsHost)
        {
            //호스트
            //if (sceneName == "InGameScene_1")
            //{
            //foreach (ulong id in clientsCompleted)
            //{

            //}

                if (KnightPlayerRef) { 
                    KnightPlayerRef.transform.position = new Vector3(0, 0, 0);
                    KnightPlayerRef.GetComponent<PlayerKnight>().HealthUI = GameObject.Find("Health").GetComponent<RectTransform>();
                    KnightPlayerRef.GetComponent<PlayerKnight>().HealthUI.transform.localScale = new Vector3(KnightPlayerRef.GetComponent<PlayerKnight>().health.Value / 100f, 1, 1);

                
                }

                if (SwordPlayerRef)
                {
                    SwordPlayerRef.transform.position = new Vector3(0, 0, 0);
                    SwordPlayerRef.GetComponent<PlayerSword>().UseUI = GameObject.Find("UseGauge").GetComponent<RectTransform>();
                    SwordPlayerRef.GetComponent<PlayerSword>().UseUI.transform.localScale = new Vector3(SwordPlayerRef.GetComponent<PlayerSword>().useGauge.Value / 100f, 1, 1);
                    
                }
            //}

        }
        else
        {
            if (KnightPlayerRef)
            {
                KnightPlayerRef.transform.position = new Vector3(0, 0, 0);
            }

            if (SwordPlayerRef)
            {
                SwordPlayerRef.transform.position = new Vector3(0, 0, 0);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)] //server owns this object but client can request a spawn
    public void SpawnPlayerServerRpc(ulong clientId, int prefabId)
    {
        GameObject newPlayer;
        if (prefabId == 0)
        {
            newPlayer = (GameObject)Instantiate(KnightPlayer);
            KnightPlayerRef = newPlayer;
            
        }
        else
        {
            newPlayer = (GameObject)Instantiate(SwordPlayer);
            SwordPlayerRef = newPlayer;
        }

        DontDestroyOnLoad(newPlayer);
        newPlayer.SetActive(true);
        newPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        
        newPlayer.transform.position = new Vector3(0, 0, 0);
    }


}
