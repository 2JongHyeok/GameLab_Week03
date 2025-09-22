using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [Header("UI Image (검은색 풀스크린)")]
    public Image fadeImage;

    [Header("설정")]
    private float fadeDuration = 3f; // 페이드 시간

    private void Awake()
    {
        gameObject.SetActive(false);
        if (fadeImage != null)
        {
            // 시작할 때 완전히 투명하게
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
    }
    public IEnumerator FadeOut()
    {
        if (fadeImage == null) yield break;
        gameObject.SetActive(true);
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);

            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;

            yield return null;
        }
    }
    public IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);

            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;

            yield return null;
        }
        gameObject.SetActive(false);
    }
}
