using UnityEngine;
using UnityEngine.UIElements;

public class UIPopupWindow : MonoBehaviour {
    UIDocument uiDoc;
    VisualElement root, window, content;
    Button closeButton;

    void Awake() {
        uiDoc = GetComponent<UIDocument>();

        root = uiDoc.rootVisualElement;
        window = root.Q<VisualElement>("Window");
        content = window.Q<VisualElement>("Content");

        AudioManager audioManager = AudioManager.Instance;

        closeButton = root.Q<Button>("CloseButton");
        closeButton.clicked += () => SetDisplay(false);

        root.Query<Button>().ForEach((button) => {
            button.clicked += () => audioManager.PlaySound("ui_click");
        });

        SetDisplay(false);
    }

    public void SetContent(VisualElement newContent) {
        content.Clear();
        content.Add(newContent);
    }

    public void SetDisplay(bool display) {
        root.visible = display;
        if (display) {
            window.AddToClassList("Open");
        } else {
            window.RemoveFromClassList("Open");
        }
    }

    void Update() {

    }
}
