using UnityEngine;
using UnityEngine.UI;

public class ColorButtonManager : MonoBehaviour
{
    public PixelArtEditor pixelArtEditor;
    public Button[] colorButtons;

    void Start()
    {
        for (int i = 0; i < colorButtons.Length; i++)
        {
            int index = i;
            colorButtons[i].onClick.AddListener(() => pixelArtEditor.SelectColor(index));
        }

    }
}
