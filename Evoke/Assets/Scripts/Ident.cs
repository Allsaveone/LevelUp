/// <summary>
/// Ident.
/// Handles the Ident, and loading to the Menu scene
/// </summary>
using UnityEngine;
using System.Collections;

public class Ident : MonoBehaviour {

	public float identTime = 3f;

	private LevelLoader loader;

	void Start () 
	{
		loader = GameObject.Find ("GameManager").GetComponent<LevelLoader> ();
		Invoke ("GoToMenu", identTime);
	}
	
	void GoToMenu()
	{
		loader.LoadLevel ("Menu");
	}
}
