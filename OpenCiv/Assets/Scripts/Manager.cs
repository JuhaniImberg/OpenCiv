using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Manager : Photon.MonoBehaviour
{
    public float version = 1.0f;
    public string guistate = "name";
    public bool menu = false, settings = false;
    public float presetResolution_width = 1280, presetResolution_height = 720;
    public GUISkin skin;
    public Texture2D kickIcon;
    public string playerName = "";

    private float reconnectDelay = 0f, reconnectTime = 0f;
    private string chatMessage = "";
    Stack<string> chatMessages;
    RoomInfo selectedRoom = null;

    void Start()
    {
        Application.runInBackground = true;
        chatMessages = new Stack<string>(Mathf.RoundToInt(360 / skin.customStyles[0].fontSize));
    }

    void Update()
    {

    }

    void OnGUI()
    {
        GUI.skin = skin;
        Vector3 scale = new Vector3((float)Screen.width / presetResolution_width, (float)Screen.height / presetResolution_height, 1);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

        if (menu)
        {
            GUILayout.BeginArea(new Rect(500, 110, 280, 500));
            GUILayout.Box("Menu", GUILayout.Width(280), GUILayout.Height(500));
            GUILayout.BeginArea(new Rect(10, 22, 260, 478));
            GUILayout.BeginVertical();
            if (GUILayout.Button(""))
            {

            }
            if (GUILayout.Button(""))
            {

            }
            if (GUILayout.Button(""))
            {

            }
            if (GUILayout.Button(""))
            {

            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
            GUILayout.EndArea();
        }
        if (settings)
        {
            GUILayout.BeginArea(new Rect(140, 110, 1000, 500));
            GUILayout.Box("Settings", GUILayout.Width(1000), GUILayout.Height(500));
            GUILayout.BeginArea(new Rect(10, 22, 980, 478));
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Username");
            string s = playerName;
            s = GUILayout.TextField(s, 30, GUILayout.Width(100));
            playerName = s;
            GUILayout.EndVertical();
            if (GUILayout.Button("Ready!", GUILayout.Width(100)))
            {
                SaveSettings();
                settings = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUILayout.EndArea();
        }

        switch (guistate)
        {
            case "name":
                GUI.Box(new Rect(500,250,280,80),"Enter your name");
                GUILayout.BeginArea(new Rect(500, 272, 280, 80));
                GUILayout.BeginVertical();
                playerName = GUILayout.TextField(playerName, 30, GUILayout.Width(280));
                if (GUILayout.Button("Ready!"))
                {
                    PhotonNetwork.playerName = playerName;
                    guistate = "connecting";
                    PlayerPrefs.SetString("name", playerName);
                    StartCoroutine("Connect");
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
                break;
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
                GUILayout.BeginArea(new Rect(10, 22, 880, 698));
                if (PhotonNetwork.GetRoomList().Length > 0)
                {
                    RoomInfo[] rooms = PhotonNetwork.GetRoomList();
                    int s = -1;
                    s = GUILayout.SelectionGrid(s, CreateList(rooms), 1, GUILayout.Width(880));
                    if (s > -1)
                    {
                        selectedRoom = rooms[s];
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
                    Message("Created a new game", null);
                }
                if (selectedRoom != null)
                {
                    if (GUILayout.Button("Join game"))
                    {
                        PhotonNetwork.JoinRoom(selectedRoom.name);
                        Message("Joined " + selectedRoom.name, null);
                        selectedRoom = null;
                        guistate = "room";
                    }
                }
                if (PhotonNetwork.GetRoomList().Length > 0)
                {
                    if (GUILayout.Button("Join random room"))
                    {
                        PhotonNetwork.JoinRandomRoom();
                        guistate = "room";
                        Message("Joined a random game", null);
                    }
                }
                if (GUILayout.Button("Settings"))
                {
                    settings = true;
                }
                if (GUILayout.Button("Exit"))
                {
                    Application.Quit();
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
                GUILayout.BeginArea(new Rect(15, 23, 870, 697));
                GUILayout.BeginVertical();
                foreach (PhotonPlayer player in PhotonNetwork.playerList)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(player.name + (player.isMasterClient ? " (host)" : ""));
                    if (PhotonNetwork.isMasterClient && player != PhotonNetwork.player)
                        if (GUILayout.Button(kickIcon, GUILayout.Width(30), GUILayout.Height(30)))
                            photonView.RPC("Kick", player);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
                GUILayout.BeginVertical();
                GUILayout.Box("", GUILayout.Width(380), GUILayout.Height(320));
                GUILayout.Box("", GUILayout.Width(380), GUILayout.Height(360));
                GUILayout.EndVertical();
                GUILayout.BeginArea(new Rect(910, 330, 380, 360));
                GUILayout.BeginVertical();
                for (int i = 0; i < chatMessages.Count; i++)
                {
                    GUI.Label(new Rect(0, 340 - i * skin.customStyles[0].fontSize - skin.customStyles[0].fontSize, 380, skin.customStyles[0].fontSize), chatMessages.ToArray()[i], skin.customStyles[0]);
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
        Message("Connected to the server", null);
    }

    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Message(player.name + " joined.", null);
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Message(player.name + " left.", null);
    }

    void SaveSettings()
    {
        PhotonNetwork.playerName = playerName;
        PlayerPrefs.SetString("name", playerName);
    }

    GUIContent[] CreateList(RoomInfo[] rooms)
    {
        GUIContent[] gc = new GUIContent[rooms.Length];
        for (int i = 0; i < gc.Length; i++)
        {
            gc[i] = new GUIContent(rooms[i].name/*+new string(' ',Mathf.RoundToInt(skin.label.CalcSize(new GUIContent(rooms[i].name)).x))+rooms[i].playerCount+" / "+rooms[i].maxPlayers*/);
        }
        return gc;
    }

    [RPC]
    void Message(string message, PhotonMessageInfo info)
    {
        if (info != null)
            chatMessages.Push(info.sender.name + ": " + message);
        else
            chatMessages.Push("System: " + message);
    }

    [RPC]
    void Kick()
    {
        PhotonNetwork.LeaveRoom();
        guistate = "connected";
    }
}
