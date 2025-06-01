using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PanelManager : PanelSingleton<PanelManager>
{
    public List<PanelModel> panelModels;

    private List<PanelInstanceModel> panelInstances = new List<PanelInstanceModel>();

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

    public void HidePanel()
    {
        if (AnyPanelsShowing())
        {
            var lastPanel = panelInstances[panelInstances.Count - 1];

            panelInstances.Remove(lastPanel);
            Destroy(lastPanel.panelInstance);
        }
    }

    public bool AnyPanelsShowing()
    {
        return GetPanelListAmount() > 0;
    }

    public int GetPanelListAmount()
    {
        return panelInstances.Count;
    }
}
