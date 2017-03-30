using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct gameTile {
	public Vector3 position;
	public int G,F,id,parent;
}



public static class AStar {	

	public static List<List<List<int>>> navigate(Vector2 start, Vector2 dest, List<List<List<int>>> grid, int id) {

		GameController gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
		List<Vector3> directions = new List<Vector3>();

		directions.Add (new Vector3(0,1f,1f));
		directions.Add (new Vector3(-1f,0,1f));
		directions.Add (new Vector3(0,-1f,1f));
		directions.Add (new Vector3(1f,0,1f));
		directions.Add (new Vector3(0,0,1f)); // pause

		int maxID = 0;  // ids are used to break ties in terms of F distance
		gameTile currentTile;
		gameTile adjacentTile;

		Vector3 v3Start = new Vector3 (start.x, start.y, 0);
		Vector3 v3End = new Vector3 (dest.x, dest.y, (float)(grid [0] [0].Count - 1));

		// the initialization phase
		List<gameTile> open = new List<gameTile> ();
		List<gameTile> closed = new List<gameTile> ();

		// add current position to closed list
		currentTile.position = v3Start;
		currentTile.G = 0;
		currentTile.F = getH (currentTile.position, v3End) + currentTile.G;
		currentTile.id = maxID++;
		currentTile.parent = currentTile.id;
		closed.Add(currentTile);

		// add all manhattan adjacent tiles to open list if they are not in an obstacle
		for( int i = 0; i < 4; i++) {

			if (!gc.inObstacle (directions[i] + v3Start)) {
				// create a new gameTile for this location
				adjacentTile.position = directions[i] + v3Start;
				adjacentTile.G = 1;
				adjacentTile.F = getH (adjacentTile.position, v3End) + adjacentTile.G;
				adjacentTile.id = maxID++;
				adjacentTile.parent = 0;
				open.Add (adjacentTile);
			} else {
				// should we add this spot to closed?
			}
		}

		int currentTileIndex = 0;

		// now comes the actual algorithm

		// pathfind until we've included the destination tile in our final path
		while (findInList (v3End, closed) == -1) {
			// determine the next tile to inspect based on being the closest to destination
			int nextTileIndex = findLowestScoreIndex (currentTile.id, open);
			currentTileIndex = nextTileIndex;

			// retrieve this tile for inspection
			currentTile = open [nextTileIndex];

			// remove the tile from the open list
			open.RemoveAt(nextTileIndex);

			// add this tile to the closed list since it has been visited
			closed.Add (currentTile);

			// look in the four directions around this tile
			for (int i = 0; i < 4; i++) {
				Vector3 testPos = directions [i] + currentTile.position;

				// we can't add tiles that exist as part of an obstacle or are already in the closed list
				if (grid[(int)testPos.x][(int)testPos.y][(int)testPos.z] == 0 && findInList (testPos, closed) == -1) {
					// now that we know this is a viable tile, check if it's already been added
					int testIndex = findInList (testPos, open);

					if (testIndex == -1) {					
						// create a new gameTile for this location
						adjacentTile.position = testPos;
						adjacentTile.G = currentTile.G + 1;
						adjacentTile.F = getH (testPos, v3End) + adjacentTile.G;
						adjacentTile.id = maxID++;
						adjacentTile.parent = currentTile.id;
						open.Add (adjacentTile);
					} else {
						// we've seen this tile before, so check to see if it's new F value is
						// improved from the G based on the current position
						if (currentTile.G + 1 < open [testIndex].G) {
							adjacentTile = open [testIndex];
							// subtract from the F the difference between the new G and the old G
							adjacentTile.F -= (currentTile.G + 1 - adjacentTile.G);
							// update the G
							adjacentTile.G = currentTile.G + 1;

							// update the parent too
							adjacentTile.parent = currentTile.id;

							open [testIndex] = adjacentTile;
						}
					}
				}
			}
		}

		return buildPath(v3End,closed,grid, id);
	}

	public static int getH(Vector3 start, Vector3 dest) {
		// this is as simple as finding the manhattan distance from this position to the destination
		Vector3 diff = start - dest;
		return (int)(Mathf.Abs (diff.x) + Mathf.Abs (diff.y) + Mathf.Abs(diff.z));
	}

	public static int findLowestScoreIndex(int currentID, List<gameTile> tiles) {
		int min = int.MaxValue;
		int minIndex = 0;
		// return the index of the tile with the lowest score
		for (int i = 0; i < tiles.Count; i++) {
			if (tiles[i].F < min) {
				// save the new minimum
				min = tiles [i].F;
				minIndex = i;
			} else if (tiles[i].F == min && tiles[i].parent == currentID) {
				// break a tie by going with the tile adjacent to the most recently inspected tile
				min = tiles [i].F;
				minIndex = i;
			}
		}

		return minIndex;
	}

	public static int findInList(Vector3 loc, List<gameTile> tiles) {
		// cycle through the list and see if any tiles are at this location
		// return the index of the tile in the list if so, otherwise return -1
		for (int i = 0; i < tiles.Count; i++) {
			if (loc == tiles [i].position) {
				return i;
			}
		}

		return -1;
	}

	public static int findInList(int id, List<gameTile> tiles) {

		// overloaded to work with id rather than location

		// cycle through the list and see if any tiles are at this location
		// return the index of the tile in the list if so, otherwise return -1
		for (int i = 0; i < tiles.Count; i++) {
			if (id == tiles [i].id) {
				return i;
			}
		}

		return -1;
	}

	public static List<List<List<int>>> buildPath(Vector3 dest, List<gameTile> tiles, List<List<List<int>>> grid, int id) {
		// walk backwards from the destination tile to the starting tile, appending a time
		// (don't add the starting tile because we know where we are)

		int currentID = findInList (dest, tiles);
		int layer = 0;
		while (tiles [currentID].id != 0) {
			
			Vector3 newTile = tiles [currentID].position;
			grid[(int)newTile.x][(int)newTile.y][(int)newTile.z] = id;
			if (layer > 0) {
				grid [(int)newTile.x] [(int)newTile.y] [(int)newTile.z + 1] = id;
			}
			layer++;
			// update the current id
			currentID = findInList(tiles [currentID].parent,tiles);
		}

		return grid;

	}
}
