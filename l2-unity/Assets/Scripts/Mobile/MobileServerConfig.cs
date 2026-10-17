using UnityEngine;

/// <summary>
/// Pre-configured connection settings for the DarkRealm Lineage 2 Interlude server.
/// </summary>
public class MobileServerConfig : MonoBehaviour
{
    [Header("Server Host & Ports")]
    public string ServerHost = "62.169.27.91";
    public int LoginPort = 2106;
    public int GamePort = 7777;

    [Header("Protocol Settings")]
    public int ProtocolVersion = 746; // Lineage 2 Interlude (C6)

    public static MobileServerConfig Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
