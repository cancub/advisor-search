using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	// class to control the functionality of the game

	public GameObject obstacles; 		// parent object of all the obstactles in the game
	public GameObject playingField;		// board on which the game will be played
	public GameObject borders;			// boundary objects which define the field
	public GameObject characterPrefab;	// the circle prefab that is used to show the agents
	public GameObject characters;		// the parent object that contains all the characters
	public int numCharacters;			// the number of characters in the game
	public GameObject marker;			// the X destination point for now
	private Vector2 bottomLeft;			// the bottom left of the playing field
	private Vector2 size;				// the height and width of the playing field

	// Use this for initialization
	void Start () {
		// we need these values for finding points in the playing field
		// (assume that everything is centered at (0,0)
		bottomLeft = -1f * new Vector2 (playingField.GetComponent<MeshFilter> ().mesh.bounds.size.x / 2f,
			playingField.GetComponent<MeshFilter> ().mesh.bounds.size.z / 2f);
		size = -2 * bottomLeft;

		// place the marker somewhere random that works
		marker.transform.position = emptyPointInGame ();
//		marker.transform.position = new Vector3(2.5f,-2.5f,0);

		// create the characters
		for (int i = 0; i < numCharacters; i++) {
			Instantiate (characterPrefab, characters.transform);
		}

	}
	
	// Update is called once per frame
	void Update() {

		// move the marker if the user has clicked somewhere that makes sense
		if (Input.GetMouseButtonDown(0)) {
			MoveTarget(Camera.main.ScreenPointToRay(Input.mousePosition));
		}
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

	public Vector2 getDestination() {
		// gameobjects in the game will want to know where they should be moving
		return marker.transform.position;
	}

	private void MoveTarget(Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit)) {
			// get the x and y positions of the click, but center in the corresponding tile
			float x = Mathf.Floor(hit.point.x) + 0.5f;
			float y = Mathf.Floor(hit.point.y) + 0.5f;

			// move the marker if the click occured in a position that is free from objects
			if (!inObstacle(x,y)){
				marker.transform.position = new Vector3 (x, y, 0);
			}
		}
	}
}
