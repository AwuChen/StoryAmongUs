using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallSetup : MonoBehaviour
{
    public Transform originalPlace;
    bool onOff;
    // Start is called before the first frame update
    void Start()
    {
        originalPlace.position = GetComponent<RectTransform>().position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!onOff)
            {
                GetComponent<RectTransform>().position = new Vector3(535, 100, 0);
                onOff = true;
            }
            else if (onOff)
            {
                GetComponent<RectTransform>().position = new Vector3(-535, -100, 0);
                onOff = false;
            }
        }
    }
}
