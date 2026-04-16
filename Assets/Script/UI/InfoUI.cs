using TMPro;
using UnityEngine;

public class InfoUI : MonoBehaviour
{
    public TMP_Text Name;
    public void Start()
    {
        Name.text = PlayerPrefs.GetString("username", "Guest");
    }

}
