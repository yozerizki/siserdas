using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerNameTag : MonoBehaviourPun
{
    public TMP_Text nameText;

    void Start()
    {
        if (nameText == null)
            nameText = GetComponentInChildren<TMP_Text>();

        // pakai nickname kalau ada
        if (!string.IsNullOrEmpty(photonView.Owner.NickName))
        {
            nameText.text = photonView.Owner.NickName;
        }
        else
        {
            nameText.text = "Player " + photonView.Owner.ActorNumber;
        }
    }
}