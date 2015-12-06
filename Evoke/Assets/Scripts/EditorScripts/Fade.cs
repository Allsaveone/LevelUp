/// <summary>
/// Fade.
/// Handle scene fading. passes fadetime to loading.
/// </summary>
using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour {

	public Texture2D fadeTexture;
	public float fadeSpeed = 3f;

	private int drawDepth = -100;
	private int fadeDirection = -1; // -1 In, 1 Out
	private float alpha = 1.0f;
	[HideInInspector]
	public bool doFade = true;
	
	void OnGUI()
	{
		if(doFade)
		{
			FadeEffect ();
		}
	}

	void FadeEffect()
	{
		alpha += fadeDirection * fadeSpeed * Time.deltaTime;
		alpha = Mathf.Clamp01 (alpha);
		
		GUI.color = new Color (GUI.color.r, GUI.color.g, GUI.color.b, alpha);
		GUI.depth = drawDepth;
		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeTexture);
	}

	public float BeginFade(int direction)
	{
		fadeDirection = direction;
		doFade = true;
		StartCoroutine ("DisableFading");

		return (fadeSpeed); // for loading levels
	}

	void OnLevelWasLoaded()
	{
		Debug.Log (Application.loadedLevelName.ToString ());
		alpha = 1;
		BeginFade(-1); //fade in
	}

	IEnumerator DisableFading()
	{
		yield return new WaitForSeconds (fadeSpeed + 1f);
		doFade = false;
		//Debug.Log ("FadeDisabled");
	}
}
