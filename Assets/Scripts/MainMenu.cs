using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles main menu functionality including scene loading and game settings
/// </summary>
public class MainMenu : MonoBehaviour
{
    public Slider sensitivitySlider;

    /// <summary>
    /// Initialization method called when the script instance is being loaded
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// Updates mouse sensitivity setting based on slider value
    /// </summary>
    /// <param name="sensModifier">The sensitivity modifier value from the slider</param>
    public void SensitivitySlider(float sensModifier)
    {
        PersistentManager.sensModifier = sensModifier;
    }

    /// <summary>
    /// Loads the main game scene
    /// </summary>
    public void LoadScene()
    {
        SceneManager.LoadScene("Default3");
    }

    /// <summary>
    /// Quits the game application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}