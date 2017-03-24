using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
	public float moveSpeedPercent;	// the percent of the distance between tiles that the character will move in each update
	private Vector3 updateMove;
	private GameController gc;		// reference to the game controller
	private Vector2 dest;			// the current destination of this character
	private List<Vector2> path;		// the remaining steps in the path to reach the destination
	private bool moving;			// check for whether the character is in motion or not
	private Vector2 nextTile;



	// Use this for initialization
	void Start () {
		gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
		transform.position = gc.emptyPointInGame ();
//		transform.position = new Vector3(-2.5f,-1.5f,0);
		moving = false;
	}
	
	// Update is called once per frame
	void Update () {
		// check if x has moved
		if (dest != gc.getDestination ()) {
			// if so, update destination position
			reroute ();
		}


		if (moving) {
			if ((Vector2)(transform.position) != nextTile) {
				// still on the way to the next tile, so keep going
				takeStep ();
			} else {
				moving = false;
				// remove the most recent tile in the path
				path.RemoveAt(0);
			}
		} else {
			// we either:
			// A) haven't started along a path or have reached the destination
			// B) we have just made it to the next tile
			// only move in B
 			if (path.Count > 0) {
				// we are stationary and there are more movements to make
				nextTile = path[0];
				updateMove = moveSpeedPercent * ((Vector3)nextTile - transform.position);

				moving = true;
			}

		}
		
	}

	private void reroute() {
		// figure out where we need to go
		dest = gc.getDestination();
		// build the path from the current position to there
		path = AStar.navigate (transform.position, dest);
	}


	private void takeStep() {
		// move along the path to the next tile
		transform.position += updateMove;
	}
}
