/// <summary>
/// Way point gizmo.
/// Gizmo!!!!!!
/// </summary>
using UnityEngine;
using System.Collections;

public class EditorGizmo : MonoBehaviour {

	public enum gizmoType {WayPoint,Spawner,PlayerSpawn};
	public gizmoType gizmo;

    void OnDrawGizmos() 
	{
		switch (gizmo)
		{
		case gizmoType.WayPoint:
			Gizmos.DrawIcon(transform.position, "wayPoint.png", true);
			break;
		default:
			break;
		}
//        Gizmos.DrawIcon(transform.position, "wayPoint.png", true);
//		Debug.Log ("Gizmos");
    }
}
