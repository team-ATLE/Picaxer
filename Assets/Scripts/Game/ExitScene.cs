using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScene : MonoBehaviour
{
    // Start is called before the first frame update
    public void ChangeScene(string sceneName)
    {
        GameObject obj = GameObject.Find("Game Manager");
        Debug.Log(obj);
        SceneManager.LoadScene(sceneName);
        Destroy(obj);
    }

    // Update is called once per frame
    public void ExitSc() {
        Application.Quit();
    }
}
