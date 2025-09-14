using DG.Tweening;
using UnityEngine;

public class FirstPersonCameraMovement : MonoBehaviour
{
    public Transform target;
    public float Sensitivity {
        get { return sensitivity; }
        set { sensitivity = value; }
    }

    [Range(0.1f, 9f)][SerializeField] float sensitivity = 6f;
    [Tooltip("Limits vertical camera rotation. Prevents the flipping that happens when rotation goes above 90.")]
    [Range(0f, 90f)][SerializeField] float pitchLimit = 60f;

    const string xAxis = "Mouse X";
    const string yAxis = "Mouse Y";

    const float HEIGHT = 1.1f;
    const float DISTANCE = 0.5f;
    int playerId = 0;

    Tweener rotationTween;

    void Start() {
        SetToPlayerPosition(0);
    }

    void HandleCamera() {
        var deltaYaw = Input.GetAxis(xAxis) * sensitivity;
        var deltaPitch = Input.GetAxis(yAxis) * sensitivity;
        var newYaw = transform.rotation.eulerAngles.y + deltaYaw;
        var newPitch = transform.rotation.eulerAngles.x - deltaPitch;
        if (newPitch > 180) newPitch -= 360;
        newPitch = Mathf.Clamp(newPitch, -pitchLimit, pitchLimit);
        transform.rotation = Quaternion.Euler(newPitch, newYaw, 0);
    }

    public void ResetRotation() {
        rotationTween?.Kill();
        rotationTween = transform.DOLookAt(target.position, 0.5f);
    }

    public void SetToPlayerPosition(int playerId) {
        this.playerId = playerId;
        if (playerId == 0) {
            transform.position = new Vector3(0, HEIGHT, -DISTANCE);
        }
        else if (playerId == 1) {
            transform.position = new Vector3(-DISTANCE, HEIGHT, 0);
        }
        else if (playerId == 2) {
            transform.position = new Vector3(0, HEIGHT, DISTANCE);
        }
        else if (playerId == 3) {
            transform.position = new Vector3(DISTANCE, HEIGHT, 0);
        }
        ResetRotation();
    }

    bool isPreviousFrameRightClick = false;
    void Update() {
        bool rightClick = Input.GetMouseButton(1);
        if (rightClick) {
            bool firstFrame = !isPreviousFrameRightClick;
            if (firstFrame) {
                rotationTween?.Kill();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                // ResetRotation();
            }
            else HandleCamera();

            isPreviousFrameRightClick = true;
        }
        else {
            if (isPreviousFrameRightClick) {
                // TODO: lerp
                ResetRotation();
            }
            isPreviousFrameRightClick = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
