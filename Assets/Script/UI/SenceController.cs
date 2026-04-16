using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SenceController : MonoBehaviour
{
    public static SenceController Instance;

    public Image fadeImage;
    public float fadeSpeed = 2f;
    public float delayAfterFade = 0.2f; // delay nhỏ cho mượt

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(Open()); // mở scene đầu
    }

    // ================= CHANGE SCENE =================
    public IEnumerator ChangeSence(string sceneName)
    {
        yield return StartCoroutine(Close()); // fade out

        yield return new WaitForSeconds(delayAfterFade);

        SceneManager.LoadScene(sceneName);

        yield return new WaitForSeconds(0.1f); // đợi scene load

        yield return StartCoroutine(Open()); // fade in
    }

    // ================= CLOSE =================
    public IEnumerator Close()
    {
        fadeImage.gameObject.SetActive(true);
        yield return StartCoroutine(Fade(1));
    }

    // ================= OPEN =================
    public IEnumerator Open()
    {
        yield return StartCoroutine(Fade(0));
        fadeImage.gameObject.SetActive(false);
    }

    // ================= FADE =================
    IEnumerator Fade(float targetAlpha)
    {
        Color color = fadeImage.color;

        while (!Mathf.Approximately(color.a, targetAlpha))
        {
            color.a = Mathf.MoveTowards(color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            fadeImage.color = color;
            yield return null;
        }
    }
}