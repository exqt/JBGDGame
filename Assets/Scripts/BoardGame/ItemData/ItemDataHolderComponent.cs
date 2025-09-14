using UnityEngine;

public class DataHolder : MonoBehaviour
{
    public ItemBase data;

    public static void Attach<T>(GameObject gameObject, T data) where T : ItemBase
    {
        var dataHolder = gameObject.AddComponent<DataHolder>();
        dataHolder.data = data;
    }

    public static T GetData<T>(GameObject gameObject) where T : ItemBase
    {
        var dataHolder = gameObject.GetComponent<DataHolder>();
        return (T)dataHolder.data;
    }

    [ContextMenu("DebugLog")]
    public void DebugLog() {
        Debug.Log(data);
    }
}
