/// <summary>
/// Level loader.
/// Just handles scene switching.
/// </summary>
using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour {

	//public string [] levels;
	public Fade fade;
	private bool loading = false;

	public void LoadLevel(string levelName)
	{
		StartCoroutine ("LoadingLevel", levelName);
	}

	IEnumerator LoadingLevel(string levelToLoad)
	{
		if(loading == false) 
		{
			loading = true;

			float fadeTime = fade.fadeSpeed;
			fade.doFade = true;
			fade.BeginFade (1); //fade out
			yield return new WaitForSeconds (fadeTime);//wait for fade
			Application.LoadLevel (levelToLoad);
			Debug.Log (levelToLoad + "is being loaded");
			loading = false;
		}
	}

	public void QuitToDesktop()
	{
		StartCoroutine ("Quit");
	}

	IEnumerator Quit()
	{
		float fadeTime = fade.fadeSpeed;
		fade.doFade = true;
		fade.BeginFade (1); //fade out
		yield return new WaitForSeconds (fadeTime);//wait for fade
		Application.Quit ();
	}
}
