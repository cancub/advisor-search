using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
	public float moveSpeedPercent;	// the percent of the distance between tiles that the character will move in each update
	private Vector3 updateMove;
	private GameController gc;		// reference to the game controller
	private Vector2 dest;			// the current destination of this character
	private List<Vector2> path;		// the remaining steps in the path to reach the destination
	private Vector2 nextTile;
	private List<plaque> plaques;



	// Use this for initialization
	void Start () {
		gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
		transform.position = gc.emptyPointInGame ();
		plaques = gc.getPlaqueInfo ();
		getRoute ();

	}
	
	// Update is called once per frame
	void Update () {

		if (path.Count > 0) {
			// there are still moves to make
			if ((Vector2)(transform.position) != nextTile) {
				// still on the way to the next tile, so keep going
				takeStep ();
			} else {
				// check if x has moved
				if (dest != gc.getDestination ()) {
					// if so, update destination position and find the new route
					getRoute ();
					takeStep ();
				} else {
					// remove the most recent tile in the path
					path.RemoveAt (0);
					if (path.Count > 0) {
						nextTile = path [0];
						updateMove = moveSpeedPercent * ((Vector3)nextTile - transform.position);
					}
				}
			}
		} else if (dest != gc.getDestination ()) {
			// if so, update destination position and find the new route
			getRoute ();
			takeStep ();
		}
		
	}

	private bool isPlaqueVisible(int index) {
		return !Visibility.isHidden((Vector2)transform.position, plaques[index].nameLocation,plaques[index].plaqueWall.p,plaques[index].plaqueWall.q);
	}

	private void getRoute() {
		// figure out where we need to go
		dest = gc.getDestination();
		// build the path from the current position to there
		path = AStar.navigate (transform.position, dest);
		nextTile = path[0];
		updateMove = moveSpeedPercent * ((Vector3)nextTile - transform.position);
	}


	private void takeStep() {
		// move along the path to the next tile
		transform.position += updateMove;
	}


}
