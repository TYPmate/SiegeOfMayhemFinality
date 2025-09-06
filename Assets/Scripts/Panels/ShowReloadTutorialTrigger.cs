using UnityEngine;

/// <summary>
/// Handles tutorial panel display when player enters trigger zone
/// </summary>
public class ShowTutorialTrigger : MonoBehaviour
{
    public string panelId;
    private PanelManager panelManager;
    private bool hasTriggered = false;

    /// <summary>
    /// Initializes reference to the panel manager
    /// </summary>
    private void Start()
    {
        panelManager = PanelManager.Instance;
    }

    /// <summary>
    /// Handles trigger entry to detect player and display tutorial panel
    /// </summary>
    /// <param name="other">The collider entering the trigger zone</param>
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        PlayerMotor playerMotor = other.GetComponent<PlayerMotor>();
        if (other.CompareTag("Player"))
        {
            DoShowPanel();
            hasTriggered = true;
        }
    }

    /// <summary>
    /// Displays the tutorial panel and pauses game time
    /// </summary>
    public void DoShowPanel()
    {
        panelManager.ShowPanel(panelId);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}