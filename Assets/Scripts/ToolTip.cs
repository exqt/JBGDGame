using UnityEngine;

public class ToolTip : MonoBehaviour
{
    private string text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetText(string text)
    {
        this.text = text;
    }

    public string GetText()
    {
        return text;
    }
}
