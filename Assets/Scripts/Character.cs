using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
	private float moveSpeedPercent;	// the percent of the distance between tiles that the character will move in each update
	private int pathWindow;
	private Vector3 updateMove;
	private GameController gc;		// reference to the game controller
	private Vector2 windowDest;		// the current destination of this character
	private Vector2 finalDest;		// the location that the character is ultimately trying to get to
	private List<Vector2> path;		// the remaining steps in the path to reach the destination
	private Vector2 nextTile;
	private int pathStep;
	private List<plaque> plaques;
	private List<Vector2> profLocations;
	private int currentPlaqueIndex;
	private int myID;



	// Use this for initialization
	void Start () {
		gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
		transform.position = gc.emptyPointInGame ();
		myID = gc.getCharacterID ();
		print ("START -- " + myID.ToString() + ": (" + transform.position.x.ToString()+"," + transform.position.y.ToString()+")");

//		print (myID);
//		transform.position = new Vector3(-6.5f,0.5f,0);

		// ensure that each character has a different random number generator
		Random.InitState((int)transform.position.x);
		Random.InitState((int)(Random.Range (0, 20f)) + (int)transform.position.y);

		profLocations = new List<Vector2> ();
		// fill the professor locations with empty vectors
		for (int i = 0; i < 6; i++) {
			profLocations.Add (new Vector2 (0, 0));
		}

		plaques = gc.getPlaqueInfo ();
//		currentPlaqueIndex = (int)(Mathf.Floor(Random.Range (0, 6f)));
		currentPlaqueIndex = 0;
		pathWindow = gc.getPathWindow ();
		moveSpeedPercent = gc.getMoveSpeed ();
//		print (plaques [currentPlaqueIndex].standingLocation);
		getRoute ((Vector2)transform.position,plaques[currentPlaqueIndex].standingLocation);
		updatePath ();
	}
	
	// Update is called once per frame
	void Update () {

		makeMovement ();
		
	}

	private void makeMovement () {
		if ((Vector2)(transform.position) != windowDest) {
			// there are still moves to make
			if (!atNextTile()) {
				// still on the way to the next tile, so keep going
				takeStep ();
				if (plaques[currentPlaqueIndex].profNumber == -1 && canReadPlaque(currentPlaqueIndex)) {
					// we can read the plaque, so read it and get the info about the professor
					updateKnowledgeBase ();
					// the path is now pointing to the professor
					getRoute (nextTile,profLocations[plaques[currentPlaqueIndex].profNumber]);
					// and the next update of the walking direction will be set back to the start of the path
				}
			} else {
				updatePath ();
				takeStep ();
			}
		} else if ((Vector2)(transform.position) != finalDest) {
			// the most recent window ended and now we need to get the next window
			getRoute (transform.position, plaques[currentPlaqueIndex].standingLocation);
			updatePath ();
			takeStep ();
		}
	}

	private bool canReadPlaque(int index) {
		// let us know if we can read the plaque
		if (isAdjacent(plaques[currentPlaqueIndex].nameLocation,0) && plaqueVisible (currentPlaqueIndex)) {
			return true;
		}
		return false;
	}

	private bool plaqueVisible(int index) {
		// check to see if the line segments describing the character to the plaque surface and the wall
		// that the plaque rests on have an intersection
		return !Visibility.isHidden((Vector2)transform.position, plaques[index].nameLocation,
			plaques[index].plaqueWall.p,plaques[index].plaqueWall.q);
	}

	private void getRoute(Vector2 start, Vector2 end) {
		// figure out where we need to go
		finalDest = end;

		// build the path from the current position to there
		path = AStar.navigate (start, finalDest, pathWindow);
		print ("END -- " + myID.ToString() + ": (" + end.x.ToString()+"," + end.y.ToString()+")");

		pathStep = 0;

		windowDest = path [path.Count - 1];
		print ("WINDOW -- " + myID.ToString() + ": (" + windowDest.x.ToString()+"," + windowDest.y.ToString()+")");

	}

	private void updatePath() {
		// set everything up for the next movement
		nextTile = path[pathStep++];
		print ("STEP -- " + myID.ToString() + ": (" + nextTile.x.ToString()+"," + nextTile.y.ToString()+")");
		updateMove = moveSpeedPercent * (nextTile - (Vector2)transform.position).normalized;
	}

	private void takeStep() {
		// move along the path to the next tile
		transform.position += updateMove;
		print (myID.ToString() + ": (" + transform.position.x.ToString()+"," + transform.position.y.ToString()+")");
	}

	private bool isAdjacent(Vector2 loc, int type) {
		// adjacency means that the distance between this character's center and the other
		// object is less than 2 radius (2*0.5f)

		// for the plaque (type 0), this means directly measuring the distance from the midpoint
		// of the plaque surface to the character
		if (type == 0) {
			return ((Vector2)transform.position - loc).magnitude < 1f;
		} else if (type == 1) {
			// for the prof(type 1), this means that their centers are at most 3 radii apart
			return ((Vector2)transform.position - loc).magnitude < 1.5f;
		}

		return false;
	}

	private void updateKnowledgeBase () {
		// update the plaque information for the current path, specifically the prof id seen on the
		// plaque
		plaque tempPlaque = plaques[currentPlaqueIndex];
		tempPlaque.profNumber = gc.readProfNumber (tempPlaque.nameLocation);
		plaques [currentPlaqueIndex] = tempPlaque;

		// update the professor location based on the prof id learned from viewing the plaque
		profLocations[tempPlaque.profNumber] = gc.getProfLocation(tempPlaque.profNumber);

		//print (tempPlaque.profNumber);
		//print (profLocations [tempPlaque.profNumber]);
	}

	private bool atNextTile() {
		// sometimes the == operator does not return true when the character is technically in the right
		// position, so we'll check to see if it's incredibly close and then lock it in place

		bool result = false;

		if (((Vector2)transform.position - nextTile).magnitude < 0.0001f) {
			// close enough

			// lock into place
			transform.position = nextTile;
			result = true;
		}

		return result;
	}
}
