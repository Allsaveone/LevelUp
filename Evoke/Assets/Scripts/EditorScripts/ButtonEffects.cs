using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonEffects : MonoBehaviour {

	public AudioClip buttonSFX;

	private Button myButtons ;//{get {return GetComponent<Button>();}}
	private AudioSource source;
	//private Animator anim; 
	private GameObject eventSystem;

	void Start () 
	{
		myButtons = GetComponent<Button> ();
		//AssignListeners (myButtons);

		source = GetComponentInParent<AudioSource> ();
		source.playOnAwake = false;
		myButtons.onClick.AddListener(() => PlaySound());
		eventSystem = GameObject.Find ("EventSystem");
	}

//	void AssignListeners(Button [] buttons)
//	{
//		foreach(Button b in buttons)
//		{
//			Debug.Log ("Assigned" + buttons.Length.ToString() + b.name.ToString());
//			b.onClick.AddListener(() =>{ PlaySound(); });
//		}
//	}
	
	public void PlaySound () 
	{
		eventSystem.SetActive (false);
		source.clip = buttonSFX;
		source.PlayOneShot (buttonSFX);
	}
}
