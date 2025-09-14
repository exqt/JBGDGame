using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Scriptable Objects/CharacterInfo")]
public class CharacterInfo : ScriptableObject {
    public int charId;
    public Texture2D charImage;
    public LocalizedString charName;
}
