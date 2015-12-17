using UnityEngine;
using System.Collections;

public class CursorLock : MonoBehaviour {

	public bool hideCursor;

	void Start () 
	{
		ControlCursor ();
	}
	
	void Update () 
	{
		CheckForInput ();
		ControlCursor ();
	}

	void ToggleCursor () 
	{
		Debug.Log ("ToggleCursor");
		hideCursor = !hideCursor;
	}

	void CheckForInput () 
	{
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			ToggleCursor ();
		}
	}

	void ControlCursor () 
	{
		if(hideCursor)
		{
			//Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else
		{
			//Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
