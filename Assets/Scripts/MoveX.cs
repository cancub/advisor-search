using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveX : MonoBehaviour {
	public GameObject obstacles; 
	public GameObject playingField;
	public GameObject borders;
	private Vector2 bottomLeft;

	void Start() {
		// hide the object by placing it in the center
		transform.position = new Vector3(2.5f,2.5f,0);
		bottomLeft = new Vector2 (-1f * playingField.GetComponent<MeshFilter> ().mesh.bounds.size.x / 2f,
			-1 * playingField.GetComponent<MeshFilter> ().mesh.bounds.size.z / 2f);
		print (bottomLeft);
	}

	void Update() {
		
		if (Input.GetMouseButtonDown(0)) {
			MoveTarget(Camera.main.ScreenPointToRay(Input.mousePosition));
		}
	}

	private void MoveTarget(Ray ray) {
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit)) {
			// get the x and y positions of the click
			float x = Mathf.Floor(hit.point.x) + 0.5f;
			float y = Mathf.Floor(hit.point.y) + 0.5f;

			print (x.ToString() + "," + y.ToString());
			if (!inObstacle(x,y)){
				transform.position = new Vector3 (x, y, transform.position.z);
			}
		}
	}

	private bool inObstacle(float x, float y) {
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
