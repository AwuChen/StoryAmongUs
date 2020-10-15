using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Click : MonoBehaviour
{
    public UnityEvent activate;
    bool runOnce = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        CheckInput();
    }

    void CheckInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            runOnce = false;
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition); RaycastHit mouseHit;

            if (Physics.Raycast(mouseRay, out mouseHit))
            {
                if (mouseHit.transform.GetComponent<Click>() != null && !runOnce)
                {
                    mouseHit.transform.GetComponent<Click>().activate.Invoke();
                    runOnce = true;
                }
                //else if (mouseHit.transform.GetComponent<Walkable>(). != null)
            }
        }
    }

    
}
