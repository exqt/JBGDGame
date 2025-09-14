using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject {
    public int volume;
    public bool fullscreen;

    public void ApplySettings() {
        Debug.Log($"Applying settings: Volume {volume}, Fullscreen {fullscreen}");
        PlayerPrefs.SetInt("Volume", volume);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
    }

    public void LoadSettings() {
        Debug.Log($"Loading settings: Volume {volume}, Fullscreen {fullscreen}");
        volume = PlayerPrefs.GetInt("Volume", 100);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }
}
