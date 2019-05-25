using System;
using System.Collections.Generic;
using UnityEngine;

namespace Insight
{
    public class ServerAuthentication : InsightModule
    {
        InsightServer server;

        public Dictionary<int, UserContainer> registeredUsers = new Dictionary<int, UserContainer>();

        public override void Initialize(InsightServer server, ModuleManager manager)
        {
            this.server = server;

            RegisterHandlers();

            server.transport.OnServerDisconnected.AddListener(HandleDisconnect);
        }

        void RegisterHandlers()
        {
            server.RegisterHandler((short)MsgId.Login, HandleLoginMsg);
        }

        //This is just an example. No actual authentication happens.
        //You would need to replace with your own logic. Perhaps with a DB connection.
        void HandleLoginMsg(InsightNetworkMessage netMsg)
        {
            LoginMsg message = netMsg.ReadMessage<LoginMsg>();

            if (server.logNetworkMessages) { Debug.Log("[Authentication] - Login Received: " + message.AccountName + " / " + message.AccountPassword); }

            //Login Sucessful
            if (true) //Put your DB logic here
            {
                string UniqueId = Guid.NewGuid().ToString();

                registeredUsers.Add(netMsg.connectionId, new UserContainer()
                {
                    username = message.AccountName,
                    uniqueId = UniqueId,
                    connectionId = netMsg.connectionId
                });

                netMsg.Reply((short)MsgId.LoginResponse, new LoginResponseMsg()
                {
                    Authenticated = true,
                    UniqueID = UniqueId
                });
            }

            //Login Failed. Unreachable code currently as there is no real auth happening.
            //else
            //{
            //    netMsg.Reply((short)MsgId.LoginResponse, new LoginResponseMsg()
            //    {
            //        Authenticated = false
            //    });
            //}
        }

        void HandleDisconnect(int connectionId)
        {
            registeredUsers.Remove(connectionId);
        }

        public UserContainer GetUserByConnection(int connectionId)
        {
            return registeredUsers[connectionId];
        }
    }

    [Serializable]
    public class UserContainer
    {
        public string uniqueId;
        public string username;
        public int connectionId;
    }
}
