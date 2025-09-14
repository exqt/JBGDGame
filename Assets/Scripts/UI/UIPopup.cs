using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UIElements;

public class Popup : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement root;
    private VisualElement closeButton;
    private Button submitButton;

    private TextField playerNameInput;
    private TextField serverIPInput;
    private TextField serverPortInput;

    public enum Mode {
        Host,
        Join,
    }

    Mode mode;

    void Start() {
        uiDoc = GetComponent<UIDocument>();

        root = uiDoc.rootVisualElement;
        closeButton = root.Q<VisualElement>("CloseButton");
        closeButton.RegisterCallback<ClickEvent>(ev => SetVisible(false));

        submitButton = root.Q<Button>("SubmitButton");
        submitButton.clicked += OnSubmitButtonClicked;

        playerNameInput = root.Q<TextField>("PlayerNameInput");
        serverIPInput = root.Q<TextField>("ServerIPInput");
        serverPortInput = root.Q<TextField>("ServerPortInput");

        SetVisible(false);
    }

    public void SetVisible(bool visible) {
        root.visible = visible;
    }

    public void SetMode(Mode _mode) {
        mode = _mode;
        if (mode == Mode.Host) {
            serverIPInput.SetEnabled(false);
            serverPortInput.SetEnabled(false);
        } else {
            serverIPInput.SetEnabled(true);
            serverPortInput.SetEnabled(true);
        }
    }

    public void OnSubmitButtonClicked() {
        var playerName = playerNameInput.value;
        var serverIP = serverIPInput.value;
        var serverPort = serverPortInput.value;

        var manager = NetworkManager.Singleton;
        var transport = manager.GetComponent<UnityTransport>();


        if (mode == Mode.Host) {
            if (manager.StartHost()) {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            }
        } else {
            transport.SetConnectionData(serverIP, ushort.Parse(serverPort));
            if (manager.StartClient()) {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
            }
        }

    }


    void Update() {

    }
}
