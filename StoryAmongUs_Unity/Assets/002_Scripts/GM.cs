﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GM : MonoBehaviour
{
    public UnityEvent activate;
    public UnityEvent deactivate;
    public UnityEvent key;
    bool activated = false;
    bool isWalking = false;

    public static GM instance;

    public GameObject player;
    public GameObject[] players;
    public List<PathCondition> pathConditions = new List<PathCondition>();
    public List<Transform> pivots;

    public Transform[] objectsToHide;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InvokeRepeating("PlayerAccount", 1.0f, 1f);
    }

    void Update()
    {
        foreach (PathCondition pc in pathConditions)
        {
            int count = 0;
            for (int i = 0; i < pc.conditions.Count; i++)
            {
                if (pc.conditions[i].conditionObject.eulerAngles == pc.conditions[i].eulerAngle)
                {
                    count++;
                }
            }
            foreach(SinglePath sp in pc.paths)
                sp.block.possiblePaths[sp.index].active = (count == pc.conditions.Count);
        }
        if (players.Length > 0)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].GetComponent<PlayerManager>().walking)
                {
                    isWalking = true;
                }
                else
                {
                    isWalking = false;
                }
            }
        }
        if (player != null)
        {
            if (player.GetComponent<PlayerManager>().walking || isWalking)
                return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int multiplier = Input.GetKey(KeyCode.RightArrow) ? 1 : -1;
            RotateMaze(multiplier);
            //NetworkManager.instance.UpdateMazeRotation(multiplier);
        }

        foreach(Transform t in objectsToHide)
        {
            t.gameObject.SetActive(pivots[0].eulerAngles.y > 45 && pivots[0].eulerAngles.y < 90 + 45);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

    }

    public void PlayerAccount()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("LocalPlayer");
        }
        players = GameObject.FindGameObjectsWithTag("NetworkPlayer");
    }

    public void ActivateEvent()
    {
        if (!activated)
        {
            activate.Invoke();
            activated = true;
        }
        else if (activated)
        {
            deactivate.Invoke();
            activated = false;
        }
    }

    public void ActivateKey()
    {
        key.Invoke();
    }
    public void RotateRightPivot()
    {
        pivots[1].DOComplete();
        pivots[1].DORotate(new Vector3(0, 0, 90), .6f).SetEase(Ease.OutBack);
    }

    public void RestoreState()
    {
        pivots[0].DOComplete();
        pivots[0].DORotate(new Vector3(0, -90 , 0), .6f).SetEase(Ease.OutBack);
        pivots[1].DOComplete();
        pivots[1].DORotate(new Vector3(0, 0, 0), .6f).SetEase(Ease.OutBack);
    }

    public void RotateMaze(int multiplier)
    {
        pivots[0].DOComplete();
        pivots[0].DORotate(new Vector3(0, 90 * multiplier, 0), .6f, RotateMode.WorldAxisAdd).SetEase(Ease.OutBack);
    }
}

[System.Serializable]
public class PathCondition
{
    public string pathConditionName;
    public List<Condition> conditions;
    public List<SinglePath> paths;
}
[System.Serializable]
public class Condition
{
    public Transform conditionObject;
    public Vector3 eulerAngle;

}
[System.Serializable]
public class SinglePath
{
    public Walkable block;
    public int index;
}
