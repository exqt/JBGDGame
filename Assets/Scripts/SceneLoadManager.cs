using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneLoadManager : MonoBehaviour {
    static SceneLoadManager instance;

    public static SceneLoadManager Instance {
        get {
            return instance;
        }
    }

    UIDocument uiDoc;

    void Awake() {
        instance = this;

        uiDoc = GetComponent<UIDocument>();
        uiDoc.enabled = false;

        // Don't destroy the LoadManager object
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator GoToSceneCoroutine(string name) {
        uiDoc.enabled = true;

        // Load the game scene
        var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Scenes/" + name);

        // Wait until the asynchronous scene fully loads
        while (!op.isDone)
        {
            yield return null;
        }

        uiDoc.enabled = false;
    }

    public void GoToScene(string name) {
        StartCoroutine(GoToSceneCoroutine(name));
    }
}
