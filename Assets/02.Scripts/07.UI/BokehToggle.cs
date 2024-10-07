using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BokehToggle : MonoBehaviour
{
    public Volume globalVolume; // �۷ι� ������ �Ҵ�
    private DepthOfField depthOfField;

    void Start()
    {
        // Volume �������Ͽ��� Depth of Field ������Ʈ�� ��������
        if (globalVolume.profile.TryGet(out depthOfField))
        {
            // Bokeh ȿ���� �⺻������ ��Ȱ��ȭ
            depthOfField.active = false;
        }
    }

    // Ư�� ��Ȳ���� Bokeh ȿ���� Ȱ��ȭ/��Ȱ��ȭ�ϴ� �Լ�
    public void ToggleBokeh(bool enable)
    {
        if (depthOfField != null)
        {
            depthOfField.active = enable;
        }
    }
}
