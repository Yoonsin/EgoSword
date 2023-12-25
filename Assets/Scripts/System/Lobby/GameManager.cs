using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
   public static GameManager Instance;

    [SerializeField] private GameObject multiMenu, multiLobby;

    //로비 UI 
    [SerializeField] private GameObject chatPanel, textObject;
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private GameObject playerFieldBox, playerCardPrefab;
    [SerializeField] private GameObject readyButton, NotReadyButton, startButton;

    [SerializeField] private int maxMessages = 20;
    private List<Message> messageList = new List<Message>();

    //인게임 UI

    //플레이어들 정보
    //키 : clientId ( 아마 네트워크 노드? ) 값 : PlayerInfo 컴포넌트가 있는 게임 오브젝트
    //SteamId는 PlayerInfo에..
    public Dictionary<ulong, GameObject> playerInfo = new Dictionary<ulong, GameObject>();

    public bool connected;
    public bool inGame;
    public bool isHost;
    public ulong myClientId;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this; 
        }
    }

    public class Message
    {
        public string text;
        public TMP_Text textObject;
    }

    public void SendMessageToChat(string _text, ulong _fromwho, bool _server)
    {
        if(messageList.Count >= maxMessages)
        {
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
        }
        Message newMessage = new Message();
        string _name = "Server";

        if (!_server)
        {
            if(playerInfo.ContainsKey(_fromwho))
            {
                _name = playerInfo[_fromwho].GetComponent<PlayerInfo>().steamName;
            }
        }

        newMessage.text = _name + ": " + _text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<TMP_Text>();
        newMessage.textObject.text = newMessage.text;

        messageList.Add(newMessage);
    
    }

    public void ClearChat()
    {
        messageList.Clear();
        GameObject[] chat = GameObject.FindGameObjectsWithTag("ChatMessage");
        foreach(GameObject chit in chat)
        {
            Destroy(chit);
        }
        Debug.Log("Clear Chat");
    }


    private void Update()
    {
        if(inputField.text != "")
        {

            if(Input.GetKeyDown(KeyCode.Return))
            {
                if (inputField.text == " ")
                {
                    inputField.text = "";
                    inputField.DeactivateInputField();
                    return;
                }
                NetworkTransmission.instance.IWishToSendAChatServerRPC(inputField.text, myClientId);
                inputField.text = "";
            }
            
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                inputField.ActivateInputField();
                inputField.text = " ";
            }
        }
    }

    public void HostCreated()
    {
        multiMenu.SetActive(false);
        multiLobby.SetActive(true);
        isHost = true;
        connected = true;
    }

    public void ConnectedAsClient()
    {
        multiMenu.SetActive(false);
        multiLobby.SetActive(true);
        isHost = false;
        connected = true;
    }

    //접속해제
    public void Disconnected()
    {
        playerInfo.Clear();
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        foreach(GameObject card in playercards)
        {
            Destroy (card);
        }

        multiMenu.SetActive(true);
        multiLobby.SetActive(false);
        isHost = false;
        connected = false;
    }

    public void AddPlayerToDictionary(ulong _clientId, string _steamName, ulong _steamId)
    {
        if (!playerInfo.ContainsKey(_clientId))
        {
            PlayerInfo _pi = Instantiate(playerCardPrefab,playerFieldBox.transform).GetComponent<PlayerInfo>();
            _pi.steamId = _steamId;
            _pi.steamName = _steamName;
            playerInfo.Add(_clientId, _pi.gameObject);
        }
    }

    public void UpdateClients()
    {
        foreach(KeyValuePair<ulong,GameObject> _player in playerInfo)
        {
            ulong _steamId = _player.Value.GetComponent<PlayerInfo>().steamId;
            string _steamName = _player.Value.GetComponent<PlayerInfo>().steamName;
            ulong _clientid = _player.Key;

            NetworkTransmission.instance.UpdateClientsPlayerInfoClientRPC(_steamId, _steamName, _clientid);
        }
    }

    public void RemovePlayerFromDictionary(ulong _steamId)
    {
        GameObject _value = null;
        ulong _key = 100;

        foreach (KeyValuePair<ulong, GameObject> _player in playerInfo)
        {
            if(_player.Value.GetComponent<PlayerInfo>().steamId == _steamId)
            {
                _value = _player.Value;
                _key = _player.Key;
            }
        }
        if(_key != 100)
        {
            playerInfo.Remove(_key);
        }
        if(_value != null)
        {
            Destroy(_value);
        }
    }

    public void ReadyButton(bool _ready)
    {
        NetworkTransmission.instance.IsTheClientReadyServerRPC(_ready, myClientId);
    }

    public void GameStart()
    {
        //1.로비를 닫아 더 이상 다른 플레이어가 들어오지 못하게 한다.
        //RPC로 모든 클라이언트에게 로비에서 나가라고 보내기

        //2.각 피어가 서로 통신을 시작하여 동기화를 맞춘다. (이미 되있는듯)
        
        //3.게임 플레이 중 상태에 진입하면 모두 로비를 떠난다.

        //각 피어와 통신 시작

        //게임 시작 
        NetworkTransmission.instance.SessionStartServerRPC();
    }

    public bool CheckIfPlayersAreReady()
    {
       bool _ready = false;
        foreach(KeyValuePair<ulong, GameObject> _player in playerInfo)
        {
            if (!_player.Value.GetComponent<PlayerInfo>().isReady)
            {
                startButton.SetActive(false);
                return false;
            }
            else
            {
                startButton.SetActive(true);
                _ready = true;
            }
        }
        return true;
    }


    public void Quit()
    {
        Application.Quit();
    }
}
