using UnityEngine;

public class MultiplayerSettings : MonoBehaviour
{
    public static MultiplayerSettings Instance { get; private set; }

    public string PlayerUsername { get; set; }

    public bool DelayStart { get => delayStart; set => delayStart = value; }
    public int MaxPlayers { get => maxPlayers; set => maxPlayers = value; }

    public int MenuScene { get => menuScene; set => menuScene = value; }
    public int MultiplayerScene { get => multiplayerScene; set => multiplayerScene = value; }

    [SerializeField] private bool delayStart;
    [SerializeField] private int maxPlayers;
    [SerializeField] private int menuScene;
    [SerializeField] private int multiplayerScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        DontDestroyOnLoad(gameObject);
    }
}
