﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct professor {
	public int id;
	public Vector2 location;
}

public class Character : MonoBehaviour {
	private float moveSpeedPercent;	// the percent of the distance between tiles that the character will move in each update
	private int pathWindow;
	private Vector3 updateMove;
	private GameController gc;		// reference to the game controller
	private Vector2 windowDest;		// the current destination of this character
	private Vector2 finalDest;		// the location that the character is ultimately trying to get to
	private List<Vector2> path;		// the remaining steps in the path to reach the destination
	private Vector2 nextTile;
	private List<plaque> plaques;
	private List<professor> knowledgeBase;
	private int currentProf;



	// Use this for initialization
	void Start () {
		gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
		transform.position = gc.emptyPointInGame ();
		plaques = gc.getPlaqueInfo ();
		currentProf = (int)(Mathf.Floor(Random.Range (0, 6f)));
		pathWindow = gc.getPathWindow ();
		moveSpeedPercent = gc.getMoveSpeed ();
		getRoute (plaques[currentProf].standingLocation);
	}
	
	// Update is called once per frame
	void Update () {

		if (path.Count > 0) {
			// there are still moves to make
			if ((Vector2)(transform.position) != nextTile) {
				// still on the way to the next tile, so keep going
				takeStep ();
			} else {
				// remove the most recent tile in the path
				path.RemoveAt (0);
				if (path.Count > 0) {
					// keep walking if need be
					startPath();
				}

			}
		} else if ((Vector2)(transform.position) != finalDest) {
			// the most recent window ended and now we need to get the next window
			getRoute (plaques[currentProf].standingLocation);
			takeStep ();
		}
		
	}

	private bool isPlaqueVisible(int index) {
		return !Visibility.isHidden((Vector2)transform.position, plaques[index].nameLocation,plaques[index].plaqueWall.p,plaques[index].plaqueWall.q);
	}

	private void getRoute(Vector2 loc) {
		// figure out where we need to go
		finalDest = loc;
		// build the path from the current position to there
		path = AStar.navigate (transform.position, finalDest, pathWindow);
		startPath ();
	}

	private void startPath() {
		nextTile = path[0];
		updateMove = moveSpeedPercent * ((Vector3)nextTile - transform.position);
	}

	private void takeStep() {
		// move along the path to the next tile
		transform.position += updateMove;
	}


}
