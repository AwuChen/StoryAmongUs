using Byn.Awrtc.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AndroidInitConfigUi : MonoBehaviour
{
    public Toggle hardwareAcc;
    public Toggle useTextures;
    public Toggle forcePref;
    public Dropdown codec;

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void Init()
    {
        AndroidInitConfig config = new AndroidInitConfig();
        config.hardwareAcceleration = hardwareAcc.isOn;
        config.useTextures = useTextures.isOn;
        if(codec.value != 0)
        {
            config.preferredCodec = codec.options[codec.value].text;
        }
        config.forcePreferredCodec = forcePref.isOn;

        Debug.Log("Setting android init config: " + config);
        UnityCallFactory.AndroidConfig = config;
        UnityCallFactory.EnsureInit(() => {
            Debug.Log("Init complete. ");

            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("menuscene");
        });
    }
}
