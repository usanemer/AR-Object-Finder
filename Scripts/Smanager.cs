using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// 씬(Scene) 전환 트리거
/// </summary>

public class Smanager : MonoBehaviour
{
    public void GotoMain()//Main씬으로 이동
    {        
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    public void GotoScene(string sceneName)//매개변수로 받은 씬으로 이동
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
