using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UI;
using Amazon;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Aws.GameLift.Realtime.Types;

public class Lobby : MonoBehaviour
{
  class GameLiftConfig
  {
    public RegionEndpoint RegionEndPoint { get; set; }
    public string AccessKeyId { get; set; }
    public string SecretAccessKey { get; set; }
    public string GameLiftAliasId { get; set; }
  }
  GameLiftConfig config;
  AmazonGameLiftClient gameLiftClient;
  public RealTimeClient realTimeClient;

  public string msg;

  void Start()
  {
    initialize();
  }

  void initialize()
  {
    config = new GameLiftConfig
    {
      RegionEndPoint  = RegionEndpoint.APNortheast1, //Tokyo
      AccessKeyId     = [AccessKeyId],
      SecretAccessKey = [SecretAccessKey]
      GameLiftAliasId = [GameLiftAliasId]
    };

    gameLiftClient = new AmazonGameLiftClient(config.AccessKeyId, config.SecretAccessKey, config.RegionEndPoint);
  }

  public string CreateRoom()
  {
    UnityEngine.Debug.Log("CreateGameSession Start!");

    var roomName = Guid.NewGuid().ToString();

    var request = new CreateGameSessionRequest
    {
      AliasId = config.GameLiftAliasId,
      MaximumPlayerSessionCount = 10,
      Name = roomName
    };

    // https://docs.aws.amazon.com/gamelift/latest/apireference/API_CreateGameSession.html
    var response = gameLiftClient.CreateGameSession(request);
    UnityEngine.Debug.Log("GameSessionStatus: " + response.GameSession.Status.ToString());

    return response.GameSession.Name.ToString();
  }

  public List<GameSession> SearchRooms()
  {
        UnityEngine.Debug.Log("SearchRooms");
        var response = gameLiftClient.SearchGameSessions(new SearchGameSessionsRequest
        {
            AliasId = config.GameLiftAliasId,
        });

        return response.GameSessions;
  }

  public (string, string) findByMostUserRoomSession(List<GameSession> sessions)
  {
    string sessionId = "";
    string roomName = "";
    int playerCount = 0;
    for (int i = 0; i < sessions.Count; i++)
    {
      UnityEngine.Debug.Log("GameSessionID: " + sessions[i].GameSessionId);
      UnityEngine.Debug.Log("Name: " + sessions[i].Name);
      if (sessions[i].CurrentPlayerSessionCount >= playerCount)
      {
        sessionId = sessions[i].GameSessionId;
        roomName  = sessions[i].Name;
        playerCount = sessions[i].CurrentPlayerSessionCount;
      }
    }

    return (sessionId, roomName);
  }

  public string JoinRoom(string sessionId)
  {
    UnityEngine.Debug.Log("CreatePlayerSession Start!");
    UnityEngine.Debug.Log("SessionID:" + sessionId);

    var response = gameLiftClient.CreatePlayerSession(new CreatePlayerSessionRequest
    {
            GameSessionId = sessionId,
            PlayerId = SystemInfo.deviceUniqueIdentifier,
    });

    var playerSession = response.PlayerSession;
    UnityEngine.Debug.Log("playerSession: " + playerSession);

    ushort DefaultUdpPort = 7777;
    var udpPort = SearchAvailableUdpPort(DefaultUdpPort, DefaultUdpPort + 100);

    realTimeClient = new RealTimeClient(
      playerSession.IpAddress,
      playerSession.Port,
      udpPort,
      ConnectionType.RT_OVER_WS_UDP_UNSECURED,
      playerSession.PlayerSessionId,
      null
    );

    realTimeClient.OnDataReceivedCallback = OnDataReceivedCallback;
    return playerSession.PlayerSessionId.ToString();
  }

  public void SendMsg()
  {
      if (realTimeClient != null) realTimeClient.SendMessage(DeliveryIntent.Reliable, "test");
  }

    public void OnDataReceivedCallback(object sender, Aws.GameLift.Realtime.Event.DataReceivedEventArgs e)
    {
        UnityEngine.Debug.Log("RecevidCallback");
        msg = "Hi! / " + DateTime.Now.ToLongTimeString();
    }

  int SearchAvailableUdpPort(int from = 1024, int to = ushort.MaxValue)
    {
        from = Mathf.Clamp(from, 1, ushort.MaxValue);
        to = Mathf.Clamp(to, 1, ushort.MaxValue);

        UnityEngine.Debug.Log("from: " + from);
        UnityEngine.Debug.Log("to: " + to);

        // Don't work, for macOS
        //var set = GetActiveUdpPorts();
        var set = LsofUdpPorts(from, to);

        for (int port = from; port <= to; port++)
            if (!set.Contains(port))
                return port;
        return -1;
    }

    HashSet<int> LsofUdpPorts(int from, int to)
    {
        var set = new HashSet<int>();
        string command = string.Join(" | ",
            $"lsof -nP -iUDP:{from.ToString()}-{to.ToString()}",
            "sed -E 's/->[0-9.:]+$//g'",
            @"grep -Eo '\d+$'");
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{command}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
        });
        if (process != null)
        {
            process.WaitForExit();
            var stream = process.StandardOutput;
            while (!stream.EndOfStream)
                if (int.TryParse(stream.ReadLine(), out int port))
                    set.Add(port);
        }
        return set;
    }

    HashSet<int> GetActiveUdpPorts()
    {
      return new HashSet<int>(IPGlobalProperties.GetIPGlobalProperties()
        .GetActiveUdpListeners().Select(listener => listener.Port));
    }
}
