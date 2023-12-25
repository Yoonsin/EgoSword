using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkTransmission : NetworkBehaviour
{
    //게임 네트워크에 대한 함수 관리 
    public static NetworkTransmission instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    
    //서버에서 실행
    [ServerRpc(RequireOwnership = false)]
    public void IWishToSendAChatServerRPC(string _message, ulong _fromWho)
    {
        ChatFromServerClientRPC(_message, _fromWho);
    }

    //클라이언트가 실행
    [ClientRpc]
    private void ChatFromServerClientRPC(string _message, ulong _fromWho)
    {
        GameManager.Instance.SendMessageToChat(_message, _fromWho,false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMeToDictionaryServerRPC(ulong _steamId,string _steamName, ulong _clientId)
    {
        GameManager.Instance.SendMessageToChat($"{_steamName} has joined", _clientId, true);
        GameManager.Instance.AddPlayerToDictionary(_clientId, _steamName, _steamId);
        GameManager.Instance.UpdateClients();
    }

    [ServerRpc(RequireOwnership =false)]
    public void RemoveMeFromDictionaryServerRPC(ulong _steamId)
    {
        RemovePlayerFromDictionaryClientRPC(_steamId);
    }

    [ClientRpc]
    private void RemovePlayerFromDictionaryClientRPC(ulong _steamId)
    {
        Debug.Log("Removing client");
        GameManager.Instance.RemovePlayerFromDictionary(_steamId);
    }

    [ClientRpc]
    public void UpdateClientsPlayerInfoClientRPC(ulong _steamId, string _steamName, ulong _clientId)
    {
        //add to dictionary
        GameManager.Instance.AddPlayerToDictionary(_clientId, _steamName,_steamId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void IsTheClientReadyServerRPC(bool _ready, ulong _clientId)
    {
        AClientMightBeReadyClientRPC(_ready, _clientId);
    }

    [ClientRpc]
    private void AClientMightBeReadyClientRPC(bool _ready, ulong _clientId)
    {
        foreach(KeyValuePair<ulong,GameObject> player in GameManager.Instance.playerInfo)
        {
            if(player.Key == _clientId)
            {
                player.Value.GetComponent<PlayerInfo>().isReady = _ready;
                player.Value.GetComponent<PlayerInfo>().readyImage.SetActive(_ready);
                if (NetworkManager.Singleton.IsHost)
                {
                    Debug.Log(GameManager.Instance.CheckIfPlayersAreReady());
                    //check if player are ready
                }

            }
        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void SessionStartServerRPC()
    {
        //서버에서 게임 씬을 시작함
        NetworkManager.Singleton.SceneManager.LoadScene("TutorialScene", LoadSceneMode.Single);  
    }
    

   

}
