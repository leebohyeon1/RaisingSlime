using System.Collections;
using UnityEngine;

public class TransparentObject : MonoBehaviour
{
    public bool isTransparent { get; private set; } = false;

    private MeshRenderer[] renderers;
    private WaitForSeconds delay = new WaitForSeconds(0.001f);
    private WaitForSeconds resetDelay = new WaitForSeconds(0.005f);
    private const float THRESHOLDE_ALPHA = 0.3f;
    private const float THRESHOLDE_MAX_TIMER = 0.5f;

    private bool isReseting = false;
    private float timer = 0f;
    private Coroutine timeCheckCoroutine;
    private Coroutine resetCoroutine;
    private Coroutine becomeTransparentCoroutine;

    private void Awake()
    {
        renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.material = new Material(renderer.material); // 재질 인스턴스화
        }
    }

    public void BecomeTransparent()
    {
        if (isTransparent)
        {
            timer = 0f;
            return;
        }

        if (resetCoroutine != null && isReseting)
        {
            isReseting = false;
            isTransparent = false;
            StopCoroutine(resetCoroutine);
        }

        SetMaterialTransparent();
        isTransparent = true;
        becomeTransparentCoroutine = StartCoroutine(BecomeTransparentCoroutine());
    }

    private void SetMaterialRenderingMode(Material material, float surfaceType, int renderQueue)
    {
        material.SetFloat("_Surface", surfaceType);
        material.SetFloat("_AlphaClip", 0); // 알파 클리핑 비활성화
        material.renderQueue = renderQueue;

        // URP에서 투명 모드 설정을 위해 EnableKeyword로 추가 세팅
        if (surfaceType == 1) // Transparent 모드
        {
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0); // ZWrite를 비활성화
        }
        else // Opaque 모드
        {
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Opaque");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1); // ZWrite 활성화
        }
    }

    private void SetMaterialTransparent()
    {
        foreach (var renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                SetMaterialRenderingMode(material, 1f, 3000);
            }
        }
    }

    private void SetMaterialOpaque()
    {
        foreach (var renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                SetMaterialRenderingMode(material, 0f, 2000);
            }
        }
    }

    public void ResetOriginalTransparent()
    {
        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        SetMaterialOpaque();
        resetCoroutine = StartCoroutine(ResetOriginalTransparentCoroutine());
 
    }

    private IEnumerator BecomeTransparentCoroutine()
    {
        while (true)
        {
            bool isComplete = true;

            foreach (var renderer in renderers)
            {
                Color color = renderer.material.color;
                if (color.a > THRESHOLDE_ALPHA)
                {
                    isComplete = false;
                    color.a = Mathf.Clamp(color.a - Time.deltaTime, 0.3f, 1f);
                    renderer.material.color = color;
                }
            }

            if (isComplete)
            {
                CheckTimer();
                break;
            }

            yield return null;
        }
    }

    private IEnumerator ResetOriginalTransparentCoroutine()
    {
        isTransparent = false;

        while (true)
        {
            bool isComplete = true;

            foreach (var renderer in renderers)
            {
                Color color = renderer.material.color;
                if (color.a < 1f)
                {
                    isComplete = false;
                    color.a = Mathf.Clamp(color.a + Time.deltaTime, 0f, 1f);
                    renderer.material.color = color;
                }
            }

            if (isComplete)
            {
                isReseting = false;
                break;
            }

            yield return null;
        }
    }

    public void CheckTimer()
    {
        if (timeCheckCoroutine != null)
        {
            StopCoroutine(timeCheckCoroutine);
        }

        timeCheckCoroutine = StartCoroutine(CheckTimerCoroutine());
    }

    private IEnumerator CheckTimerCoroutine() 
    {
        timer = 0f;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= THRESHOLDE_MAX_TIMER)
            {
                isReseting = true;
                ResetOriginalTransparent();
                break;
            }

            yield return null;
        }
    }
}
