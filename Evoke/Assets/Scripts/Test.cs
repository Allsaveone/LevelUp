using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	public NavMeshAgent nav;

	void Update()
	{
		float speed = Vector3.Project(nav.desiredVelocity,transform.forward).magnitude;
		Debug.Log (speed);
	}
}
