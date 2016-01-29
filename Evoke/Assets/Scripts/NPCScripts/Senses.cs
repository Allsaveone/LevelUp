/// <summary>
/// Senses
/// Basic senses for NpcAi - personal use
/// David ONeill
/// 10/12/15
/// </summary>
using UnityEngine;
using System.Collections;

[RequireComponent (typeof (SphereCollider))]
[RequireComponent (typeof (NavMeshAgent))]
[RequireComponent (typeof (NpcAi))]

public class Senses : MonoBehaviour {

	public enum targetType
	{
		Player,
		Bot
	};
	public targetType typeOfTarget;
	public LayerMask detectionMask;
	public GameObject senseTarget;

	public float fieldOfView = 160.0f;
	public bool targetInSight;
	public bool grounded;
	private SphereCollider col;
	float radius;
	NavMeshAgent nav;
	Animator anim; //need these later
	NpcAi ai;

	void Awake()
	{
		col = GetComponent<SphereCollider> ();
		col.radius = GetComponent<NpcAi> ().detectRange;
		col.isTrigger = true;

		nav = GetComponent<NavMeshAgent> ();
		anim = GetComponentInChildren<Animator> ();
		ai = GetComponent<NpcAi> ();
		senseTarget = null;

		InvokeRepeating ("GroundedCheck", 0.1f, 0.1f);
	}
	//Make a custom Update -Invoke
	void Update()
	{

	}

	void GroundedCheck()
	{
		Ray ray = new Ray (transform.position, Vector3.down);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, 1.25f))
		{
			grounded = true;
			Debug.DrawLine (transform.position, hit.point);
			if(nav.enabled == false)
			{
//				nav.enabled = true;
//				ai.enabled = true;
				anim.SetBool ("Grounded", true);
				StartCoroutine ("LandRecovery");
			}
		} 
		else 
		{
			grounded = false;

			if(nav.enabled == true)
			{
				nav.enabled = false;
				ai.enabled = false;
				anim.SetBool ("Grounded", false);

			}
		}	
	}

	IEnumerator LandRecovery()
	{
		yield return new WaitForSeconds (0.5f);
		nav.enabled = true;
		ai.enabled = true;
	}

	void ClearTarget() // Call this to go back to normal behaviour.
	{
		senseTarget = null;
		targetInSight = false;
		senseTarget = null;
		ai.targeting = false;
		ai.myTarget = null;
	}

	//Entering Detection Sphere - switch to Physics.overlapshpere?
	void OnTriggerStay(Collider other)
	{
		// Timer for additional optisation?
		if(other.gameObject.tag == typeOfTarget.ToString() && ai.currentState != NpcAi.State.Init)
		{
			if(ai.currentState == NpcAi.State.Idle ||ai.currentState == NpcAi.State.Patrol || ai.currentState == NpcAi.State.Chase || ai.currentState == NpcAi.State.Investigate || ai.currentState == NpcAi.State.Attack)
			{
				targetInSight = false;

				Vector3 direction = other.transform.position - transform.position;
				float angle = Vector3.Angle(direction,transform.forward);
				
			//Sight
				if(angle < fieldOfView * 0.5f)
				{
					RaycastHit hit;

					if(Physics.Raycast(transform.position + transform.up,direction.normalized, out hit,col.radius,detectionMask))
					{
						if(hit.collider.gameObject.tag == typeOfTarget.ToString())
						{					
							targetInSight = true;
							Debug.DrawLine (transform.position, hit.transform.position, Color.red, 0.1f, true);

							if(ai.myTarget == null)
							{
								senseTarget = hit.transform.gameObject;
								ai.SetTarget(senseTarget);
								//Debug.Log ("Target Aquired!");
							} 
						}
						else
						{
							if(senseTarget != null && targetInSight == false)
							{
								//Line of sight has been broken! 
								//ClearTarget();
								//ai.GoInvestigate(other.transform.position);
							}
						}
					}
				}
				//Hearing
//				if(ai.currentState != NpcAi.State.Chase && ai.currentState != NpcAi.State.Attack)//they are effectivly deaf when engaging an enemy
//				{
//					if(CalculatePathLength(other.transform.position) <= col.radius /2) //back here later
//					{
//						//ai.GoInvestigate(other.transform.position);
//						//Debug.Log ("Heard the Player!!"); //Carefull here, requires externa critera for solid logic gate
//					}
//				}
			}//State Logic gate end
		}
	}

	//Leaving Detection Sphere
	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == typeOfTarget.ToString())
		{
			//Debug.Log ("Lost Target");
			if(senseTarget !=  null)
			{
				ai.GoInvestigate(other.transform.position);
			}

			targetInSight = false;
			senseTarget = null;
			ai.targeting = false;
			ai.myTarget = null;
		}

	}
	
	float CalculatePathLength(Vector3 targetPos)
	{
		NavMeshPath path = new NavMeshPath ();

		if(nav.enabled)
		{
			nav.CalculatePath(targetPos, path);
		}

		Vector3 [] waypointsToTarget = new Vector3[path.corners.Length + 2]; //2 for orgin + target.

		waypointsToTarget [0] = transform.position;
		waypointsToTarget [waypointsToTarget.Length - 1] = targetPos;

		for(int i= 0; i<path.corners.Length; i++)
		{
			waypointsToTarget[i+1] = path.corners[i];
		}

		float pathLength = 0;

		for(int i = 0; i < waypointsToTarget.Length -1; i++)
		{
			pathLength += Vector3.Distance(waypointsToTarget[i],waypointsToTarget[i+1]);
		}

		return pathLength;
	}

}
