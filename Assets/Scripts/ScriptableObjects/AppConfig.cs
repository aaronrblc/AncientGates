using UnityEngine;

[CreateAssetMenu(fileName = "AppConfig", menuName = "SO/AppConfig")]
public class AppConfig : ScriptableObject
{
    public LanguageEnum Language = LanguageEnum.English;
    public string MainFont;
    public float MouseSensitivity = 2f;
    public bool InvertY = false;
}
