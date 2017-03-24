using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct gameTile {
	public Vector2 position;
	public int distance;
}



public class AStar {
	public AStar(Vector2 start, Vector2 dest) {
		A = dest;
		B = start;
		directions.Add (Vector2.up);
		directions.Add (Vector2.left);
		directions.Add (Vector2.down);
		directions.Add (Vector2.right);
		gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
	}
		
	public List<Vector2> directions;
	public Vector2 A;
	public Vector2 B;
	public GameController gc;

	public List<Vector2> navigate() {
		
		List<gameTile> open = new List<gameTile> ();
		List<gameTile> closed = new List<gameTile> ();
		List<Vector2> finalPath = new List<Vector2> ();

		// add current position to closed list
		gameTile tile;
		tile.position = A;
		tile.distance = 0;
		closed.Add(tile);

		// add all manhattan adjacent tiles to open list if they are not in an obstacle
		foreach (Vector2 dir in directions) {

			if (!gc.inObstacle (dir + A)) {
				// create a new gameTile for this location
				tile.position = dir + A;
//				tile.distance = 
				open.Add (tile);
			} else {
				// should we add this spot to closed?
			}
		}

		return finalPath;
	}

	private int H(Vector2 loc) {
		// this is as simple as finding the manhattan distance from this position to the destination
		Vector2 diff = loc - B;
		return (int)(Mathf.Abs (diff.x) + Mathf.Abs (diff.y));
	}

//	private int G(Vector2 loc) {
//
//	}

//	private int F(Vector2 loc) {
//		return H (loc) + G (loc);
//	}
}
