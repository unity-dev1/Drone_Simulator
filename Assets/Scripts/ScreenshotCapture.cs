using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class ScreenshotCapture : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public Camera screenshotCamera;
    public string fileName = "Screenshot";
    public int imageWidth = 1024;
    public int imageHeight = 1024;

    [Header("UI Settings")]
    public GameObject screenshotCanvas;
    public RawImage previewImage;
    public float previewDuration = 2f;
    public Button screenshotButton;

    private void Start()
    {
        if (screenshotCanvas != null)
        {
            screenshotCanvas.SetActive(false);
        }

        if (screenshotButton != null)
        {
            screenshotButton.onClick.AddListener(CaptureScreenshot);
        }
        else
        {
            Debug.LogWarning("Screenshot button not assigned!");
        }
    }

    public void CaptureScreenshot()
    {
        if (screenshotCamera == null || mainCamera == null) 
        {
            Debug.LogWarning("Cameras not assigned for screenshot capture!");
            return;
        }

        bool mainCamEnabled = mainCamera.enabled;
        bool screenshotCamEnabled = screenshotCamera.enabled;

        try
        {
            mainCamera.enabled = false;
            screenshotCamera.enabled = true;

            RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
            screenshotCamera.targetTexture = rt;

            screenshotCamera.Render();

            Texture2D tex = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            tex.Apply();

            if (previewImage != null && screenshotCanvas != null)
            {
                previewImage.texture = tex;
                ShowPreview();
            }

            byte[] bytes = tex.EncodeToPNG();
            string path = Path.Combine(Application.persistentDataPath, fileName + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
            File.WriteAllBytes(path, bytes);
            Debug.Log($"Screenshot saved to: {path}");

            screenshotCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
        }
        finally
        {
            mainCamera.enabled = mainCamEnabled;
            screenshotCamera.enabled = screenshotCamEnabled;
        }
    }

    private void ShowPreview()
    {
        screenshotCanvas.SetActive(true);
      //  Invoke("HidePreview", previewDuration);
    }

    private void HidePreview()
    {
        screenshotCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        if (screenshotButton != null)
        {
            screenshotButton.onClick.RemoveListener(CaptureScreenshot);
        }
    }
} 