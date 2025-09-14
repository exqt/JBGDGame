using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

[UxmlElement]
partial class InGamePopupWindow : VisualElement {
    static VisualTreeAsset template;

    public override VisualElement contentContainer => _contentContainer;
    VisualElement _contentContainer;

    VisualElement root;
    Shadow shadow;

    public InGamePopupWindow()
    {
        if (InGamePopupWindow.template == null) {
            var url = "Assets/UI/Common/UIPopupWindow.uxml";
            var handle = Addressables.LoadAssetAsync<VisualTreeAsset>(url);
            InGamePopupWindow.template = handle.WaitForCompletion();
        }

        var template = InGamePopupWindow.template.CloneTree();
        root = template.Q<VisualElement>("Root");
        shadow = root.Q<Shadow>();

        var closeButton = template.Q<Button>("CloseButton");
        closeButton.RegisterCallback<ClickEvent>((ev) => {
            SetDisplay(false);
        });

        var content = template.Q<VisualElement>("Content");
        _contentContainer = content;

        hierarchy.Add(root);
    }

    public void SetDisplay(bool display) {
        if (display) {
            this.style.visibility = Visibility.Visible;
            root.RemoveFromClassList("Hidden");
            shadow.RemoveFromClassList("Hidden");
        }
        else {
            schedule.Execute(() => {
                this.style.visibility = Visibility.Hidden;
                root.MarkDirtyRepaint();
            }).StartingIn(400);
            root.AddToClassList("Hidden");
            shadow.AddToClassList("Hidden");
        }
        root.MarkDirtyRepaint();
    }
}
