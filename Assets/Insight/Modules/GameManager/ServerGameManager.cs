﻿using Insight;
using System.Collections.Generic;
using UnityEngine;

// Idle - GameServer just started. It has not been assigned a scene. It has no running NetworkManager
// Active - GameServer was provided a scene and its NetworkManager is now running. Players can connect.
// Done - Game session is now complete. Marked to either be destroyed or returned to a pool.
public enum GameServerState { Idle, Active, Done}

public class ServerGameManager : InsightModule
{
    InsightServer server;
    MasterSpawner masterSpawner;

    public List<GameContainer> registeredGameServers = new List<GameContainer>();

    public void Awake()
    {
        AddDependency<MasterSpawner>();
    }

    public override void Initialize(InsightServer insight, ModuleManager manager)
    {
        server = insight;
        masterSpawner = manager.GetModule<MasterSpawner>();
        RegisterHandlers();

        server.transport.OnServerDisconnected.AddListener(HandleDisconnect);
    }

    void RegisterHandlers()
    {
        server.RegisterHandler((short)MsgId.RegisterGame, HandleRegisterGameMsg);
    }

    private void HandleRegisterGameMsg(InsightNetworkMessage netMsg)
    {
        RegisterGameMsg message = netMsg.ReadMessage<RegisterGameMsg>();

        if (server.logNetworkMessages) { Debug.Log("Received GameRegistration request"); }

        registeredGameServers.Add(new GameContainer() { connectionId = netMsg.connectionId, uniqueId = message.UniqueID, NetworkAddress = message.NetworkAddress, NetworkPort = message.NetworkPort});
    }

    private void HandleDisconnect(int connectionId)
    {
        foreach (GameContainer game in registeredGameServers)
        {
            if (game.connectionId == connectionId)
            {
                registeredGameServers.Remove(game);
                return;
            }
        }
    }

    public List<GameContainer> GetActiveGameServers()
    {
        List<GameContainer> activeGames = new List<GameContainer>();
        foreach(GameContainer game in registeredGameServers)
        {
            if(game.state == GameServerState.Active)
            {
                activeGames.Add(game);
            }
        }
        return activeGames;
    }
}

public struct GameContainer
{
    public string NetworkAddress;
    public ushort NetworkPort;
    public string uniqueId;
    public int connectionId;
    public GameServerState state;
    public Dictionary<string, string> Properties;
}
