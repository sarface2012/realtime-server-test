﻿using System;
using System.Text;
using Aws.GameLift.Realtime;
using Aws.GameLift.Realtime.Event;
using Aws.GameLift.Realtime.Types;

public class RealTimeClient
{
    public Aws.GameLift.Realtime.Client Client { get; private set; }

    public Action<object, DataReceivedEventArgs> OnDataReceivedCallback { get; set; }
    // An opcode defined by client and your server script that represents a custom message type
    public static class OpCode
    {
        public const int SendTest1 = 10;
        public const int SendTest2 = 11;

        public const int RecieveTest1 = 31;
        public const int RecieveTest2 = 32;

    }

    /// Initialize a client for GameLift Realtime and connect to a player session.
    /// <param name="endpoint">The DNS name that is assigned to Realtime server</param>
    /// <param name="remoteTcpPort">A TCP port for the Realtime server</param>
    /// <param name="listeningUdpPort">A local port for listening to UDP traffic</param>
    /// <param name="connectionType">Type of connection to establish between client and the Realtime server</param>
    /// <param name="playerSessionId">The player session ID that is assigned to the game client for a game session </param>
    /// <param name="connectionPayload">Developer-defined data to be used during client connection, such as for player authentication</param>
    public RealTimeClient(string endpoint, int remoteTcpPort, int listeningUdpPort, ConnectionType connectionType,
                 string playerSessionId, byte[] connectionPayload)
    {
        // Create a client configuration to specify a secure or unsecure connection type
        // Best practice is to set up a secure connection using the connection type RT_OVER_WSS_DTLS_TLS12.
        ClientConfiguration clientConfiguration = new ClientConfiguration()
        {
            // C# notation to set the field ConnectionType in the new instance of ClientConfiguration
            ConnectionType = connectionType
        };

        // Create a Realtime client with the client configuration
        Client = new Client(clientConfiguration);

        // Initialize event handlers for the Realtime client
        Client.ConnectionOpen += OnOpenEvent;
        Client.ConnectionClose += OnCloseEvent;
        Client.GroupMembershipUpdated += OnGroupMembershipUpdate;
        Client.DataReceived += OnDataReceived;

        // Create a connection token to authenticate the client with the Realtime server
        // Player session IDs can be retrieved using AWS SDK for GameLift
        ConnectionToken connectionToken = new ConnectionToken(playerSessionId, connectionPayload);

        // Initiate a connection with the Realtime server with the given connection information
        Client.Connect(endpoint, remoteTcpPort, listeningUdpPort, connectionToken);
    }

    public void Disconnect()
    {
        if (Client.Connected)
        {
            Client.Disconnect();
        }
    }

    public bool IsConnected()
    {
        return Client.Connected;
    }

    /// <summary>
    /// Example of sending to a custom message to the server.
    ///
    /// Server could be replaced by known peer Id etc.
    /// </summary>
    /// <param name="intent">Choice of delivery intent ie Reliable, Fast etc. </param>
    /// <param name="payload">Custom payload to send with message</param>
    public void SendMessage(DeliveryIntent intent, string payload)
    {
        UnityEngine.Debug.Log("SendMessage");
        Client.SendMessage(Client.NewMessage(OpCode.SendTest1)
            .WithDeliveryIntent(intent)
            .WithTargetPlayer(Constants.PLAYER_ID_SERVER)
            .WithPayload(StringToBytes(payload)));
    }

    /**
     * Handle connection open events
     */
    public void OnOpenEvent(object sender, EventArgs e)
    {
        UnityEngine.Debug.Log("OnOpenEvent");
    }

    /**
     * Handle connection close events
     */
    public void OnCloseEvent(object sender, EventArgs e)
    {
        UnityEngine.Debug.Log("OnCloseEvent");
    }

    /**
     * Handle Group membership update events
     */
    public void OnGroupMembershipUpdate(object sender, GroupMembershipEventArgs e)
    {
        UnityEngine.Debug.Log("OnGroupMembershipUpdate");
    }

    /**
     *  Handle data received from the Realtime server
     */
    public virtual void OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        //
        UnityEngine.Debug.Log("OnDataReceived");
        UnityEngine.Debug.Log($"OpCode = {e.OpCode}");
        switch (e.OpCode)
        {
            // handle message based on OpCode
            default:
                break;
        }

        if (OnDataReceivedCallback != null) OnDataReceivedCallback(sender, e);
    }

    /**
     * Helper method to simplify task of sending/receiving payloads.
     */
    public static byte[] StringToBytes(string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    /**
     * Helper method to simplify task of sending/receiving payloads.
     */
    public static string BytesToString(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    public void SendEvent(int opCode)
    {
        UnityEngine.Debug.Log("SendEvent");
        if (!IsConnected()) return;
        Client.SendEvent(opCode);
    }
}