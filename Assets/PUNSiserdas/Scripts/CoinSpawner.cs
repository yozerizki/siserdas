using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public int jumlahCoin = 12;

    public Vector3 areaSize = new Vector3(20f, 0f, 20f);
    public float spawnHeight = 20f;
    public LayerMask groundLayer;

    public float minDistance = 2f;
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private bool hasSpawned = false;

    public string[] coinNames = {
        "Padi", "Jagung", "Gandum", "Oat", "Jelai", "Sorgum",
        "Milet", "Jewawut", "Jali", "Kinoa", "Fonio", "Buckwheat"
    };

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnCoins();
        }
    }

    public void SpawnCoins()
    {
        if (hasSpawned)
            return;

        hasSpawned = true;

        // reset list posisi (kalau nanti respawn)
        spawnedPositions.Clear();

        for (int i = 0; i < jumlahCoin; i++)
        {
            if (i >= coinNames.Length)
            {
                Debug.LogError("Jumlah coinNames kurang dari jumlahCoin!");
                return;
            }

            Vector3 randomPos = GetRandomPosition();
            SpawnCoinAtGround(randomPos, i);
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector3 pos;
        int attempts = 0;

        do
        {
            float x = Random.Range(-areaSize.x / 2, areaSize.x / 2);
            float z = Random.Range(-areaSize.z / 2, areaSize.z / 2);

            pos = transform.position + new Vector3(x, 0, z);
            attempts++;

        } while (!IsFarEnough(pos) && attempts < 20);

        spawnedPositions.Add(pos);
        return pos;
    }

    bool IsFarEnough(Vector3 pos)
    {
        foreach (var p in spawnedPositions)
        {
            if (Vector3.Distance(p, pos) < minDistance)
                return false;
        }
        return true;
    }

    void SpawnCoinAtGround(Vector3 position, int index)
    {
        RaycastHit hit;

        Vector3 rayStart = position + Vector3.up * spawnHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out hit, spawnHeight * 2, groundLayer))
        {
            Vector3 spawnPos = hit.point + Vector3.up * 0.5f;

            PhotonNetwork.InstantiateRoomObject(
                coinPrefab.name,
                spawnPos,
                Quaternion.identity,
                0,
                new object[] { coinNames[index] }
            );
        }
        else
        {
            Debug.LogWarning("Gagal menemukan ground untuk coin!");
        }
    }
}