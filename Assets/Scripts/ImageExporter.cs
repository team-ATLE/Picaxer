using UnityEngine;
using System.IO;

public class ImageExporter : MonoBehaviour
{
    public void ExportToPNG(Color[] pixelColors, int width, int height, string filePath)
    {
        // 디렉토리가 존재하는지 확인하고, 없으면 생성
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 새로운 텍스처를 생성하고 픽셀 색상을 적용
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixelColors);
        texture.Apply();

        // PNG 데이터를 얻고 파일로 저장
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);
    }
}
