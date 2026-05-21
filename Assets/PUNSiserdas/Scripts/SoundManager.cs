using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{

    public Sprite mute;
    public Sprite unmute;
    public Image soundicon;
    public AudioSource[] bgms;


    Button buttonmute;
    public void Awake()
    {

        mute = Resources.Load("mut", typeof(Sprite)) as Sprite;
        unmute = Resources.Load("unmut", typeof(Sprite)) as Sprite;
        GameObject volumeObject = GameObject.Find("vol");
        GameObject holderObject = GameObject.Find("holder");

        if (volumeObject != null)
        {
            soundicon = volumeObject.GetComponent<Image>();
            buttonmute = volumeObject.GetComponent<Button>();
        }

        if (holderObject != null)
        {
            bgms = holderObject.GetComponents<AudioSource>();
        }
        else
        {
            bgms = new AudioSource[0];
        }

        if (buttonmute != null)
        {
            buttonmute.onClick.AddListener(mutepressed);
        }
    }

    public void mutepressed()
    {
        GameObject holderObject = GameObject.Find("holder");
        if (holderObject == null)
            return;

        GameDataHolder holder = holderObject.GetComponent<GameDataHolder>();
        if (holder == null)
            return;

        if (holder.soundon == true)
        {
            holder.soundon = false;
            if (soundicon != null)
                soundicon.sprite = mute;
            foreach (AudioSource bgm in bgms)
                bgm.Pause();
        }
        else
        {
            holder.soundon = true;
            if (soundicon != null)
                soundicon.sprite = unmute;
            foreach (AudioSource bgm in bgms)
                bgm.Play();
        }
    }
}