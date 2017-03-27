using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct wallSegment {
	// a struct to hold the coordinates that describe a wall
	public Vector2 p;
	public Vector2 q;
}

public struct plaque {
	public Vector2 nameLocation; // the midpoint of the side of the plaque closest to the prof
	public Vector2 standingLocation; // where the character should move to in order to see the plaque
	public wallSegment plaqueWall; // the wall that the plaque hangs on, used to determine if there is a line of sight to the plaque
	public int profNumber; // the number of the associated prof
}

public class GameController : MonoBehaviour {
	// class to control the functionality of the game

	public GameObject obstacles; 		// parent object of all the obstactles in the game
	public GameObject playingField;		// board on which the game will be played
	public GameObject borders;			// boundary objects which define the field
	public GameObject characterPrefab;	// the circle prefab that is used to show the agents
	public GameObject characters;		// the parent object that contains all the characters
	public int numCharacters;			// the number of characters in the game
	public float moveSpeedPercent;	// the percent of the distance between tiles that the character will move in each update
	public int pathWindow;				// how many steps a player will take in each window
	private Vector2 bottomLeft;			// the bottom left of the playing field
	private Vector2 size;				// the height and width of the playing field
	private List<plaque> plaques;

	// Use this for initialization
	void Start () {
		// we need these values for finding points in the playing field
		// (assume that everything is centered at (0,0)
		bottomLeft = -1f * new Vector2 (playingField.GetComponent<MeshFilter> ().mesh.bounds.size.x / 2f,
			playingField.GetComponent<MeshFilter> ().mesh.bounds.size.z / 2f);
		size = -2 * bottomLeft;

		// build the list of plaque information for the characters to obtain
		populatePlaqueInfo ();

		// create the characters
		for (int i = 0; i < numCharacters; i++) {
			Instantiate (characterPrefab, characters.transform);
		}

	}
	
	// Update is called once per frame
	void Update() {
	}

	public bool inObstacle(float x, float y) {
		foreach (Transform child in obstacles.transform) {
			// get the rectangle that describes the top surface of the object
			Vector2 pos = new Vector2 (child.position.x - child.localScale.x / 2f, 
				child.position.y - child.localScale.y / 2f) - bottomLeft;
			Vector2 size = new Vector2 (child.localScale.x, child.localScale.y);

			// this rectangle contains the obstacle from a 2D standpoint
			Rect rect = new Rect(pos,size);

			// is the point under investigation within the object
			if (rect.Contains (new Vector2 (x, y) - bottomLeft)) {
				return true;
			}
		}

		// repeat with borders
		foreach (Transform child in borders.transform) {
			
			Vector2 pos = new Vector2 (child.position.x - child.localScale.x / 2f, 
				child.position.y - child.localScale.y / 2f) - bottomLeft;
			Vector2 size = new Vector2 (child.localScale.x, child.localScale.y);
			Rect rect = new Rect(pos,size);

			if (rect.Contains (new Vector2 (x, y) - bottomLeft)) {
				return true;
			}
		}

		return false;
	}

	public bool inObstacle(Vector2 click) {
		// the other version of the above, given a "click" or the Vector2 representation of a point
		foreach (Transform child in obstacles.transform) {
			Vector2 pos = new Vector2 (child.position.x - child.localScale.x / 2f, 
				child.position.y - child.localScale.y / 2f) - bottomLeft;
			Vector2 size = new Vector2 (child.localScale.x, child.localScale.y);
			Rect rect = new Rect(pos,size);
			if (rect.Contains (new Vector2 (click.x, click.y) - bottomLeft)) {
				return true;
			}
		}

		foreach (Transform child in borders.transform) {
			Vector2 pos = new Vector2 (child.position.x - child.localScale.x / 2f, 
				child.position.y - child.localScale.y / 2f) - bottomLeft;
			Vector2 size = new Vector2 (child.localScale.x, child.localScale.y);
			Rect rect = new Rect(pos,size);
			if (rect.Contains (new Vector2 (click.x, click.y) - bottomLeft)) {
				return true;
			}
		}

		return false;
	}

	public Vector2 emptyPointInGame() {
		// used to locate an empty point in the game for placing a player or a desitnation marker

		float x;
		float y;

		do {
			// get x and y values somewhere in the playing field at the center of a tile
			x = Mathf.Floor(Random.value * size.x + bottomLeft.x) + 0.5f;
			y = Mathf.Floor(Random.value * size.y + bottomLeft.y) + 0.5f;

			// check to see if this is a valid point to place something
		} while (inObstacle (x, y));

		return new Vector2 (x, y);
	}

//	public Vector2 getDestination() {
//		// gameobjects in the game will want to know where they should be moving
//		return marker.transform.position;
//	}

	private void populatePlaqueInfo() {
		// build the list of necessary information for the characters concerning plaque information

		plaques = new List<plaque> ();

		GameObject go;
		plaque newPlaque;

		// all walls are 1x3 in size, so finding the position will allow us to find the lines segment of
		// the wall that each plaque hangs on, given prior knowledge of the layout of the map

		go = GameObject.Find ("wall1");
		newPlaque.profNumber = 1;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (0, 1f);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0, 0.51f);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-1.5f, 0.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (1.5f, 0.5f);
		plaques.Add (newPlaque);

		go = GameObject.Find ("wall2");
		newPlaque.profNumber = 2;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (0, 1f);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0, 0.51f);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-1.5f, 0.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (1.5f, 0.5f);
		plaques.Add (newPlaque);

		go = GameObject.Find ("wall3");
		newPlaque.profNumber = 3;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (-1f, 0);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (-0.51f, 0);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-0.5f, -1.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (-0.5f, 1.5f);
		plaques.Add (newPlaque);

		go = GameObject.Find ("wall4");
		newPlaque.profNumber = 4;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (1f, 0);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0.51f, 0);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (0.5f, -1.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (0.5f, 1.5f);
		plaques.Add (newPlaque);

		go = GameObject.Find ("wall5");
		newPlaque.profNumber = 5;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (0, -1f);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0, -0.51f);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-1.5f, -0.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (1.5f, -0.5f);
		plaques.Add (newPlaque);

		go = GameObject.Find ("wall6");
		newPlaque.profNumber = 6;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (0, -1f);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0, -0.51f);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-1.5f, -0.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (1.5f, -0.5f);
		plaques.Add (newPlaque);
	}

	public List<plaque> getPlaqueInfo () {
		// provide all the plaque information to the characters
		return plaques;
	}

	public float getMoveSpeed() {
		return moveSpeedPercent;
	}


	public int getPathWindow() {
		return pathWindow;
	}
}
