using UnityEngine;
using Photon.Pun;

public class GameManagerSiserdas : MonoBehaviourPun
{
    public Transform[] spawnPoints;

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("GameManagerSiserdas: Spawn points belum di-assign.");
            return;
        }

        if (string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            PhotonNetwork.NickName = "Kelompok " + PhotonNetwork.LocalPlayer.ActorNumber;
        }

        int index = PhotonNetwork.LocalPlayer.ActorNumber % spawnPoints.Length;

        Transform spawnPoint = spawnPoints[index];

        PhotonNetwork.Instantiate("Player", spawnPoint.position, Quaternion.identity);
    }
}