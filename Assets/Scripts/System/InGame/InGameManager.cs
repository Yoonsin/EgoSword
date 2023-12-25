using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class InGameManager : NetworkBehaviour
{
    [SerializeField] private GameObject ClientGameOverMenu, HostGameOverMenu;
    public static InGameManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

     
    }

    // Start is called before the first frame update
    void Start()
    {
        //게임 오브젝트 보존
        DontDestroyOnLoad(this.gameObject);

       
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        //둘다 접속한 상태
        if(NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            //만약 현재 플레이어가 기사고, 체력이 다 떨어졌으면
            //게임 오버 창을 서버-호스트에게 띄우고
            //계속 하기-리스폰 하기 여부를 서버에게 고르게 하기
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GoNextMapServerRPC()
    {

        if (SceneManager.GetActiveScene().name == "TutorialScene")
        {
            //서버에서 게임 씬을 시작함

            NetworkManager.Singleton.SceneManager.LoadScene("InGameScene_1", LoadSceneMode.Single);
            return;
        } else if (SceneManager.GetActiveScene().name == "InGameScene_1")
        {
            NetworkManager.Singleton.SceneManager.LoadScene("InGameScene_2", LoadSceneMode.Single);
            return;
        }


    }
}
