/// <summary>
/// Npc ai.
/// Basic state machine ai for personal use.
/// David ONeill
/// 7/12/15
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcAi : MonoBehaviour {

	public bool targeting = false;
	public GameObject myTarget = null;
	public Vector3 pointOfInterest,guardPost; // shiny? or last sighting of enemy
	public float detectRange = 10f;//Range of detection sphere
	public float attackTimer,attackTime = 2f; //attackCooldown
	[Header("-Navigation-")]
	public float attackRange = 2f; //range for attacks
	public float inspectRange = 2f; //range for invesivate/idle etc
	public float patrolSpeed, combatSpeed; // walkspeed/runspeed
	public float patrolAccel, combatAccel; // nav agent acceration contol
	//public LayerMask detect;
	public GameObject path;//waypoint collection
	public enum RouteType
	{
		Loop,
		Returning,
		//Random,  // later
		Line
	}
	public RouteType pathType;
	[HideInInspector]
	public bool finishedRoute;
	public List<GameObject> myWps = new List<GameObject>();
	[Header("-Timer-")]
	public float waitTime,searchTime, currentTime; //wait time when idle, wait time when searching.
	//
	public enum State
	{
		Init,
		Idle,
		Investigate,
		Patrol,
		Chase,
		Attack,
		Dead
	};
	[Header("-Ai State-")]
	public State currentState;

	//Animator Stuff
	[Header("-Animation-")]
	Vector3 currentPosition;
	Vector3 previousPosition;
	public Animator anim;
	float currentSpeed; //Anim speed float

	//Grounded
	bool grounded = false;
	//PRIVATES
	NavMeshAgent nav;
	[HideInInspector]
	public int index; // WP Enumeration
	//for rotation
	Quaternion _lookRotation;
	Vector3 _direction;

	// make custom update?- would need to restructure to facilitate timekeeping/lerps
	void Update () 
	{
		switch(currentState)
		{
		case State.Init:
			Init();
			break;
		case State.Idle:
			Idle();
			break;
		case State.Investigate:
			Investigate();
			break;
		case State.Patrol:
			Patrol ();
			break;
		case State.Chase:
			Chase();
			break;
		case State.Attack:
			Attack();
			break;
		default:
			Debug.Log ("WTF!!!");
			break;
		}
	
		if(anim != null)
		{
			AnimatorInput ();
		}
	}

	// Use this for initialization
	void Init () 
	{
		nav = GetComponent<NavMeshAgent> ();
		nav.updateRotation = true;

		//currentTime = waitTime;
		attackTimer = attackTime;

		//find whole wp routes and populate
		if(path != null)
		{
			CreateWayPointRoute(path);
		}

		if(grounded){}// Thoughts, need to be instanciated on the navmesh before disabling GO/nav agent for pooling.

		if(myWps.Count > 0)
		{
			nav.SetDestination (myWps [0].transform.position) ;
			currentState = State.Patrol;
		}
		else
		{
			if (guardPost == Vector3.zero) 
			{
				Debug.Log (this.gameObject.name + " says"+ " I have no GuardPost.... So guess il just hang out here......");
				guardPost = transform.position; // This should be assigned by spawner or handler
				currentState = State.Idle;
			} 
			else 
			{
				currentState = State.Idle;
			}
		}
	}

	//Can call this eternally to assign a new Path - Does this need additional logic?
	public void AssignNewRoute(GameObject path)
	{
		CreateWayPointRoute (path);
		currentState = State.Idle;
	}

	void CreateWayPointRoute(GameObject myPath)
	{
		path = myPath;
		myWps.Clear ();
		Transform [] tr = path.gameObject.GetComponentsInChildren<Transform> ();
		foreach(Transform t in tr)
		{
			if(t.gameObject.name.Contains ("WayPoint"))
			{
				GameObject g = t.transform.gameObject;
				myWps.Add (g);
			}
		}
	}

	void Idle()
	{
		if(nav.stoppingDistance != inspectRange)
		{
			nav.stoppingDistance = inspectRange;
			nav.acceleration = patrolAccel;
		}

		nav.Resume ();

		currentTime -= Time.deltaTime;
		if(currentTime <= 0)
		{
			currentTime = 0;
		}

		if (currentTime <= 0 & path != null) //if we have a path of Wps
		{
			//Add Enum for patrol types - line/pingpong/loop?
			if (pathType == RouteType.Loop) 
			{
				//Loop array/List back to the start.
				if (index == myWps.Count - 1)
				{
					index = 0;
				} 
				else 
				{//Increment the index
					index++;
				}
			} 
			else if (pathType == RouteType.Line) 
			{
				if (index == myWps.Count - 1)//We are at the end of the line
				{
					if (guardPost != myWps [index].transform.position) 
					{
						guardPost = myWps [index].transform.position;
					} 
					path = null; //not sure about about this 
				} 
				else 
				{//Increment the index
					index++;
				}
			}
			else if(pathType == RouteType.Returning)
			{
				if (index == myWps.Count - 1)
				{
					finishedRoute = true;
					index = myWps.Count - 1;
				} 
				else if (index == 0)
				{//Increment the index
					finishedRoute = false;
					index = 0;
				}

				if (finishedRoute) 
				{
					index--;
				}
				else
				{
					index++;
				}
			}
		

			currentTime = waitTime;
			nav.SetDestination (myWps [index].transform.position);
			currentState = State.Patrol;
		}
		else if(currentTime <= 0 && path == null) //if we have no Wps
		{
			if(Vector3.Distance(transform.position,guardPost) > nav.stoppingDistance)
			{
				//Debug.Log ("Back to Guard");
				nav.SetDestination(guardPost);
			}
		}

		if(myTarget != null)
		{
			SetTarget(myTarget);
		}
	}

	//For externally sending ai to check a location 
	public void GoInvestigate(Vector3 interest)
	{
		pointOfInterest = interest;
		currentTime = searchTime; //NB
		currentState = State.Investigate;
	}

	//for last sightings and heard noises
	void Investigate()
	{
		nav.acceleration = patrolAccel;
		nav.stoppingDistance = inspectRange;
		nav.Resume ();
		currentTime -= Time.deltaTime;
		if(currentTime <= 0)
		{
			currentTime = 0;
		}

		nav.SetDestination (pointOfInterest);

		if (nav.remainingDistance < nav.stoppingDistance)
		{
			if(currentTime <= 0.1f)
			{
				//Debug.Log("Finished Snooping !" + currentTime + "  " + Vector3.Distance (transform.position, pointOfInterest) + "POI");
				pointOfInterest = Vector3.zero;
				nav.speed = patrolSpeed;
				currentState = State.Idle;
			}
		}
	}

	//Patrol behaviour.
	void Patrol()
	{
		nav.Resume ();

		nav.speed = patrolSpeed;
		if (nav.remainingDistance < nav.stoppingDistance)
		{
			currentState = State.Idle;
		}

		if(myTarget != null)
		{
			SetTarget(myTarget);
		}

		nav.SetDestination (myWps [index].transform.position);
	}
	//End of WP
	
	public void SetTarget(GameObject tar)
	{
		myTarget = tar;

		if(myTarget != null)
		{
			targeting = true;

			nav.speed = combatSpeed;
			nav.acceleration = combatAccel;
			//anim.SetBool ("");
			currentState = State.Chase;
		}
	}

	void Chase()
	{
		nav.Resume ();
		nav.stoppingDistance = attackRange - attackRange/4;

		if(myTarget)
		{
			nav.SetDestination(myTarget.transform.position);

			//check to see if target is in line of sight here
			Vector3 direction = myTarget.transform.position - transform.position;
			RaycastHit hit;
			
			if(Physics.Raycast(transform.position + transform.up,direction.normalized, out hit,attackRange))
			{
				Vector3 s = new Vector3(hit.point.x,hit.point.y + 1f,hit.point.z);
				Debug.DrawLine(transform.position,s);
				if(hit.collider.gameObject == myTarget)
				{
					Debug.Log ("CanAttack");
					currentState = State.Attack;

				}
			}
		}

		//Disengage handles by Senses
	}

	float CheckDistance(GameObject from,GameObject to )
	{
		float distance = 0;
		distance = Vector3.Distance (from.transform.position, to.transform.position);
		return distance;
	}

	void Attack()
	{
//		if(myTarget == null)//safety
//		{
//		}

		nav.Stop ();
		float dist = CheckDistance (this.gameObject, myTarget);

		if(dist >= attackRange) //AttackRange check
		{
			Debug.Log ("!" + dist);
			currentState = State.Chase;
		}
		else // we have range!
		{
			//Attack Coroutine
			attackTimer -= Time.deltaTime;
			if(attackTimer <= 0)
			{
				attackTimer = attackTime;
				//Debug.Log ("Poke!");
			}
			RotateTowordsTarget();
		}
	}

	void RotateTowordsTarget() //switch to turret method?
	{
		Vector3 direction = (myTarget.transform.position - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		Quaternion aimRotation = new Quaternion (transform.rotation.x, lookRotation.y, transform.rotation.z, lookRotation.w);
		transform.rotation = Quaternion.Slerp(transform.rotation, aimRotation, Time.deltaTime * 10f);
	}

	void AnimatorInput()
	{
		currentSpeed = Vector3.Project (nav.desiredVelocity, transform.forward).magnitude;
		float angle = FindAngle (transform.forward, nav.desiredVelocity, transform.up);
		//float a = angle

		anim.SetFloat ("Speed", currentSpeed);
		anim.SetFloat ("Angle", angle);
	}

	float FindAngle (Vector3 fromVector, Vector3 toVector, Vector3 upVector)
	{
		// If the vector the angle is being calculated to is 0...
		if(toVector == Vector3.zero)
		{
			// ... the angle between them is 0.
			return 0f;
		}
		// Create a float to store the angle between the facing of the enemy and the direction it's travelling.
		float angle = Vector3.Angle(fromVector, toVector);
		// Find the cross product of the two vectors (this will point up if the velocity is to the right of forward).
		Vector3 normal = Vector3.Cross(fromVector, toVector);
		// The dot product of the normal with the upVector will be positive if they point in the same direction.
		angle *= Mathf.Sign(Vector3.Dot(normal, upVector));
		// We need to convert the angle we've found from degrees to radians.
		angle *= Mathf.Deg2Rad;
		
		return angle;
	}
	
}//End
