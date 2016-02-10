﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class RestartButton : MonoBehaviour, IPointerDownHandler
{

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // When the button is pressed restart the game
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("pressed");
        restartScene();
    }

    public void restartScene()
    {
        SceneManager.LoadScene("JumpScene");
    }
}