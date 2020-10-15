using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Activate : MonoBehaviour
{
    public UnityEvent activate;
    public UnityEvent deactivate;
    bool activated = false;
    // Start is called before the first frame update
    void Start()
    {
        activate.Invoke();
        activated = true;
    }

    public void Act()
    {
        if (!activated)
        {
            activate.Invoke();
            activated = true;
        }
        else if(activated)
        {
            deactivate.Invoke();
            activated = false;
        }
    }
}
