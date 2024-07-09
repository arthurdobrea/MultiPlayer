using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestLoby : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartbeatTimer;
    private string playerName;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName = "StickMan" + Random.Range(10, 99);
        Debug.Log(playerName);
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartBeatTimerMax = 15f;
                heartbeatTimer = heartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    [ServerRpc]
    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                Player = getPLayer()
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            hostLobby = lobby;
            Debug.Log("Created Lobby! " + lobby.Name + "  " + lobby.MaxPlayers);
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ServerRpc]
    public async void ListLobbies()
    {
        try
        {
            QueryResponse queryLobbiesAsync = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies found: " + queryLobbiesAsync.Results.Count);

            foreach (Lobby lobby in queryLobbiesAsync.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
                PrintPlayers(lobby);
            }
            
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ServerRpc]
    public async void JoinLobby()
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = getPLayer()
            };
            QueryResponse queryLobbiesAsync = await Lobbies.Instance.QueryLobbiesAsync();
            Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(queryLobbiesAsync.Results[0].Id, joinLobbyByIdOptions);
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [ServerRpc]
    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = getPLayer()
            };
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private Player getPLayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }
    public void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in lobby" + lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }
}