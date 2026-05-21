using UnityEngine;
using Photon.Pun;
using TMPro;

public class Coin : MonoBehaviourPun
{
    public string coinName;
    public TMP_Text coinText;

    private bool isTaken = false;
    private Collider[] cachedColliders;
    private Renderer[] cachedRenderers;

    private void Awake()
    {
        cachedColliders = GetComponentsInChildren<Collider>(true);
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
    }

    void Start()
    {
        // ambil data dari spawn
        if (photonView.InstantiationData != null)
        {
            coinName = (string)photonView.InstantiationData[0];
        }

        // set text
        if (coinText == null)
        {
            coinText = GetComponentInChildren<TMP_Text>();
        }

        if (coinText != null)
        {
            coinText.text = coinName;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerData player = other.GetComponent<PlayerData>();
        if (player == null || player.hasCoin) return;

        if (isTaken) return;
        photonView.RPC("RequestTakeCoin", RpcTarget.MasterClient, player.photonView.ViewID);
    }

    [PunRPC]
    void RequestTakeCoin(int playerViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (isTaken) return;

        isTaken = true;

        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView == null) return;

        PlayerData player = playerView.GetComponent<PlayerData>();
        if (player == null || player.hasCoin) return;

        // Hide this coin on every client immediately.
        photonView.RPC(nameof(SetTakenState), RpcTarget.AllBuffered);

        // Grant coin only to the owning client so each player can progress independently.
        playerView.RPC("ReceiveCoin", playerView.Owner, coinName);

        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void SetTakenState()
    {
        // Jangan guard dengan isTaken di sini - karena OnTriggerEnter dan RequestTakeCoin
        // sudah set isTaken=true sebelum RPC ini sampai, sehingga visual tidak ter-update.
        // Guard hanya di RequestTakeCoin (master) dan OnTriggerEnter.
        isTaken = true;

        if (cachedColliders != null)
        {
            for (int i = 0; i < cachedColliders.Length; i++)
            {
                if (cachedColliders[i] != null)
                    cachedColliders[i].enabled = false;
            }
        }

        if (cachedRenderers != null)
        {
            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                    cachedRenderers[i].enabled = false;
            }
        }
    }
}