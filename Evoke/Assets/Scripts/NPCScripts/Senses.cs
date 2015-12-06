/// <summary>
/// Senses
/// Basic senses for TSS prototype
/// David ONeill
/// 3/10/15
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
	public GameObject target;

	public float fieldOfView = 160.0f;
	public bool targetInSight;
	public Vector3 lastSighting;

	private Vector3 _resetSight = new Vector3(1000.0f,1000.0f,1000.0f);
//	private Vector3 _previousSighting = new Vector3(1000.0f,1000.0f,1000.0f);

	public SphereCollider col;
	float radius;
	NavMeshAgent nav;
	//Animator anim; //need these later
	NpcAi ai;

	void Awake()
	{
		col = GetComponent<SphereCollider> ();
		col.radius = GetComponent<NpcAi> ().detectRange;
		col.isTrigger = true;

		nav = GetComponent<NavMeshAgent> ();
		//anim = GetComponent<Animator> ();
		ai = GetComponent<NpcAi> ();
		target = null;

		lastSighting = _resetSight;
//		_previousSighting = _resetSight;

	}
	//Make a custom Update -Invoke
	void Update()
	{
//		if(lastSighting != _previousSighting) 
//		{
//			Debug.Log ("target has moved");
//		}
//		_previousSighting = _resetSight;
	}

	void ClearTarget() // Call this to go back to normal behaviour.
	{
		target = null;
		ai.myTarget = null;
		ai.currentState = NpcAi.State.Idle;
	}

	//Entering Detection Sphere
	void OnTriggerStay(Collider other)
	{
		if(other.gameObject.tag == "Player" && ai.currentState != NpcAi.State.Init)
		{
			//if(ai.currentState == NpcAi.State.Idle ||ai.currentState == NpcAi.State.Patrol || ai.currentState == NpcAi.State.Chase || ai.currentState == NpcAi.State.Investigate)
			//{
				targetInSight = false;
				Vector3 direction = other.transform.position - transform.position;
				float angle = Vector3.Angle(direction,transform.forward);
				
			//Sight
				if(angle < fieldOfView * 0.5f)
				{
					RaycastHit hit;

					if(Physics.Raycast(transform.position + transform.forward,direction.normalized, out hit,col.radius))
					{
						if(hit.collider.gameObject.tag == typeOfTarget.ToString())
						{
							targetInSight = true;
							Debug.DrawRay(transform.position + transform.up,direction.normalized,Color.red,1.0f); 
							lastSighting = hit.transform.position;
							//_previousSighting = hit.transform.position;
							
							if(ai.myTarget == null)
							{
//								target = hit.transform.gameObject;
//
//								ai.SetTarget(target); // needs work!!!!!!!!!
//								Debug.Log ("Target Aquired!");
							} 

						}
					}
				}
				//Hearing
				if(ai.currentState != NpcAi.State.Chase && ai.currentState != NpcAi.State.Attack)//they are effectivly deaf when engaging an enemy
				{
					if(CalculatePathLength(other.transform.position) <= col.radius /2) //back here later
					{
						ai.pointOfInterest = other.transform.position;
						ai.currentTime = 5f;
						ai.currentState = NpcAi.State.Investigate;
						Debug.Log ("Heard the Player!!");
						//Debug.DrawRay(transform.position + transform.up,direction.normalized,Color.red,1.0f); 
						//lastSighting = other.transform.position;
					}
				}
				

			//}
		}
	}
	//Leaving Detection Sphere
	void OnTriggerExit(Collider other)
	{
		if(other.gameObject.tag == "Player")
		{
			Debug.Log ("Lost Player");
			if(target !=  null)
			{
				lastSighting = other.transform.position;
				ai.pointOfInterest = lastSighting;
				ai.currentTime = 5f;
				ai.currentState = NpcAi.State.Investigate;
//				_previousSighting = lastSighting;
				lastSighting = _resetSight;
			}

			targetInSight = false;
			target = null;
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
