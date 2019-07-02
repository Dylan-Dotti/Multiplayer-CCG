using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby Instance { get; private set; }

    [SerializeField] private GameObject usernameInputDisplay;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button cancelButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.AutomaticallySyncScene = true;
        usernameInputDisplay.SetActive(true);
        connectButton.gameObject.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join room");
        CreateRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room");
        CreateRoom();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room: " + PhotonNetwork.CurrentRoom.Name);
    }

    private void CreateRoom()
    {
        int randomRoomName = Random.Range(0, 10000);
        RoomOptions roomOps = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = (byte)MultiplayerSettings.Instance.MaxPlayers
        };
        Debug.Log("Attempting to create room: Room_" + randomRoomName);
        PhotonNetwork.CreateRoom("Room_" + randomRoomName, roomOps);
    }

    public void OnConnectButtonClicked()
    {
        connectButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnCancelButtonClicked()
    {
        Debug.Log("Leaving room: " + PhotonNetwork.CurrentRoom.Name);
        cancelButton.gameObject.SetActive(false);
        connectButton.gameObject.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }

    public void OnUsernameChanged(string newName)
    {
        connectButton.interactable = newName.Length > 0;
        //check for valid name
        foreach (char c in newName)
        {
            if (c != ' ')
            {
                MultiplayerSettings.Instance.PlayerUsername = newName;
                return;
            }
        }
        connectButton.interactable = false;
    }
}
