using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Writer : MonoBehaviour
{
    bool isWriter = false;
    public GameObject writerStuff;
    public GameObject collectiveStuff;
    public void IsWriter(bool isit)
    {
        isWriter = isit;
    }

    public void Submit()
    {
        if(isWriter)
        {
            writerStuff.SetActive(true);
        }else
        {
            collectiveStuff.SetActive(true);
        }
    }
}
