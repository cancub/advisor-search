using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	public GameObject obstacles; 
	public GameObject playingField;
	public GameObject borders;
	public GameObject characterPrefab;
	public GameObject characters;
	private Vector2 bottomLeft;
	private Vector2 size;


	// Use this for initialization
	void Start () {
		// we need these values for finding points in the playing field
		// (assume that everything is centered at (0,0)
		bottomLeft = -1f * new Vector2 (playingField.GetComponent<MeshFilter> ().mesh.bounds.size.x / 2f,
			playingField.GetComponent<MeshFilter> ().mesh.bounds.size.z / 2f);
		size = -2 * bottomLeft;

//		print (bottomLeft);
		// create a character 
		Instantiate(characterPrefab,characters.transform);

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool inObstacle(float x, float y) {
		foreach (Transform child in obstacles.transform) {
			// get the rectangle that describes the top surface of the object
			Vector2 pos = new Vector2 (child.position.x - child.localScale.x / 2f, 
				child.position.y - child.localScale.y / 2f) - bottomLeft;
			Vector2 size = new Vector2 (child.localScale.x, child.localScale.y);
			Rect rect = new Rect(pos,size);
			// this child rectangle contains the object
			print(rect);

			if (rect.Contains (new Vector2 (x, y) - bottomLeft)) {
				return true;
			}
		}

		foreach (Transform child in borders.transform) {
			// get the rectangle that describes the top surface of the object
			Vector2 pos = new Vector2 (child.position.x - child.localScale.x / 2f, 
				child.position.y - child.localScale.y / 2f) - bottomLeft;
			Vector2 size = new Vector2 (child.localScale.x, child.localScale.y);
			Rect rect = new Rect(pos,size);
			// this child rectangle contains the object
			print(rect);

			if (rect.Contains (new Vector2 (x, y) - bottomLeft)) {
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
}
