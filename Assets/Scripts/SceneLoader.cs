using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
	/// <summary>
	/// Loads New Scenes
	/// </summary>
	/// <param name="sceneName">New Scene Name</param>
	public void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}