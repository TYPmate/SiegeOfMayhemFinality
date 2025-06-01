using UnityEngine;

public class PanelSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    public void Awake()
    {
        instance = (T)(object)this;
    }
}
