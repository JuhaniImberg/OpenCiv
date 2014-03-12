﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles nearly everything, core of everything
/// </summary>

public class Manager : Photon.MonoBehaviour
{
    public float version = 1.0f;
    public string guistate = "connecting";
    public float presetResolution_width = 1280, presetResolution_height = 720;

    private float reconnectDelay = 0f, reconnectTime = 0f;
    private string chatMessage = "";
    public List<string> chatMessages = new List<string> { " ", " ", " ", " ", " ", " ", " ", " ", " ", " " };

    void Start()
    {
        StartCoroutine("Connect");
    }

    void Update()
    {

    }

    void OnGUI()
    {
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
                GUILayout.BeginVertical();
                GUILayout.Box("", GUILayout.Width(380), GUILayout.Height(300));
                GUILayout.Box("", GUILayout.Width(380), GUILayout.Height(420));
                GUILayout.BeginArea(new Rect(910, 300, 380, 390));
                GUILayout.BeginVertical();
                for (int i = 0; i < chatMessages.Count; i++)
                {
                    GUI.Label(new Rect(0, 390- i * 22-22, 380, 22), chatMessages[i]);
                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();

                GUI.SetNextControlName("asd");
                chatMessage = GUI.TextArea(new Rect(900, 690, 380, 30), chatMessage);
                if (chatMessage.Contains("\n"))
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
        PhotonNetwork.JoinRandomRoom();
    }

    void OnJoinedRoom()
    {
        guistate = "connected";
    }

    void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom("room", true, true, 20);
    }

    void OnPhotonCreateRoomFailed()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    [RPC]
    void Message(string message, PhotonMessageInfo info)
    {
        message.Trim();
        for (int i = chatMessages.Count - 1; i > 0; i--)
        {
            chatMessages[i] = chatMessages[i - 1];
        }
        chatMessages[0] = info.sender.name + ": " + message;
    }
}
