using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BokehToggle : MonoBehaviour
{
    public Volume globalVolume; // 글로벌 볼륨을 할당
    private DepthOfField depthOfField;

    void Start()
    {
        // Volume 프로파일에서 Depth of Field 컴포넌트를 가져오기
        if (globalVolume.profile.TryGet(out depthOfField))
        {
            // Bokeh 효과를 기본적으로 비활성화
            depthOfField.active = false;
        }
    }

    // 특정 상황에서 Bokeh 효과를 활성화/비활성화하는 함수
    public void ToggleBokeh(bool enable)
    {
        if (depthOfField != null)
        {
            depthOfField.active = enable;
        }
    }
}
