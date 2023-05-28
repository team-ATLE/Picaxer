using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroLogo : MonoBehaviour
{
    public float fadeInTime = 2.0f;
    public float waitTime = 2.0f;  // 로고가 페이드인 한 후 정지되는 시간
    public string nextScene;
    private Image logoImage;
    public Image iconImage;  // 추가한 아이콘 이미지

    void Start()
    {
        logoImage = GetComponent<Image>();
        logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, 0);
        iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, 0);  // 아이콘도 처음에는 안 보이게 합니다.
        StartCoroutine(FadeLogoAndIcon());
    }

    IEnumerator FadeLogoAndIcon()
    {
        // 로고 페이드인
        while (logoImage.color.a < 1.0f)
        {
            float newAlpha = logoImage.color.a + (Time.deltaTime / fadeInTime);
            logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, newAlpha);
            yield return null;
        }

        // 로고가 페이드인 한 후 정지되는 시간을 기다립니다.
        yield return new WaitForSeconds(waitTime);

        // 아이콘 페이드인
        while (iconImage.color.a < 1.0f)
        {
            float newAlpha = iconImage.color.a + (Time.deltaTime / fadeInTime);
            iconImage.color = new Color(iconImage.color.r, iconImage.color.g, iconImage.color.b, newAlpha);
            yield return null;
        }

        // 아이콘이 페이드인 한 후 정지되는 시간을 기다립니다.
        yield return new WaitForSeconds(waitTime);

        // 모든 페이드인 및 대기 시간이 끝나면 다음 씬으로 넘어갑니다.
        SceneManager.LoadScene(nextScene);
    }
}
