using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    //room info
    public static PhotonRoom Instance { get; private set; }

    public bool IsGameLoaded { get; private set; }
    public int CurrentScene { get; private set; }

    private PhotonView pView;

    //player info
    public int PlayersInRoom { get; private set; }
    public int MyNumberInRoom { get; private set; }
    public int PlayersInGame { get; private set; }

    private Player[] players;

    //delayed start
    public float StartingTime { get => startingTime; set => startingTime = value; }

    [SerializeField] private float startingTime = 3;
    [SerializeField] private Text waitingText;
    [SerializeField] private Text countdownText;

    private float lessThanMaxPlayers;
    private float atMaxPlayers;
    private float timeToStart;

    private bool readyToCount;
    private bool readyToStart;


    private void Awake()
    {
        //set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(Instance.gameObject);
                Instance = this;
            }
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        pView = GetComponent<PhotonView>();
        readyToCount = false;
        readyToStart = false;
        lessThanMaxPlayers = StartingTime;
        atMaxPlayers = 3;
        timeToStart = StartingTime;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (MultiplayerSettings.Instance.DelayStart)
        {
            StartCoroutine(DelayStartCR());
        }
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        players = PhotonNetwork.PlayerList;
        PlayersInRoom = players.Length;
        MyNumberInRoom = PlayersInRoom;
        PhotonNetwork.NickName = MyNumberInRoom.ToString();
        if (MultiplayerSettings.Instance.DelayStart)
        {
            Debug.Log(string.Format("{0}/{1} players", PlayersInRoom, 
                MultiplayerSettings.Instance.MaxPlayers));
            if (PlayersInRoom > 1)
            {
                readyToCount = true;
            }
            if (PlayersInRoom == MultiplayerSettings.Instance.MaxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient) return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }
        }
        else
        {
            StartGame();
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        StopAllCoroutines();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("A new player has entered the room");
        players = PhotonNetwork.PlayerList;
        PlayersInRoom++;
        if (MultiplayerSettings.Instance.DelayStart)
        {
            Debug.Log(string.Format("{0}/{1} players", PlayersInRoom,
                MultiplayerSettings.Instance.MaxPlayers));
            if (PlayersInRoom > 1)
            {
                readyToCount = true;
            }
            if (PlayersInRoom == MultiplayerSettings.Instance.MaxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient) return;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }

        }
    }

    private void StartGame()
    {
        IsGameLoaded = true;
        if (!PhotonNetwork.IsMasterClient) return;
        if (MultiplayerSettings.Instance.DelayStart)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        PhotonNetwork.LoadLevel(MultiplayerSettings.Instance.MultiplayerScene);
    }

    private void RestartTimer()
    {
        lessThanMaxPlayers = StartingTime;
        timeToStart = StartingTime;
        atMaxPlayers = 3;
        readyToCount = false;
        readyToStart = false;
    }

    private void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        CurrentScene = scene.buildIndex;
        if (CurrentScene == MultiplayerSettings.Instance.MultiplayerScene)
        {
            IsGameLoaded = true;
            if (MultiplayerSettings.Instance.DelayStart)
            {
                pView.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
            }
            else
            {
                RPC_CreatePlayer();
            }
        }
    }

    [PunRPC]
    private void RPC_LoadedGameScene()
    {
        Debug.Log("Loaded game scene");
        PlayersInGame++;
        if (PlayersInGame == PhotonNetwork.PlayerList.Length)
        {
            pView.RPC("RPC_CreatePlayer", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_CreatePlayer()
    {
        Debug.Log("Creating player");
        PhotonNetwork.Instantiate(Path.Combine(
            "Photon Prefabs", "Photon Network Player"), 
            transform.position, Quaternion.identity, 0);
    }

    private IEnumerator DelayStartCR()
    {
        while (true)
        {
            if (PlayersInRoom < MultiplayerSettings.Instance.MaxPlayers)
            {
                waitingText.gameObject.SetActive(true);
                countdownText.gameObject.SetActive(false);
                RestartTimer();
            }
            else
            {
                waitingText.gameObject.SetActive(false);
                countdownText.gameObject.SetActive(true);

                if (readyToStart)
                {
                    atMaxPlayers = Mathf.Max(0,
                        atMaxPlayers - Time.deltaTime);
                    lessThanMaxPlayers = atMaxPlayers;
                    timeToStart = atMaxPlayers;
                }
                else if (readyToCount)
                {
                    lessThanMaxPlayers = Mathf.Max(0,
                        lessThanMaxPlayers - Time.deltaTime);
                    timeToStart = lessThanMaxPlayers;
                }
                countdownText.text = string.Format("Starting game in: {0}",
                    Mathf.CeilToInt(timeToStart));
                if (timeToStart <= 0)
                {
                    StartGame();
                    countdownText.gameObject.SetActive(false);
                    break;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
