using UnityEngine;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;

public class GameNetworkManager : MonoBehaviour
{
    //게임 네트워크에 대한 이벤트 관리
    public static GameNetworkManager instance { get; private set; } = null;

    private FacepunchTransport transport = null;

    public Lobby? currentLobby { get; private set; } = null;

    public ulong hostId;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        transport = GetComponent<FacepunchTransport>();

        //로비
        SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
        SteamFriends.OnGameOverlayActivated += SteamFriends_OnGameOverlayActivated;

        //인게임
        SteamNetworking.OnP2PSessionRequest += SteamNetworking_OnP2PSessionRequest;

        
        
    }

    //다른 피어에서 통신이 들어올 때마다 콜백. 
    private void SteamNetworking_OnP2PSessionRequest(SteamId steamId)
    {
        Debug.Log("세션 연결요청 들어옴");
        
        //계속 응답을 해줘야 세션연결이 유지됨.
        SteamNetworking.AcceptP2PSessionWithUser(steamId);
    }

   
    private void SteamFriends_OnGameOverlayActivated(bool obj)
    {
        throw new System.NotImplementedException();
    }

    private void OnDestroy()
    {
        //로비
        SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= SteamMatchmaking_OnLobbyInvite;
        SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmaking_OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;

        //인게임
        SteamNetworking.OnP2PSessionRequest -= SteamNetworking_OnP2PSessionRequest;


        if (NetworkManager.Singleton == null)
        {
            return;
        }
        NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
    }

    private void OnApplicationQuit()
    {
        Disconnected();
    }

    //친구가 로비에 참가하려고 할 때
    private  async void SteamFriends_OnGameLobbyJoinRequested(Lobby _lobby, SteamId _steamId)
    {
        RoomEnter joinedLobby = await _lobby.Join();
        if (joinedLobby != RoomEnter.Success)
        {
            Debug.Log("Failed to create lobby");
        }
        else
        {
            currentLobby = _lobby;
            GameManager.Instance.ConnectedAsClient();
            Debug.Log("Joined Lobby");
        }
    }

    private void SteamMatchmaking_OnLobbyGameCreated(Lobby _lobby, uint _ip, ushort _port, SteamId steamId)
    {
        Debug.Log("Lobby was Created");
        GameManager.Instance.SendMessageToChat($"Lobby was created", NetworkManager.Singleton.LocalClientId,true);
        NetworkTransmission.instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, SteamClient.Name,NetworkManager.Singleton.LocalClientId);

    }

    //friend send you an steam invite
    //내가 친구를 로비에 초대했을 때 친구 클라에서 호출됨
    private void SteamMatchmaking_OnLobbyInvite(Friend _steamId, Lobby _lobby)
    {
        Debug.Log($"Invite from {_steamId.Name}");
    }

    private void SteamMatchmaking_OnLobbyMemberLeave(Lobby _lobby, Friend _steamId)
    {
        Debug.Log("Member Leave");
        GameManager.Instance.SendMessageToChat($"{_steamId.Name} has left", _steamId.Id, true);
        NetworkTransmission.instance.RemoveMeFromDictionaryServerRPC(_steamId.Id);
    }

    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby _lobby, Friend _steamId)
    {
        Debug.Log("Member join");
    }

    private void SteamMatchmaking_OnLobbyCreated(Result _result, Lobby _lobby)
    {
      if(_result!= Result.OK)
        {
            Debug.Log("Lobby was not created");
            return;
        }

        //로비 가입 가능하게 설정
        _lobby.SetPublic();
        _lobby.SetJoinable(true);
        _lobby.SetGameServer(_lobby.Owner.Id);
        Debug.Log($"lobby created {_lobby.Owner.Name}");
    }

    private void SteamMatchmaking_OnLobbyEntered(Lobby _lobby)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            return;
        }
        StartClient(currentLobby.Value.Owner.Id);

        //초대받은 클라이언트는 facepunchTransport의 TargetsteamId에 초대한 클라이언트의 Id가 있어야 한다.
        //어...아닌듯? 차피 로비에 들어가면 전부다  StartClient를 호출하고, facepunchTransport의 TargetsteamId는 모두 현재 로비 소유자 (아마도 호스트)로 설정하기 때문이다.
        //흠...
    }

    public async void StartHost(int _maxMembers)
    {
        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
        NetworkManager.Singleton.StartHost();
        GameManager.Instance.myClientId = NetworkManager.Singleton.LocalClientId;
        currentLobby = await SteamMatchmaking.CreateLobbyAsync(_maxMembers);
        
    }

    public void StartClient(SteamId _sId)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
        transport.targetSteamId = _sId;
        GameManager.Instance.myClientId = NetworkManager.Singleton.LocalClientId;
        if (NetworkManager.Singleton.StartClient()) 
        {
            Debug.Log("Client has started");
        }
    }

    public void Disconnected()
    {
        currentLobby?.Leave();
        if(NetworkManager.Singleton == null)
        {
            return;
        }
        if(NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
        }
        else
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        }

        NetworkManager.Singleton.Shutdown(true);
        GameManager.Instance.ClearChat();
        GameManager.Instance.Disconnected();
        Debug.Log("disconnected");
    }



    private void Singleton_OnClientDisconnectCallback(ulong _clientId)
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
        if(_clientId == 0)
        {
            Disconnected();
        }
        //throw new System.NotImplementedException();
    }

    private void Singleton_OnClientConnectedCallback(ulong _clientId)
    {
        NetworkTransmission.instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, SteamClient.Name, _clientId);
        GameManager.Instance.myClientId = _clientId;
        NetworkTransmission.instance.IsTheClientReadyServerRPC(false, _clientId);
        Debug.Log($"Client has connected : {_clientId}");
        //throw new System.NotImplementedException();
    }

    private void Singleton_OnServerStarted()
    {
        Debug.Log("Host started");
        GameManager.Instance.HostCreated();
    }
}
