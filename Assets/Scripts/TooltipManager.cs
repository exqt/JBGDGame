using TMPro;
using UnityEngine;

public class ToolTipManager : MonoBehaviour
{
    private ToolTip currentToolTip;
    private Transform messageBox;
    private TMP_Text text;
    private bool isEnable = false;

    void Start()
    {
        messageBox = transform.Find("MessageBox");
        text = messageBox.Find("Text").GetComponent<TMP_Text>();
        messageBox.gameObject.SetActive(false);
    }

    void SetEnableMessage(bool enable)
    {
        isEnable = enable;
        messageBox.gameObject.SetActive(enable);
    }

    void SetSize() {
        text.ForceMeshUpdate();
        var textSize = text.GetPreferredValues();
        var size = new Vector2(textSize.x + 6, textSize.y + 6);
        messageBox.GetComponent<RectTransform>().sizeDelta = size;
    }

    void Raycast() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        currentToolTip = null;
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.collider != null) {
                if (hit.collider.TryGetComponent(out currentToolTip) && currentToolTip.enabled) {
                    SetEnableMessage(true);
                    text.text = currentToolTip.GetText();
                    SetSize();
                }
                else {
                    SetEnableMessage(false);
                }
            }
            else {
                SetEnableMessage(false);
            }
        }
    }

    void Update() {
        Raycast();
        if (isEnable && currentToolTip != null) {
            messageBox.position = Input.mousePosition + new Vector3(0, -32, 0);
        }
    }
}
