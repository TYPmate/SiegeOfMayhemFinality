using UnityEngine;

public class NextButton : MonoBehaviour
{
    private PanelManager panelManager;

    private void Start()
    {
        panelManager = PanelManager.Instance;
    }

    public void DoHidePanel()
    {
        panelManager.HidePanel();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1.0f;
    }
}
