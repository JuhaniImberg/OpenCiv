using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Manager : Photon.MonoBehaviour
{
    public float version = 1.0f;
    public string guistate = "connecting";
    public bool menu = false;
    public float presetResolution_width = 1280, presetResolution_height = 720;
    public GUISkin skin;

    private float reconnectDelay = 0f, reconnectTime = 0f;
    private string chatMessage = "";
    List<string> chatMessages = new List<string>(11);

    void Start()
    {
        StartCoroutine("Connect");
        Debug.Log(chatMessages.Capacity);
    }

    void Update()
    {

    }

    void OnGUI()
    {
        GUI.skin = skin;
        Vector3 scale = new Vector3((float)Screen.width / presetResolution_width, (float)Screen.height / presetResolution_height, 1);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        switch (guistate)
        {
            case "waitingtoreconnect":
                GUILayout.Label("Attempting to connect in: " + (reconnectTime - Time.time));
                break;
            case "connecting":
                GUILayout.Label("Connecting");
                break;
            case "connected":
                GUILayout.BeginArea(new Rect(0, 0, 1280, 720));
                GUILayout.BeginHorizontal();
                GUILayout.Box("Games", GUILayout.Width(900), GUILayout.Height(720));
                GUILayout.BeginArea(new Rect(10, 10, 900, 720));
                if (PhotonNetwork.GetRoomList().Length > 0)
                {
                    foreach (RoomInfo room in PhotonNetwork.GetRoomList())
                    {
                        GUILayout.Label(room.name + " " + room.playerCount + "/" + room.maxPlayers);
                    }
                }
                else
                    GUILayout.Label("There are no games available.");
                GUILayout.EndArea();
                GUILayout.BeginVertical();
                GUILayout.Box("", GUILayout.Width(380), GUILayout.Height(720));
                GUILayout.BeginArea(new Rect(910, 10, 370, 710));
                GUILayout.BeginVertical();
                if (GUILayout.Button("New game"))
                {
                    PhotonNetwork.CreateRoom(null, true, true, 20);
                    guistate = "room";
                }
                if (GUILayout.Button("Join game"))
                {

                }
                if (PhotonNetwork.GetRoomList().Length > 0)
                {
                    if (GUILayout.Button("Join random room"))
                    {
                        PhotonNetwork.JoinRandomRoom();
                        guistate = "room";
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                GUILayout.EndHorizontal();
                break;
            case "room":
                GUILayout.BeginArea(new Rect(0, 0, 1280, 720));
                GUILayout.BeginHorizontal();
                GUILayout.Box("Players", GUILayout.Width(900), GUILayout.Height(720));
                GUILayout.BeginVertical();
                GUILayout.Box("", GUILayout.Width(380), GUILayout.Height(320));
                GUILayout.Box("", GUILayout.Width(380), GUILayout.Height(360));
                GUILayout.EndVertical();
                GUILayout.BeginArea(new Rect(910, 330, 380, 360));
                GUILayout.BeginVertical();
                for (int i = 0; i < chatMessages.Count; i++)
                {
                    GUI.Label(new Rect(0, 340 - i * skin.customStyles[0].fontSize - skin.customStyles[0].fontSize, 380, skin.customStyles[0].fontSize), chatMessages[i], skin.customStyles[0]);
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
                chatMessage = GUI.TextArea(new Rect(900, 690, 380, 30), chatMessage);
                if (chatMessage.Contains("\n"))
                {
                    if (chatMessage.Length > 1)
                    {
                        if (chatMessage.StartsWith("/name"))
                        {
                            chatMessage = chatMessage.Replace("/name", "");
                            chatMessage = chatMessage.Trim();
                            PhotonNetwork.playerName = chatMessage;
                            chatMessage = "";
                        }
                        else
                        {
                            chatMessage.Trim();
                            photonView.RPC("Message", PhotonTargets.All, chatMessage);
                            chatMessage = "";
                        }
                    }
                    else
                    {
                        chatMessage = "";
                    }
                }
                GUILayout.EndArea();
                break;
        }
    }

    IEnumerator Connect()
    {
        while (true)
        {
            if (reconnectTime <= Time.time && PhotonNetwork.connectionState == ConnectionState.Disconnected)
            {
                PhotonNetwork.ConnectUsingSettings(version.ToString());
                guistate = "connecting";
            }
            yield return null;
        }
    }

    void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        reconnectDelay++;
        reconnectTime = Time.time + reconnectDelay;
        guistate = "waitingtoreconnect";
    }

    void OnJoinedLobby()
    {
        StopCoroutine("Connect");
        guistate = "connected";
    }

    [RPC]
    void Message(string message, PhotonMessageInfo info)
    {
        message.Trim();
        if (chatMessages.Count > 0)
        {
            if (chatMessages.Capacity >= chatMessages.Count)
            {
                for (int i = chatMessages.Count - 1; i > 0; i--)
                {
                    chatMessages[i] = chatMessages[i - 1];
                }
                chatMessages.Insert(0,info.sender.name + ": " + message);
            }
            else
            {
                for (int i = chatMessages.Count - 1; i > 0; i--)
                {
                    chatMessages[i] = chatMessages[i - 1];
                }
                chatMessages.RemoveAt(0);
                chatMessages.Insert(0, info.sender.name + ": " + message);
            }
        }
        else
        {
            chatMessages.Add(info.sender.name + ": " + message);
        }
    }
}
