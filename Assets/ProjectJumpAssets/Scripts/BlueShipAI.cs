﻿using UnityEngine;
using System.Collections;

public class BlueShipAI : MonoBehaviour {

    //setting the target and movement speed
    public Transform target;
    public float speed = 2f;
    GameObject player;

	// Use this for initialization
	void Start () {
        //Finding position and target
        player = GameObject.FindGameObjectWithTag("Player");
    }
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance (transform.position, player.transform.position) > 0 && Vector3.Distance(transform.position, player.transform.position) < 15) {
            transform.LookAt(player.transform);
            transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));
        }
     }
}