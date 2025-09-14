using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using System.Collections;

public class UISettings : MonoBehaviour {
    public GameSettings gameSettings;

    VisualElement root;
    SliderInt volumeSlider;
    VisualElement localizationButtons;
    InGamePopupWindow popupWindow;

    void Awake() {
        InitializeElements();
    }

    public void InitializeElements() {
        var uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;

        popupWindow = root.Q<InGamePopupWindow>();
        var content = popupWindow.Q<VisualElement>("Content");

        volumeSlider = content.Q<SliderInt>("VolumeSlider");
        volumeSlider.RegisterCallback<ChangeEvent<int>>((ev) => {
            gameSettings.ApplySettings();
        });

        AudioManager audioManager = AudioManager.Instance;
        content.Query<Button>().ForEach((button) => {
            button.clicked += () => {
                Debug.Log("Button clicked");
                audioManager.PlaySound("ui_click");
            };
        });

        SetDisplay(false);
    }

    IEnumerator Start() {
        yield return LocalizationSettings.InitializationOperation;
        localizationButtons = root.Q<VisualElement>("LocalizationButtons");
        localizationButtons.Query<VisualElement>().ForEach((button) => {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(button.name);
            button.RegisterCallback<ClickEvent>((ev) => {
                LocalizationSettings.SelectedLocale = locale;
            });
        });
    }

    void OnApplyButtonClicked() {
        gameSettings.ApplySettings();
        Debug.Log("Apply button clicked");
    }

    public void SetDisplay(bool display) {
        popupWindow.SetDisplay(display);
    }

    void Update() {

    }
}
