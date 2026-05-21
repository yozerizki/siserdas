using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tutupAftersecond : MonoBehaviour
{

    public float seconds = 3.0f;
    public Text countdownText;

    // Start is called before the first frame update
    void Start()
    {
        //countdownText = GetComponentInChildren<Text>(true);
        StartCoroutine(closeAfterSeconds(seconds));
    }

    public IEnumerator closeAfterSeconds(float seconds)
    {
        float remainingSeconds = seconds;

        while (remainingSeconds > 0f)
        {
            UpdateCountdownText(remainingSeconds);
            yield return null;
            remainingSeconds -= Time.deltaTime;
        }

        UpdateCountdownText(0f);
        this.transform.gameObject.SetActive(false);
    }

    private void UpdateCountdownText(float remainingSeconds)
    {
        if (countdownText == null)
            return;

        countdownText.text = "Tutup (" + Mathf.CeilToInt(Mathf.Max(remainingSeconds, 0f)) + ")";
    }

}
