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
        //���� ������Ʈ ����
        DontDestroyOnLoad(this.gameObject);

       
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;

        //�Ѵ� ������ ����
        if(NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            //���� ���� �÷��̾ ����, ü���� �� ����������
            //���� ���� â�� ����-ȣ��Ʈ���� ����
            //��� �ϱ�-������ �ϱ� ���θ� �������� ���� �ϱ�
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GoNextMapServerRPC()
    {

        if (SceneManager.GetActiveScene().name == "TutorialScene")
        {
            //�������� ���� ���� ������

            NetworkManager.Singleton.SceneManager.LoadScene("InGameScene_1", LoadSceneMode.Single);
            return;
        } else if (SceneManager.GetActiveScene().name == "InGameScene_1")
        {
            NetworkManager.Singleton.SceneManager.LoadScene("InGameScene_2", LoadSceneMode.Single);
            return;
        }


    }
}
