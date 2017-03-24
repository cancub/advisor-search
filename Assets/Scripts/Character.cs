using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
	private GameController gc;
	private Vector2 dest;

	// Use this for initialization
	void Start () {
		gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
		transform.position = gc.emptyPointInGame ();
	}
	
	// Update is called once per frame
	void Update () {
		// check if x has moved
		// if so, update destination position
		// restart algorithm
		
	}
}
