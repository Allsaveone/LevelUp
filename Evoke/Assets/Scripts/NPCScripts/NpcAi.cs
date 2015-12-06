/// <summary>
/// Npc ai.
/// Basic state machine ai for TSS prototype
/// David ONeill
/// 3/10/15
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcAi : MonoBehaviour {

	public bool targeting = false;
	public GameObject myTarget = null;
	public Vector3 pointOfInterest;
	public float detectRange = 10f,attackRange = 2f;
	public float patrolSpeed, attackSpeed;
	//public LayerMask detect;
	public GameObject path;

	public List<GameObject> myWps = new List<GameObject>();

	public float waitTime, currentTime;

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

	public State currentState;
	//Nav Shizzle
	NavMeshAgent nav;
	public int index;
	//for rotation
	private Quaternion _lookRotation;
	private Vector3 _direction;
	//Animator Stuff
	Vector3 currentPosition;
	Vector3 previousPosition;
	public float currentSpeed;
	public Animator anim;
	//Grounded
	bool grounded = false;

//	void Start()
//	{
		//InvokeRepeating ("StateMachine", 0.01f, 0.1f);
//	}

	void AnimatorInput()
	{
		currentPosition = transform.position - previousPosition;
		currentSpeed = currentPosition.magnitude / Time.deltaTime; // move float
		previousPosition = transform.position;
		anim.SetFloat ("Speed", currentSpeed);
	}

	// make custom update?
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
	void Init () {

		nav = GetComponent<NavMeshAgent> ();
		nav.updateRotation = true; //will need to 

		//Better, find whole wp routes and populate
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
			currentState = State.Idle;
		}
	}
	//Can call this eternally to assign new Paths
	public void CreateWayPointRoute(GameObject myPath)
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
		nav.Resume ();

		currentTime -= Time.deltaTime;
		//Debug.Log ("Waiting");

		if (currentTime < 0 & path != null) 
		{
			//Loop array/List
			if (index == myWps.Count -1) 
			{
				index = 0;
			}
			else//Increment the index
			{
				index++;
			}

			currentTime = waitTime;
			nav.SetDestination (myWps [index].transform.position);
			currentState = State.Patrol;
		}

		if(myTarget != null)
		{
			SetTarget(myTarget);
		}
	}

	//for last sightings and heard noises
	void Investigate()
	{
		nav.acceleration = 50f;

		nav.Resume ();
		currentTime -= Time.deltaTime;

		nav.SetDestination (pointOfInterest);
		//Debug.Log (Vector3.Distance (transform.position, pointOfInterest) + "POI");
		if (nav.remainingDistance < nav.stoppingDistance)
		{
			if(currentTime <= 0)
			{
				Debug.Log("Finished Snooping !" + currentTime);
				currentState = State.Idle;
			}
		}
	}

	//Array managing Patrol behaviour.
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
	}//End of WP
	
	public void SetTarget(GameObject tar)
	{
		myTarget = tar;

		if(myTarget != null)
		{
			targeting = true;

			nav.speed = attackSpeed;
			nav.acceleration = 400f;
			currentState = State.Chase;
		}
	}

	void Chase()
	{
		nav.Resume ();
	

		if(myTarget)
		{
			//check to see if target is in line of sight here
			nav.SetDestination(myTarget.transform.position);
		}
		else // senses determines wether we have a target or not!
		{
			currentState = State.Patrol;
			Debug.Log ("Is This ever used?");
		}

		if (nav.remainingDistance < nav.stoppingDistance && nav.remainingDistance != 0)
		{
			currentState = State.Attack;
			Debug.Log (nav.remainingDistance);
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
//			//currentState = State.Idle;
//			this.gameObject.SendMessage("ClearTarget",SendMessageOptions.RequireReceiver);
//		}

		nav.Stop ();
		float dist = CheckDistance (this.gameObject, myTarget);
		//Debug.Log (dist);

		if(dist > attackRange) //AttackRange check
		{
			currentState = State.Chase;
		}
		else // we have range!
		{
			//line of Sight check from Senses???
			//Attack Coroutine
			RotateTowordsTarget();
		}
	}

	void RotateTowordsTarget() //switch to turret method?
	{
		//nav.updateRotation = false;
		Vector3 direction = (myTarget.transform.position - transform.position).normalized;
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		Quaternion aimRotation = new Quaternion (transform.rotation.x, lookRotation.y, transform.rotation.z, lookRotation.w);
		transform.rotation = Quaternion.Slerp(transform.rotation, aimRotation, Time.deltaTime * 10f);
	}
	
}
