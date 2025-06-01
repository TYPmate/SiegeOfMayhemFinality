using UnityEngine;

public class ShowTutorialTrigger : MonoBehaviour
{
    public string panelId;
    private PanelManager panelManager;
    private bool hasTriggered = false;

    private void Start()
    {
        panelManager = PanelManager.Instance;
    }

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

    public void DoShowPanel()
    {
        panelManager.ShowPanel(panelId);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }
}
