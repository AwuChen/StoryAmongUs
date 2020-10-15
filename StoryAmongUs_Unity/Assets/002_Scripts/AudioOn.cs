using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioOn : MonoBehaviour
{
    public AudioSource aud;
    public void AudioToggle(bool state)
    {
        aud.mute = state;
    }
}
