using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveX : MonoBehaviour {
	private GameController gc;

	void Start() {
		// hide the object by placing it in the center
		transform.position = new Vector3(2.5f,2.5f,0);
		gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
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
			if (!gc.inObstacle(x,y)){
				transform.position = new Vector3 (x, y, transform.position.z);
			}
		}
	}


}
