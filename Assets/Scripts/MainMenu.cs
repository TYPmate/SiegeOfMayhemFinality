using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Slider sensitivitySlider;
    void Start()
    {
        
    }

    public void SensitivitySlider(float sensModifier)
    {
        PersistentManager.sensModifier = sensModifier;
    }

    public void LoadScene()
    {
        SceneManager.LoadScene("Default3");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
