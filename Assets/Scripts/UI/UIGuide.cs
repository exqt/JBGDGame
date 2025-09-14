using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class UIGuide : MonoBehaviour {
    static GameInfo[] gameInfos;

    VisualElement root;
    InGamePopupWindow popupWindow;
    Label text;
    int gameId;

    void Awake() {
        InitializeElements();
    }

    void Start() {
        if (gameInfos == null) {
            gameInfos = Resources.LoadAll<GameInfo>("Data/GameInfo");
            gameInfos = gameInfos.OrderBy(x => x.gameID).ToArray();
        }
    }

    public void InitializeElements() {
        var uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;

        popupWindow = root.Q<InGamePopupWindow>();
        text = root.Q<Label>("Text");

        SetDisplay(false);
    }

    public void SetGameId(int gameId) {
        this.gameId = gameId;
        var guideText = gameInfos[gameId];
        text.text = guideText.gameGuide.GetLocalizedString();
    }

    public void SetDisplay(bool display) {
        popupWindow.SetDisplay(display);
    }
}
