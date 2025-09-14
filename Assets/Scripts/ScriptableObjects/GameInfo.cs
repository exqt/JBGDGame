using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "GameInfo", menuName = "Scriptable Objects/GameInfo")]
public class GameInfo : ScriptableObject {
    public int gameID;
    public Texture2D gamePreviewImage;
    public LocalizedString gameName;
    public LocalizedString gameDescription;
    public LocalizedString gameGuide;
    public string cartoonUrl;
}
