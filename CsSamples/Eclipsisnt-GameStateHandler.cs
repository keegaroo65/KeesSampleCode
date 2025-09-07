using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameStateHandler : NetworkBehaviour
{
    #region Static Variables

    public static GameStateHandler main;

    #endregion

    #region Instance Variables

    public List<PlayerConnection> players;
    public List<PlayerConnection> inactivePlayers;
    public NetworkManager networkManager;

    public Transform PlayerConnectionsObject;

    public Team defaultTeam;

    // Used as alternatives from the NetworkManager versions to ensure that PlayerConnection objects are available
    public event Action<PlayerConnection> OnPlayerConnected;
    public event Action<PlayerConnection> OnPlayerDisconnecting;

    //public event Action OnServerBegan;

    #endregion

    #region MonoBehaviour Implementation

    // Singleton w/ MonoBehaviour
    private void Awake()
    {
        main = this;
        players = new List<PlayerConnection>();
        inactivePlayers = new List<PlayerConnection>();

        PlayerConnectionsObject = GameObject.Find("PlayerConnections").transform;

        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();

        networkManager.ConnectionApprovalCallback = ApprovalCheck;
        networkManager.OnClientConnectedCallback += ConnectClient;
        networkManager.OnClientDisconnectCallback += DisconnectClient;

        // Set up Construction, as many structures rely on it.
        Construction.main = GameObject.Find("Construction").GetComponent<Construction>();
        Construction.Initialize();

        // Set up debugging settings
        EclipsisntDebug.Initialize();

        Entity.Initialize();
        IridiumGrid.Initialize();
        PortafabUtility.Initialize();
        Team.Initialize();

        // Add a callback for when the NetworkManager Server is started
        networkManager.OnServerStarted += () => { OnServerStarted(); };
    }

    private void OnServerStarted()
    {
        defaultTeam = Team.NewTeam();

        //yield return new WaitForSeconds(10f); // TODO: I don't like having to defer execution to make this work properly
        Construction.main.BuildSceneBuildings();

        //OnServerBegan?.Invoke();
    }

    #endregion

    #region NetworkBehaviour Implementation



    #endregion

    #region Public Methods

    public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = false;
    }

    private PlayerConnection NewPlayerConnection(ulong clientId, Player player)
    {
        GameObject newGameObject = Construction.main.InstantiateNewPlayerConnection();
        newGameObject.GetComponent<NetworkObject>().Spawn(); // look into Server->Client RPCs

        newGameObject.transform.parent = PlayerConnectionsObject;
        newGameObject.name = "newconn" + clientId.ToString();

        PlayerConnection connection = newGameObject.GetComponent<PlayerConnection>();

        connection.Setup(clientId, player, defaultTeam);

        return connection;
    }

    private void ConnectClient(ulong clientId)
    {
        if (!networkManager.IsServer) return;

        GameObject playerObject = new GameObject();

        if (playerReconnecting(clientId)) ReconnectClient(clientId);
        else players.Add(NewPlayerConnection(clientId, null));

        PlayerConnection connection = ConnectionFromClientId(clientId);
        
        connection.Respawn();

        OnPlayerConnected?.Invoke(connection);
    }

    private void ReconnectClient(ulong clientId)
    {
        ConnectionFromClientId(clientId).connected = true;
    }

    private void DisconnectClient(ulong clientId)
    {
        if (!networkManager.IsServer && networkManager.DisconnectReason != string.Empty)
        {
            Debug.Log($"Approval Declined Reason: {networkManager.DisconnectReason}");
        }
        else if (networkManager.IsServer)
        {
            PlayerConnection connection = ConnectionFromClientId(clientId);

            OnPlayerDisconnecting?.Invoke(connection);

            //connection.Despawn();

            players.Remove(connection);
            inactivePlayers.Add(connection);
        }
    }

    [Rpc(SendTo.Server)]
    public void RespawnRpc(RpcParams rpcParams = default)
    { // Static to prevent players from being able to respawn other players
        ConnectionFromClientId(rpcParams.Receive.SenderClientId).Respawn();
    }



    #endregion

    #region Private Methods

    public PlayerConnection ConnectionFromClientId(ulong clientId)
    {
        foreach (PlayerConnection connection in players)
        {
            if (connection.clientId == clientId) return connection;
        }

        foreach (PlayerConnection connection in inactivePlayers)
        {
            if (connection.clientId == clientId) return connection;
        }

        return null;
    }

    public PlayerConnection LocalPlayerConnection()
    {
        ulong clientId = networkManager.LocalClientId;

        foreach (PlayerConnection connection in PlayerConnectionsObject.GetComponentsInChildren<PlayerConnection>())
        {
            //Debug.Log($"Checking {connection} for {clientId}");
            if (connection.clientId == clientId) return connection;
        }

        return null;
    }

    #endregion

    #region Utility Struct

    //public struct PlayerConnection
    //{
    //    public Player player;
    //    public ulong clientId;
    //    public bool connected;

    //    public PlayerConnection(Player _player, ulong _clientId, bool _connected = true)
    //    {
    //        player = _player;
    //        clientId = _clientId;
    //        connected = _connected;
    //    }
    //}

    private bool playerReconnecting(ulong clientId)
    {
        foreach (PlayerConnection connection in inactivePlayers)
        {
            if (connection.clientId == clientId) return true;
        }

        return false;
    }

    #endregion
}
