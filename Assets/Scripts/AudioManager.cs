using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

public class AudioManager : MonoBehaviour {
    public AssetLabelReference audioClipsReference;

    static AudioManager _instance;

    AudioSource audioSource;
    AudioClip[] clips;
    Dictionary<string, AudioClip> clipsDict = new Dictionary<string, AudioClip>();
    UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<AudioClip>> handle;

    public static AudioManager Instance {
        get {
            return _instance;
        }
    }

    void Start() {
        if (_instance == null) {
            _instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();

        // Load all audio clips in Assets/Audio
        handle = Addressables.LoadAssetsAsync<AudioClip>(audioClipsReference, null);
        handle.Completed += (handle) => {
            if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) {
                Debug.LogError("Failed to load audio clips");
                return;
            }

            foreach (var clip in handle.Result) {
                clipsDict[clip.name] = clip;
            }
        };
    }

    public void PlaySound(string clipName) {
        if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded) {
            Debug.LogError("Audio clips not loaded yet!");
            return;
        }

        var clip = clipsDict[clipName];
        if (clip == null) {
            Debug.LogError("Clip not found: " + clipName);
            return;
        }

        audioSource.PlayOneShot(clip);
    }
}
