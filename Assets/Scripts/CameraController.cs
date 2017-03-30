using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	private Camera mainCam;
	private double initialScale;
	public float zoomStep;
	public float moveStep;

	// Use this for initialization
	void Start () {
		mainCam = Camera.main;
		initialScale = mainCam.orthographicSize;

	}
	
	// Update is called once per frame
	void Update () {
		moveWASD ();
		zoomInOut ();
	}

	private void moveWASD() {
		// check to see which of the WASD keys were pressed and move in the cumulative direction
		Vector3 totalDisplacement = new Vector3(0,0,0);

		if (Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.UpArrow)) {
//			print ("up");
			totalDisplacement += Vector3.up;
		}

		if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
			totalDisplacement += Vector3.left;
		}

		if (Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.DownArrow)) {
			totalDisplacement += Vector3.down;
		}

		if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
			totalDisplacement += Vector3.right;
		}

		// scale the movement based on how zoomed in we are, so that the camera will pan by the same relative amount
		totalDisplacement *= (float)(mainCam.orthographicSize/initialScale) * moveStep;

		Vector3 newPosition = mainCam.transform.position;

		newPosition += totalDisplacement;

		mainCam.transform.position = newPosition;

	}

	private void zoomInOut () {
		var d = Input.GetAxis("Mouse ScrollWheel");
		if (d > 0f)
		{
			//decrease the size of the camera to simulate zooming in
			mainCam.orthographicSize -= zoomStep;
		}
		else if (d < 0f)
		{
			// scroll down
			mainCam.orthographicSize += zoomStep;
		}
	}
}
