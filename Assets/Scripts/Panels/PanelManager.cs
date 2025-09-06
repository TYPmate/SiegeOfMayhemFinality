using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages UI panel display and navigation using singleton pattern
/// </summary>
public class PanelManager : PanelSingleton<PanelManager>
{
    public List<PanelModel> panelModels;

    private List<PanelInstanceModel> panelInstances = new List<PanelInstanceModel>();

    /// <summary>
    /// Shows a panel by its ID and adds it to the panel instances list
    /// </summary>
    /// <param name="panelId">The ID of the panel to show</param>
    public void ShowPanel(string panelId)
    {
        PanelModel panelModel = panelModels.FirstOrDefault(panelModels => panelModels.panelId == panelId);

        if (panelModel != null)
        {
            var newInstancePanel = Instantiate(panelModel.panelPrefab, transform);

            newInstancePanel.transform.localPosition = Vector3.zero;

            panelInstances.Add(new PanelInstanceModel
            {
                panelId = panelId,
                panelInstance = newInstancePanel
            });
        }
        else
        {
            Debug.LogWarning($"Trying to use panelId = {panelId}, but this is not found in panelModels");
        }
    }

    /// <summary>
    /// Hides the most recently shown panel
    /// </summary>
    public void HidePanel()
    {
        if (AnyPanelsShowing())
        {
            var lastPanel = panelInstances[panelInstances.Count - 1];

            panelInstances.Remove(lastPanel);
            Destroy(lastPanel.panelInstance);
        }
    }

    /// <summary>
    /// Checks if any panels are currently showing
    /// </summary>
    /// <returns>True if any panels are active, false otherwise</returns>
    public bool AnyPanelsShowing()
    {
        return GetPanelListAmount() > 0;
    }

    /// <summary>
    /// Gets the number of panels currently in the instances list
    /// </summary>
    /// <returns>The count of panel instances</returns>
    public int GetPanelListAmount()
    {
        return panelInstances.Count;
    }
}