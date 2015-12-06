/// <summary>
/// Game master.
/// HighLevel Project Manager Class
/// ...... Not sure what it will manage yet...
/// </summary>
using UnityEngine;
using System.Collections;

public class GameMaster : MonoBehaviour {

	public bool dontDestroy;

	void Start () 
	{
		if(dontDestroy)
		{
			DontDestroyOnLoad(this.gameObject);
		}
	}
	
//	void Update () {
//	
//	}
}
