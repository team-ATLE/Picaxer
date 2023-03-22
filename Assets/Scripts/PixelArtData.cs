using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PixelArtData
{
    public int width;
    public int height;
    public List<Color> colors;
    [HideInInspector]
    public string colorString;

    // 생성자 (픽셀 아트의 가로, 세로 길이와 색상 리스트를 받아 초기화)
    public PixelArtData(int width, int height, List<Color> colors)
    {
        this.width = width;
        this.height = height;
        this.colors = colors;
        this.colorString = ConvertColorListToString(colors);
    }

    // 색상 문자열을 받는 생성자를 추가합니다.
    public PixelArtData(int width, int height, string colorString)
    {
        this.width = width;
        this.height = height;
        this.colorString = colorString;
        this.colors = ParseColorList(colorString);
    }

    // Json으로 변환하는 메서드
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    // Json 데이터를 불러와 PixelArtData 객체로 변환하는 메서드
    public static PixelArtData Load()
    {
        if (!PlayerPrefs.HasKey("PixelArt"))
        {
            Debug.LogError("No PixelArt data found.");
            return null;
        }

        if (!PlayerPrefs.HasKey("PixelArtWidth") || !PlayerPrefs.HasKey("PixelArtHeight"))
        {
            Debug.LogError("No PixelArt width and height data found.");
            return null;
        }

        int width = int.Parse(PlayerPrefs.GetString("PixelArtWidth"));
        int height = int.Parse(PlayerPrefs.GetString("PixelArtHeight"));
        string colorString = PlayerPrefs.GetString("PixelArt");

        PixelArtData data = new PixelArtData(width, height, colorString);
        return data;
    }

    // 색상 리스트를 문자열로 변환하는 메서드
    private string ConvertColorListToString(List<Color> colors)
    {
        return string.Join(";", colors.Select(c => ColorUtility.ToHtmlStringRGBA(c)));
    }

    // 문자열을 색상 리스트로 변환하는 메서드
    private List<Color> ParseColorList(string colorString)
    {
        List<Color> colorList = new List<Color>();
        string[] colorCodes = colorString.Split(';');

        for (int i = 0; i < colorCodes.Length; i++)
        {
            if (colorCodes[i].Length == 8)
            {
                Color color;
                if (ColorUtility.TryParseHtmlString("#" + colorCodes[i], out color))
                {
                    colorList.Add(color);
                }
                else
                {
                    Debug.LogError("Failed to parse color: " + colorCodes[i]);
                }
            }
            else if (colorCodes[i].Length > 0)
            {
                Debug.LogError("Invalid color code length: " + colorCodes[i].Length);
            }
        }

        return colorList;
    }

}
