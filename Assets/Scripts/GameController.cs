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
	public GameObject profs;			//
	public GameObject characterPrefab;	// the circle prefab that is used to show the agents
	public GameObject characters;		// the parent object that contains all the characters
	public int numCharacters;			// the number of characters in the game
	public float moveSpeedPercent;	// the percent of the distance between tiles that the character will move in each update
	public int pathWindow;				// how many steps a player will take in each window
	private Vector2 bottomLeft;			// the bottom left of the playing field
	private Vector2 size;				// the height and width of the playing field
	private List<plaque> plaques;		// list of the plaque information
	private List<Vector2> profLocs;		// list of professor information
	private int characterCount;			// a count of the number of characters currently walking
	private List<bool> characterPriorities;	// a check to see which characters should be scheduled in the algorithm first
	private List<List<List<int>>> AStarGrid; // the grid to be used for pathfinding


	// Use this for initialization
	void Start () {

		// we need these values for finding points in the playing field
		// (assume that everything is centered at (0,0)
		bottomLeft = -10f * new Vector2 (playingField.transform.localScale.x / 2f, playingField.transform.localScale.z / 2f);
		size = -2 * bottomLeft;

		// build the list of plaque information for the characters to obtain
		populateMapInfo ();

		characterPriorities = new List<bool> ();

		// create the characters
		for (int i = 0; i < numCharacters; i++) {
			Instantiate (characterPrefab, characters.transform);
			characterPriorities.Add (false);
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

		foreach (Transform prof in profs.transform) {
			if ((new Vector2 (x, y) - (Vector2) prof.position).magnitude < 0.001f) {
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

		foreach (Transform prof in profs.transform) {
			if ((click - (Vector2) prof.position).magnitude < 0.001f) {
				return true;
			}
		}

		return false;
	}

	public Vector2 emptyPointInGame() {
		// used to locate an empty point in the game for placing a player or a desitnation marker
//TODO: update this to include spots currently inhabitted by other stationary players as well as profs
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

	private void populateMapInfo() {
		// build the list of necessary information for the characters concerning plaque information

		plaques = new List<plaque> ();
		profLocs = new List<Vector2> ();

		GameObject go;
		plaque newPlaque;

		// all walls are 1x3 in size, so finding the position will allow us to find the lines segment of
		// the wall that each plaque hangs on, given prior knowledge of the layout of the map

		go = GameObject.Find ("wall1");
		newPlaque.profNumber = 0;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (0, 1f);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0, 0.51f);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-1.5f, 0.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (1.5f, 0.5f);
		plaques.Add (newPlaque);
		profLocs.Add (newPlaque.standingLocation + new Vector2 (-2f, 2f));

		go = GameObject.Find ("wall2");
		newPlaque.profNumber = 1;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (0, 1f);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0, 0.51f);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-1.5f, 0.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (1.5f, 0.5f);
		plaques.Add (newPlaque);
		profLocs.Add (newPlaque.standingLocation + new Vector2 (-2f, 2f));

		go = GameObject.Find ("wall3");
		newPlaque.profNumber = 2;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (-1f, 0);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (-0.51f, 0);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-0.5f, -1.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (-0.5f, 1.5f);
		plaques.Add (newPlaque);
		profLocs.Add (newPlaque.standingLocation + new Vector2 (-2f, -2f));

		go = GameObject.Find ("wall4");
		newPlaque.profNumber = 3;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (1f, 0);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0.51f, 0);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (0.5f, -1.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (0.5f, 1.5f);
		plaques.Add (newPlaque);
		profLocs.Add (newPlaque.standingLocation + new Vector2 (2f, 2f));

		go = GameObject.Find ("wall5");
		newPlaque.profNumber = 4;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (0, -1f);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0, -0.51f);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-1.5f, -0.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (1.5f, -0.5f);
		plaques.Add (newPlaque);
		profLocs.Add (newPlaque.standingLocation + new Vector2 (2f, -2f));

		go = GameObject.Find ("wall6");
		newPlaque.profNumber = 5;
		newPlaque.standingLocation = (Vector2)(go.transform.position) + new Vector2 (0, -1f);
		newPlaque.nameLocation = (Vector2)(go.transform.position) + new Vector2 (0, -0.51f);
		newPlaque.plaqueWall.p = (Vector2)(go.transform.position) + new Vector2 (-1.5f, -0.5f);
		newPlaque.plaqueWall.q = (Vector2)(go.transform.position) + new Vector2 (1.5f, -0.5f);
		plaques.Add (newPlaque);
		profLocs.Add (newPlaque.standingLocation + new Vector2 (2f, -2f));
	}

	public List<plaque> getPlaqueInfo () {
		// provide all the plaque information to the characters except for
		// the number on the plaque and randomize the list to ensure that the
		// character has to learn information
		return randomizedList(plaques);
	}

	private List<plaque> randomizedList(List<plaque> inputList)
	{

		int seed = (int)System.Environment.TickCount;
//		print (seed);
		Random.InitState (seed);

		List<int> usedValues = new List<int> ();
		List<plaque> randomList = new List<plaque>();
		int val;

		while (randomList.Count < 6) {
			val = Random.Range(0, 6);
			while (usedValues.Contains (val)) {
				val = Random.Range(0, 6);
			}

			// randomly add a plaque
			plaque tempPlaque = inputList [val];
			// obscure the number on the plaque so that the character has to query the gamecontroller
			// upon reaching the plaque
			tempPlaque.profNumber = -1;

			randomList.Add (tempPlaque);
		}

		return randomList;

	}

	public float getMoveSpeed() {
		return moveSpeedPercent;
	}


	public int getPathWindow() {
		return pathWindow;
	}

	public int readProfNumber(Vector2 plaqueLocation) {
		// once the character sees the id, they will read it based on its location
		foreach (plaque p in plaques) {
			if (p.standingLocation == plaqueLocation) {
				return p.profNumber;
			}
		}

		// the character was not close enough to any plaques
		return -1;
	}

	public Vector2 getProfLocation(int profID) {
		// characters inquire as to the position of a professor when they read the ID
		return profLocs [profID];
	}

	public plaque getPlaque(int profID) {
		// characters inquire as to the position of a professor when they read the ID
		return plaques[profID];
	}

	public int getCharacterID() {
		// this id will allow the characters to identify themselves during debugging
		return ++characterCount;
	}

	public List<Vector3> obtainCombinedPath (Vector2 start, Vector2 end, int pathSize, int id) {
		// combined path in the sense that it is in relation to all other paths in 3D space
		List<Vector2> newPath;
		newPath = AStar.navigate (start, end, , id);


		return finalPath;
	}

	public Vector2 getClosestSpot(int profID) {
		// look at the nodes adjacent to a particular prof's location and return the tile that
		// is closest to the prof and the shortest walk distance from the plaque

		List<Vector2> emptyTiles = new List<Vector2>();
		Vector2 testTile;
		Vector2 loc = getProfLocation (profID);

		// scan through the empty tiles one move away from the prof
		for (float i = -1f; i <= 1f; i += 1f) {
			for (float j = -1f; j <= 1f; j += 1f) {
				if (!(i == 0 && j == 0)){
					testTile = loc;
					testTile.x += i;
					testTile.y += j;
					if (!inObstacle (testTile)) {
						emptyTiles.Add (testTile);
					}
				}
			}
		}

		// pick the tile that is closest to the character
		float minDist = float.MaxValue;
		Vector2 bestLoc = new Vector2();

		Vector2 plaqueLoc = plaques [profID].nameLocation;

		foreach (Vector2 tile in emptyTiles) {
			if ((plaqueLoc - tile).magnitude < minDist) {
				minDist = (plaqueLoc - tile).magnitude;
				bestLoc = tile;
			}
		}

		return bestLoc;

	}

	private bool toProfessor(Vector2 loc) {
		foreach (Vector2 prof in profLocs) {
			if (prof == loc) {
				return true;
			}
		}

		return false;
	}

	public int getRandProfID(){
		return (int)Random.Range (0, 6f);
	}

	public void setPriority(int id, bool p){
		characterPriorities [id] = p;
	}

	private List<List<List<int>>> getWindowedGraph() {
		// build a grid of int that has a height and width of the playing field and depth of the window size
		List<List<List<int>>> graph = new List<List<List<int>>>();

		for (int i = 0; i < size.y; i++) {
			graph [i] = new List<List<int>> ();
			for (int j = 0; j < size.x; j++) {
				graph [i] [j] = new List<int> ();
				for (int k = 0; k < 20; k++){
					if (inObstacle ((float)i + 0.5f, (float)j + 0.5f)) {
						graph [i] [j] [k] = -1;
					} else {
						graph [i] [j] [k] = 0;
					}
				}
			}
		}

		return graph;
	}
}
