using UnityEngine;

public class ClientPlayer : MonoBehaviour {
    public static ClientPlayer instance;

    public GameObject cameraObject;
    FirstPersonCameraMovement cameraComponent;
    NumberMahjong game;

    public int playerId = 0;

    // 2d
    public GameObject playerHandObject;

    void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    void Start() {
        cameraObject.TryGetComponent(out cameraComponent);

        var gameManager = NumberMahjongManager.instance;
        game = gameManager.game;

        for (int i = 0; i < 4; i++) {
        }

        playerHandObject.SetActive(true);
    }

    private GameObject Raycast() {
        Camera mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            GameObject hitObject = hit.collider.gameObject;
            return hitObject;
        }
        return null;
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            var o = Raycast();
            if (o != null) {
                if (o.TryGetComponent<ISelectableBoardItem>(out var comp)) comp.OnSelect();
            }
        }
    }
}
