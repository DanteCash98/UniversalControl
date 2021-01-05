using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public void TransitionScene(string sceneName){
        SceneManager.LoadScene(sceneName);
    }
}
