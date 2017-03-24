using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveX : MonoBehaviour {
	public GameObject obstacles; 

	void Start() {
		// hide the object by placing it in the center
		transform.position = new Vector3(2.5f,2.5f,0);
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

			//				print (x.ToString() + "," + y.ToString());

			transform.position = new Vector3 (x, y, transform.position.z);
		}
	}

	private bool inObstacle(float x, float y) {
		foreach (Transform child in obstacles.transform) {
			// get the rectangle that describes the top surface of the object

		}
	}
}
