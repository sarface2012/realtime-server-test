using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Amazon;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Aws.GameLift.Realtime.Types;

public class LobbyUI : MonoBehaviour
{
    GameObject gameobject;
    Lobby lobby;

    public Text RoomNameText;
    public Text PlayerSessionIDText;

    void Start()
    {
        GameObject go = GameObject.Find("Lobby");
        lobby = go.GetComponent<Lobby>();
    }
    public void OnCreateRoomButtonClick()
    {
        Debug.Log("Createroom Button click!");
        var roomName = lobby.CreateRoom();
        Debug.Log("roomName: " + roomName);
        RoomNameText.text  = "RoomName: " + roomName.ToString();
    }

    public void OnSearchRoomButtonClick()
    {
        Debug.Log("Search Button click!");

        var sessions = lobby.SearchRooms();
        var session  = lobby.findByMostUserRoomSession(sessions);
        
        Debug.Log("RoomSession" + session);
        var pSession = lobby.JoinRoom(session);

        PlayerSessionIDText.text = "PlayerSessionID: " + pSession.ToString();
    }

    public void OnSendMsgTest1ButtonClick()
    {
        Debug.Log("SendMsg1 click!");
        lobby.SendMsg();
        Debug.Log(lobby.msg);
    }

}
