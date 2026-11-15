using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinBlurCapture : MonoBehaviour
{
    [SerializeField] private RawImage blurImage;
    [SerializeField] private Shader blurShader;

    private RenderTexture blurredImage;

    public void ShowWinPanel(GameObject panel)
    {
        StartCoroutine(CaptureAndShow(panel));
    }

    private IEnumerator CaptureAndShow(GameObject panel)
    {
        yield return CaptureBlur();
        panel.SetActive(true);
    }



    public IEnumerator CaptureBlur()
    {
        if (blurImage == null || blurShader == null || !blurShader.isSupported)
            yield break;

        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        if (screenshot == null)
            yield break;

        ReleaseBlur();

        int width = Mathf.Max(1, screenshot.width / 4);
        int height = Mathf.Max(1, screenshot.height / 4);

        RenderTexture source = RenderTexture.GetTemporary(width, height, 0);
        RenderTexture horizontal = RenderTexture.GetTemporary(width, height, 0);
        Material material = new Material(blurShader);

        blurredImage = new RenderTexture(width, height, 0);
        blurredImage.filterMode = FilterMode.Bilinear;

        source.filterMode = FilterMode.Bilinear;
        horizontal.filterMode = FilterMode.Bilinear;
        source.wrapMode = TextureWrapMode.Clamp;
        horizontal.wrapMode = TextureWrapMode.Clamp;

        Graphics.Blit(screenshot, source);

        material.SetVector("_BlurDirection", new Vector4(2f, 0f, 0f, 0f));
        Graphics.Blit(source, horizontal, material);

        material.SetVector("_BlurDirection", new Vector4(0f, 2f, 0f, 0f));
        Graphics.Blit(horizontal, blurredImage, material);

        blurImage.texture = blurredImage;
        blurImage.enabled = true;

        RenderTexture.ReleaseTemporary(source);
        RenderTexture.ReleaseTemporary(horizontal);
        Destroy(material);
        Destroy(screenshot);
    }

    private void ReleaseBlur()
    {
        if (blurredImage == null)
            return;

        if (blurImage != null)
            blurImage.texture = null;

        blurredImage.Release();
        Destroy(blurredImage);
        blurredImage = null;
    }

    private void OnDestroy()
    {
        ReleaseBlur();
    }
}