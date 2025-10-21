using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public void PlayGame()
    {
        // ��������� ����� � ��������� "Game"
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
        
        Debug.Log("Quit Game!");
    }
}