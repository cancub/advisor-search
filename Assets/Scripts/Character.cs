using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct professor
{
	public int id;
	public Vector2 location;
}

public class Character : MonoBehaviour {
	private GameController gc;

	//  **************behaviour tree variables***************

		// ids
	private int myID;
	private int lastProfID;			// the id of the last prof that was spoken to
	private int currentProfID;		// the id of the prof that we actually need to talk to
	private int randomProfID;		// when we don't know the location of currentProf, pick a random prof and check their plaque

		// flags
	private bool goToProf;				// flag to show that the character is on its way to a prof
	private bool isTalking;			// flag to show that the character is currently talking to a prof
	private bool goToWait;				// flag to show that the character must wait in a random spot
	private bool isWaiting;			// flag to show that the character is currently waiting in a random spot

		// times
	private float waitTime;			// the time that the character started waiting at a designated location
	private float blockedTime;		// the time that the character started being blocked from moving
	private float minWaitTime;		// 0.5s
	private float maxWaitTime;		// 2.0s
	private float minTalkTime;		// 0.5s
	private float annoyedTime;		// 0.1s
	private float frustratedTime;	// 3.0s

	// path related
	private float moveSpeedPercent;	// the percent of the distance between tiles that the character will move in each update
	private Vector3 trajectory;
	private List<Vector3> path;		// the remaining steps in the path to reach the destination, third dimension is time
	private Vector3 nextTile;
	private Vector3 lastPosition;    // the last 2D space occupied by the character
	private Vector3 currentPosition; // the current position in two dimensions plus time
	private int pathStep;
	private Vector2 unknownDest;	// the reference destination for not knowing where we're going
	private Vector2 finalDest;		// the location that the character is ultimately trying to route to
	private Vector3 windowDest;		// the current destination of this character in this path window
	private float closeEnoughDistance; // a check for the unreliable == operator with Vector2/3
	private List<professor> knownProfessors;
	private bool pathPrecendence;	// false for mobile
									// true for stationary at either a designated waiting spot or at prof

		// materials
	public Material calmMaterial;
	public Material annoyedMaterial;
	public Material frustratedMaterial;



	// Use this for initialization
	void Start () {
		gc = (GameController)GameObject.Find ("GameController").GetComponent (typeof(GameController));
		transform.position = gc.emptyPointInGame ();
		myID = gc.getCharacterID ();
		print ("START -- " + myID.ToString() + ": (" + transform.position.x.ToString()+"," + transform.position.y.ToString()+")");

//		print (myID);
//		transform.position = new Vector3(-6.5f,0.5f,0);

		// ensure that each character has a different random number generator

		lastProfID = int.MaxValue;
		currentProfID  = gc.getRandProfID();
		print ("searching for " + currentProfID.ToString ());

		moveSpeedPercent = gc.getMoveSpeed ();

		// the character will start off by looking for a specific plaque (i.e., not yet walking to a prof,
		// not talking to the prof, not yet walking to a designated spot and not waiting in a designated spot)
		goToProf = false;
		isTalking = false;
		isWaiting = false;
		goToWait = false;

		minWaitTime = 0.5f;
		maxWaitTime = 2.0f;
		minTalkTime = 0.5f;
		annoyedTime = 0.1f;

		frustratedTime = 3.0f;

		waitTime = float.MaxValue;
		blockedTime = float.MaxValue;

		unknownDest = new Vector2 (100f, 100f);
		finalDest = unknownDest;
		windowDest = unknownDest;
		currentPosition = unknownDest;

//		currentPosition = new Vector3 (0, 0, 0);

		knownProfessors = new List<professor> ();
		path = new List<Vector3> ();

		closeEnoughDistance = 0.0001f;

	}


	
	// Update is called once per frame and will run the root of the behaviour tree
	void Update () {

		BehaviourTree ();

	}

	private bool BehaviourTree() {
		// hardcode the decision tree, it's going to be too much effort to generalize it
		// the root is a selector
		if (!waitingBranch ()) {
			if (!gettingAdviceBranch ()) {
				if (!updateDestinationBranch ()) {
					if (!planRouteBranch ()) {
						return movementBranch ();
					}
				}
			}
		}

		return false;
	}



	/**********************************waiting branch ******************************************
	this section is specifically related to the waiting in a random spot prior to moving on to a prof
	*/

	private bool waitingBranch() {
		// this branch determines if the character has been waiting at their designated random spot,
		// checks if they should be finished and decides what to do next

		// runs as a sequence
		if (isWaiting) {
			// currently waiting
			if (waitTimerFinished ()) {
				// should no longer be waiting
				// the remainder of the branch should always return true
				if (resetTimer ()) {
					if (resetWaitingFlags ()) {
						// this character will now be moving, and so does not need to be given priority over
						// their spot
						resetPathPrecedence();
					}
				}
//				path = new List<Vector3> ();
				// let the character immediately move
				return false;
			}
			return true;
		}

		return false;
	}

	private bool waitTimerFinished() {
		return Time.time > (waitTime + minWaitTime);
	}

	/**********************************advice branch ******************************************
	this section is specifically related to the standing next to the prof to get advice
	*/

	private bool gettingAdviceBranch() {
		// checks if a player should be waiting next to a prof and releases them for that wait if need be

		// runs as a sequence
		if (getIsTalkingFlag ()) {
			// currently talking
			if (talkTimerFinished ()) {
				// should no longer be talking
				// the remainder of the branch should always return true
				if (resetTimer ()) {
					if (resetIsTalkingFlag ()) {
						// should now be moving
						if (resetPathPrecedence()) {
							if (saveProfID()) {
								// remember this most recent prof so that we can skip over the
								// plaque we will pass by on the way out to the new prof
								if (getNextProfID()) {
									// selector subbranch
									if (processID()) {
										// the prof's location was not known

									}
									processRandomWait();
								}
							}
						}
					}
				}
//				path = new List<Vector3> ();
				// we want to let the character move immediately after we've determined that they have finished talking 
				return false;
			}
			return true;
		}

		return false;

	}

	public bool talkTimerFinished() {
		float currentTime = Time.time;
		return currentTime > (waitTime + minTalkTime);
	}

	public bool saveProfID() {
		lastProfID = currentProfID;
		return lastProfID == currentProfID;
	}

	public bool getNextProfID() {

		// randomly select another prof to talk to
		currentProfID = gc.getRandProfID();
		while (currentProfID == lastProfID) {
			currentProfID = gc.getRandProfID();
		}

		print ("searching for " + currentProfID.ToString ());

		return true;
	}

	public bool processID() {
		// here we figure out if we know where the new prof is and if we should be waiting

		// subbranch implemented as a sequence
		if (profLocationKnown (currentProfID)) {
			// heading directly to a prof
			return setGoToProfFlag ();
		} else {
			resetGoToProfFlag();
		}

		return false;
	}

	private bool processRandomWait() {
		if (!goingToWait ()) {
			return resetGoToWaitFlag ();
		}

		return false;
	}

	public bool goingToWait() {
		// with 50% likelihood, the character will decide to go wait at a random spot anywhere in the level
		if (shouldWait()) {
			print ("will chill out for a bit");
			return setGoToWaitFlag ();
		}

		return false;
	}

	public bool shouldWait() {
		return Random.value > 0.5f;
	}




	/**********************************path update branch ******************************************
	this section is specifically related to the obtaining of a new path if need be
	*/


	public bool updateDestinationBranch() {
		// a path will be provided to the character is if does no have one
		// this path is related to the flags that are currently set

		// selector
		if (!destinationKnown ()) {
			if (!setWaitDestination ()) {
				// not supposed to wait
				if (!setProfDestination ()) {
					// don't know where the prof is, so go to an plaque that we don't know about

					// to make the character move smoothly, there shouldn't be a break between
					// the obtaining of a path and the movement, so return false so that the selector 
					// can continue
					return !setUnknownPlaqueDestination();
				}
			}
		}

		return false;
	}

	public bool destinationKnown() {
		return finalDest != unknownDest;
	}

	public bool setWaitDestination() {
		// sequence
		if (goToWait) {
			// supposed to wait so set the next destination
			// will be true
			return setRandomDestination();
		}

		return false;
	}

	private bool setRandomDestination() {
		Vector2 randomDest = gc.emptyPointInGame ();
		finalDest = randomDest ;
		return finalDest == randomDest;
	}

	private bool setProfDestination() {
		// check if we're supposed to be directly to a prof and then set the destination to that prof

		// sequence
		if (goToProf) {
			// know where the prof is, so set the destination
			// will return true
			return readProfDestination();
		}
		return false;
	}

	private bool readProfDestination() {
		// can only get so close to prof, don't want any ethics problems
		finalDest = gc.getClosestSpot(currentProfID);
		// returns true if we already knew about this prof's location
		return finalDest != unknownDest;
	}

	private bool setUnknownPlaqueDestination() {
		// sequence
		if (getUnknownPlaque ()) {
			return setPlaqueDestination ();
		}
		return false;
	}

	private bool getUnknownPlaque() {

		// select from the ids of unknown professor locations to get the next plaque to visit
		randomProfID = gc.getRandProfID();

		while (profLocationKnown (randomProfID)) {
			randomProfID = gc.getRandProfID();
		}

		print ("searching for " + currentProfID.ToString() + ", will check out " + randomProfID.ToString ());

		return true;
	}


	private bool setPlaqueDestination() {
		Vector2 plaqueDest = gc.getPlaque(randomProfID).standingLocation;
		finalDest = plaqueDest;
		return finalDest == plaqueDest;
	}



	/**********************************route planning ******************************************
	this section obtains a route from the gamecontroller
	*/

	private bool planRouteBranch () {
		if (!stepsRemain ()) {
			if (getRoute ()) {
				if (updateMovement ()) {

				}
			}
		}

		return false;
	}

	private bool stepsRemain() {
		return !((currentPosition == windowDest) && (pathStep == path.Count));
	}

	private bool getRoute() {
		// build the path from the current position to final destination
		// (we add in our own time for the moment)

		// note that going to a random position or a plaque requires an exact location while going to a 
		// prof requires an adjacent position

		// if your must recent planning only took you up to the plaque and that's it, you just have to take what you can
		// get for movement in the remainder of the window. That is, everyone else has already planned their routes in 3D
		// space and if you were only able to plan 3 moves ahead because you had yet to figure out if this was the right plaque
		// then you poll for 2 more movements and hope for the best
		int newPathSize = 5-path.Count;
		if (newPathSize == 0 || newPathSize == 5) {
			path = gc.obtainCombinedPath ((Vector2)(transform.position), finalDest, 5);
			pathStep = 0;
			currentPosition = transform.position;
			currentPosition.z = 0;
		} else {
			List<Vector3> newPath = gc.obtainCombinedPath ((Vector2)(transform.position), finalDest, newPathSize);
			path.AddRange (newPath);
			// continue on as if nothing happened
		}
//		print ("END -- " + myID.ToString() + ": (" + end.x.ToString()+"," + end.y.ToString()+")");





		windowDest = path [path.Count - 1];
//		print ("WINDOW -- " + myID.ToString() + ": (" + windowDest.x.ToString()+"," + windowDest.y.ToString()+")");

		return true;
	}



	/**********************************movement to important locations ************************************
	this section takes care of the movement of the character as well as the events that we either 
	see a plaque, make it to the prof or reach the random spot on the map 
	*/

	private bool movementBranch () {
		// move, then test for one of three, non overlapping situations of reaching a plaque, prof or wait spot

		// sequence
		if (attemptMovement()) { 
			return checkImportantLocations ();
		}

		return false;
	}

	private bool attemptMovement() {
		// attempt to move and be calm if you are able to move
		//selector
		if (makeMovement ()) {
			return noMoreFrustration ();
		}
		return true;
	}

	private bool makeMovement () {
		// sequence to move in spacetime and check to see if at the same 2D location
		if (saveCurrentSpaceTime()) {
			if (moveToNextSpaceTime ()) {
				if (atSameSpace ()) {
					return increaseFrustration ();
				}
			}
		}

		return false;
	}

	private bool saveCurrentSpaceTime() {
		// we need to save this to know if we are actually moving in 2D space
		lastPosition = currentPosition;
		return lastPosition == currentPosition;
	}

	private bool moveToNextSpaceTime() {
		// sequence
		if (moveCharacter ()) {
			if (atNextTile ()) {
				return updateMovement ();
			}
		}

		return false;
	}

	private bool moveCharacter() {
		// move along by the increment amount to the next 3D tile (space_x, space_y,time)
		currentPosition += trajectory;
		// nullify the time when actually moving the character
		Vector3 tempPosition = currentPosition;
		tempPosition.z = 0;
		transform.position = tempPosition;
		return transform.position == tempPosition;
	}



	private bool atNextTile() {
		// sometimes the == operator does not return true when the character is technically in the right
		// position, so we'll check to see if it's incredibly close and then lock it in place

		bool result = false;


		if ((currentPosition - nextTile).magnitude < closeEnoughDistance) {
			// close enough

			// lock into place
			Vector3 tempPosition = nextTile;
			tempPosition.z = 0;
			transform.position = tempPosition;
			result = true;
		}

		return result;
	}

	private bool atSameSpace () {
		return ((Vector2)currentPosition - (Vector2)lastPosition).magnitude < closeEnoughDistance;
	}

	private bool increaseFrustration() {
		// sequence
		if (setBlockedTimer ()) {
			return changeColour ();
		}

		return true;
	}

	private bool changeColour() {
		// set the frustration level according to the amount of time we've been blocked
		float totalTime = Time.time - blockedTime;
		if (totalTime > annoyedTime) {
			if (totalTime > frustratedTime) {
				// change the furstrated colour
				GetComponent<Renderer>().material = frustratedMaterial;
			} else {
				// change to annoyed colour
				GetComponent<Renderer>().material = annoyedMaterial;
			}
		} else {
			// cool as a cucumber
			GetComponent<Renderer>().material = calmMaterial;
		}

		return true;
	}

	private bool noMoreFrustration () {
		if (resetBlockedTimer ()) {
			return changeColour ();
		}

		return true;
	}

	private bool setBlockedTimer() {
		// start the timer if it we just began to be blocked
		if (blockedTime == float.MaxValue) {
			blockedTime = Time.time;
		}
		return true;
	}

	private bool resetBlockedTimer() {
		blockedTime = float.MaxValue;
		return blockedTime == float.MaxValue;
	}


	/**********************************arrivals ******************************************
	this section is very important in that it will set a lot of the flags that will dictate the
 	functionality of the character based on certain events
	*/


	private bool checkImportantLocations() {
		// check to see if we have arrived at any sought-after objects/locations
		if (!foundUnknownPlaque ()) {
			if (!foundProf ()) {
				return foundWaitSpot ();
			}
		}
		return true;
	}

	private bool foundUnknownPlaque() {
		// will return true if the character has found a new plaque, with the innards of the branch
		// determining if the character will use this to move directly to the next prof or if more searching
		// is needed

		//sequence
		if (!goToProf && !goToWait) {
			// must be searching for a plaque (i.e. not going directly to a prof)
			if (adjacentToNewPlaque()) {
				if (setDistinationInfo ()) {
					print ("found plaque");
					return addProfInfo();
				}
			}
		}

		return false;
				
	}

	private bool adjacentToNewPlaque() {
		// we might be next to a plaque, but it should be a new plaque (i.e. not the plaque
		// for the prof that we just talked to)

		if (isAdjacent ((Vector2)finalDest, 0)) {
			return gc.readProfNumber ((Vector2)finalDest) != lastProfID;
		}
		return false;
	}

	private bool setDistinationInfo() {
		// read the id from the plaque
		int newID = gc.readProfNumber ((Vector2)finalDest);

		// notify the route planning parts of the tree that we have found the plaque related to this
		// prof and can now go directly to aid prof
		if (newID == currentProfID) {
			if (setGoToProfFlag ()) {
				// maybe show a popup at some point to show that the character is happy
			}
		} 

		// let the other branch find this info
		finalDest = unknownDest;

		return true;
	}

	private bool addProfInfo() {

		if (makeKnowledgeSpace()) {
			return addNewProfessor ();
		}

		return false;
	}

	private bool makeKnowledgeSpace() {
		if (knownProfessors.Count == 4) {
			// pop the oldest from the front of the list
			knownProfessors.RemoveAt(0);
		}

		return true;
	}

	private bool addNewProfessor() {
		professor newProf;
		int newID = gc.readProfNumber ((Vector2)finalDest);

		// notify the route planning parts of the tree that we have found the plaque related to this
		// prof and can now go directly to aid prof
		if (newID == currentProfID) {
			newProf.id = currentProfID;
			newProf.location = gc.getProfLocation (currentProfID);
		} else {
			newProf.id = randomProfID;
			newProf.location = gc.getProfLocation (randomProfID);
		}


		knownProfessors.Add (newProf);
		foreach (professor prof in knownProfessors) {
			print ("known: " + prof.id.ToString ());
		}
		return true;
	}

	private bool foundProf () {
		if (goToProf && !goToWait) {
			if (atFinalSpot()) {
				if (setTimer ()) {
					if (setPathPrecedence ()) {
						if (setIsTalkingFlag ()) {
							finalDest = unknownDest;
							return true;
						}
					}
				}
			}
		}

		return false;
	}

	private bool atFinalSpot(){
		bool result = false;


		if (((Vector2)currentPosition - finalDest).magnitude < closeEnoughDistance) {
			// close enough

			// lock into place
			transform.position = finalDest;
			result = true;
		}

		return result;
	}

	private bool foundWaitSpot() {
		if (goToWait) {
			if (atFinalSpot()) {
				if (setTimer ()) {
					if (setPathPrecedence ()) {
						if (setIsWaitingFlag ()) {
							finalDest = unknownDest;
							return true;
						}
					}
				}
			}
		}

		return false;
	}


	/**********************************common tools ******************************************
	this section is specifically related to functions that are common to at least two branches
	*/

	private bool resetTimer() {
		waitTime = float.MaxValue;
		return waitTime == float.MaxValue;
	}

	private bool setTimer() {
		waitTime = Time.time;
		return true;
	}

	private bool setPathPrecedence() {
		// order or precendence for path planning should be stationary before mobile
		pathPrecendence = true;
		gc.setPriority (myID, pathPrecendence);
		return pathPrecendence;
	}

	private bool resetPathPrecedence() {
		pathPrecendence = false;
		gc.setPriority (myID, pathPrecendence);
		return !pathPrecendence;
	}

	public bool profLocationKnown(int id) {
		// look at the id of the current prof and see if it shows up anywhere in the list
		// of known professor locations
		foreach (professor prof in knownProfessors) {
			if (id == prof.id) {
				return true;
			}
		}
		return false;
	}



	/***prof sets/resets ****/

	private bool resetAdviceFlags() {
		if (resetGoToProfFlag ()) {
			return resetIsTalkingFlag();
		}
		return false;
	}

	public bool resetGoToProfFlag() {
		goToProf = false;
		return !goToProf;
	}

	public bool resetIsTalkingFlag() {
		isTalking = false;
		return !isTalking;
	}

	private bool setAdviceFlags() {
		if (setGoToProfFlag ()) {
			return setIsTalkingFlag();
		}
		return false;
	}

	public bool setGoToProfFlag() {
		goToProf = true;
		return goToProf;
	}

	public bool setIsTalkingFlag() {
		isTalking = true;
		return isTalking;
	}

	private bool getGoToProfFlag() {
		return goToProf;
	}

	public bool getIsTalkingFlag() {
		return isTalking;
	}

	/***waiting sets/resets ****/

	private bool resetWaitingFlags() {
		// aren't on our way to waiting and no longer are waiting
		// all should return true;
		if (resetGoToWaitFlag ()) {
			return resetIsWaitingFlag ();
		}
		return false;
	}

	private bool resetGoToWaitFlag() {
		goToWait = false;
		return !goToWait;
	}

	private bool resetIsWaitingFlag() {
		isWaiting = false;
		return !isWaiting;
	}

	private bool setWaitingFlags() {
		if (setGoToWaitFlag ()) {
			return setIsWaitingFlag ();
		}
		return false;
	}

	private bool setGoToWaitFlag() {
		goToWait = true;
		return goToWait;
	}

	private bool setIsWaitingFlag() {
		isWaiting = true;
		return isWaiting;
	}

	private bool getGoToWaitFlag() {
		return goToWait;
	}

	private bool getIsWaitingFlag() {
		return isWaiting;
	}

	private bool updateMovement() {
		if (pathStep == path.Count) {
//			print ("deal with this condition of going too far on the path");
			return false;
		} else {
			nextTile = path [pathStep++];
			trajectory = nextTile - currentPosition;
			trajectory.x *= moveSpeedPercent;
			trajectory.y *= moveSpeedPercent;
			trajectory.z *= moveSpeedPercent;
//			if (((Vector2)trajectory).magnitude > moveSpeedPercent) {
//				print (((Vector2)trajectory).magnitude.ToString() + " = going too fast!!!!!!!!!!!!!!!!!!!");
//			}
		}

		return true;
	}

	private bool isAdjacent(Vector2 loc, int type) {
		// adjacency means that the distance between this character's center and the other
		// object is less than 2 radius (2*0.5f)

		// for the plaque (type 0), this means directly measuring the distance from the midpoint
		// of the plaque surface to the character
		if (type == 0) {
			return ((Vector2)transform.position - loc).magnitude < 1f;
		} else if (type == 1) {
			// for the prof(type 1), this means that their centers are at most 3 radii apart
			return ((Vector2)transform.position - loc).magnitude < 1.5f;
		}

		return false;
	}










//	private void checkForPlaques() {
//		// maybe we are at a spot where we can see a plaque
//		if (canReadPlaque(currentPlaqueIndex)) {
//			// we can read the plaque, so read it and get the info about the professor
//			if (plaques[currentPlaqueIndex].profNumber == -1) {
//				updateKnowledgeBase ();
//			}
//
//			// the path is now pointing to the professor
//			getRoute (nextTile,profLocations[plaques[currentPlaqueIndex].profNumber]);
//			// and the next update of the walking direction will be set back to the start of the path
//		}
//	}
//
//	private bool canReadPlaque(int index) {
//		// let us know if we can read the plaque
//		if (isAdjacent(plaques[index].nameLocation,0) && plaqueVisible (index)) {
//			return true;
//		}
//		return false;
//	}
//
//	private bool plaqueVisible(int index) {
//		// check to see if the line segments describing the character to the plaque surface and the wall
//		// that the plaque rests on have an intersection
//		return !Visibility.isHidden((Vector2)transform.position, plaques[index].nameLocation,
//			plaques[index].plaqueWall.p,plaques[index].plaqueWall.q);
//	}
//
////	private void getRoute(Vector2 start, Vector2 end) {
////		// figure out where we need to go
////		finalDest = end;
////
////		// build the path from the current position to there
////		path = AStar.navigate (start, finalDest, pathWindow);
////		print ("END -- " + myID.ToString() + ": (" + end.x.ToString()+"," + end.y.ToString()+")");
////
////		pathStep = 0;
////
////		windowDest = path [path.Count - 1];
////		print ("WINDOW -- " + myID.ToString() + ": (" + windowDest.x.ToString()+"," + windowDest.y.ToString()+")");
////
////	}
//
//	private void updatePath() {
//		// set everything up for the next movement
//		nextTile = path[pathStep++];
//		print ("STEP -- " + myID.ToString() + ": (" + nextTile.x.ToString()+"," + nextTile.y.ToString()+")");
//		updateMove = moveSpeedPercent * (nextTile - (Vector2)transform.position).normalized;
//	}
//
//	private void takeStep() {
//		// move along the path to the next tile
//		transform.position += updateMove;
//		print (myID.ToString() + ": (" + transform.position.x.ToString()+"," + transform.position.y.ToString()+")");
//	}
//
//	private bool isAdjacent(Vector2 loc, int type) {
//		// adjacency means that the distance between this character's center and the other
//		// object is less than 2 radius (2*0.5f)
//
//		// for the plaque (type 0), this means directly measuring the distance from the midpoint
//		// of the plaque surface to the character
//		if (type == 0) {
//			return ((Vector2)transform.position - loc).magnitude < 1f;
//		} else if (type == 1) {
//			// for the prof(type 1), this means that their centers are at most 3 radii apart
//			return ((Vector2)transform.position - loc).magnitude < 1.5f;
//		}
//
//		return false;
//	}
//
//	private void updateKnowledgeBase () {
//		// update the plaque information for the current path, specifically the prof id seen on the
//		// plaque
//		plaque tempPlaque = plaques[currentPlaqueIndex];
//		tempPlaque.profNumber = gc.readProfNumber (tempPlaque.nameLocation);
//		plaques [currentPlaqueIndex] = tempPlaque;
//
//		// update the professor location based on the prof id learned from viewing the plaque
//		profLocations[tempPlaque.profNumber] = gc.getProfLocation(tempPlaque.profNumber);
//
//		//print (tempPlaque.profNumber);
//		//print (profLocations [tempPlaque.profNumber]);
//	}
}
