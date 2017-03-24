using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	public GameObject obstacles; 
	public GameObject playingField;
	public GameObject borders;
	private Vector2 bottomLeft;

	// Use this for initialization
	void Start () {
		bottomLeft = new Vector2 (-1f * playingField.GetComponent<MeshFilter> ().mesh.bounds.size.x / 2f,
			-1 * playingField.GetComponent<MeshFilter> ().mesh.bounds.size.z / 2f);
		print (bottomLeft);
		// create a character and place them in a location
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
}
