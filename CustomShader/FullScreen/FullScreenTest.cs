using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.Playables;

//[ExecuteInEditMode]
public class FullScreenTest : MonoBehaviour
{
    public Material rendererFeature;
    //public int index, index1;
    public FullScreenPassRendererFeature fullScreenPassRendererFeature;
    public FullScreenPassRendererFeature.InjectionPoint injectionPoint =
        FullScreenPassRendererFeature.InjectionPoint.AfterRenderingPostProcessing;
    public Button renderFeature, actionTimeline;
    public bool onFeature;
    ScriptableRendererData scriptableRendererData;
    public RenderObjects renderObject;
    bool acting;
    public GameObject[] model;
    public PlayableDirector playable;
    public Toggle toggle;
    public int index;
    public Material instMat;

    void Start()
    {
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline);
        var propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        scriptableRendererData = ((ScriptableRendererData[])propertyInfo.GetValue(pipeline))[0];
        Debug.LogWarning(scriptableRendererData.rendererFeatures.Count);

        fullScreenPassRendererFeature = (FullScreenPassRendererFeature)scriptableRendererData.rendererFeatures[1];
        //renderObject = (RenderObjects)scriptableRendererData.rendererFeatures[12];

        SetButton();
    }

    void SwitchRenderFeature(Material _material)
    {
        fullScreenPassRendererFeature.SetActive(_material != null);
        if (_material == null)
        {
            fullScreenPassRendererFeature.passMaterial = null;
            Destroy(instMat);
            return;
        }
        instMat = Instantiate(_material);
        fullScreenPassRendererFeature.passMaterial = instMat;
        fullScreenPassRendererFeature.injectionPoint = injectionPoint;

#if UNITY_EDITOR
        scriptableRendererData.SetDirty();
#endif
    }

    private void Update()
    {

    }

    void SwitchRenderObject()
    {
        renderObject.settings.depthCompareFunction = CompareFunction.Greater;
    }

    void SetButton()
    {
        renderFeature.onClick.AddListener(ButtonA);
        //actionTimeline.onClick.AddListener(ActionTimeline);
        //toggle.onValueChanged.AddListener(ToggleChange);
    }

    void ButtonA()
    {
        onFeature = !onFeature;
        Material setMat = onFeature == true ? rendererFeature : null;
        SwitchRenderFeature(setMat);
    }

    void ActionTimeline()
    {
        acting = !acting;
        playable.gameObject.SetActive(acting);
        if (acting == true)
        {
            playable.Play();
        }
        else
        {
            playable.Stop();
        }
        for (int i = 0; i < model.Length; i++)
        {
            model[i].SetActive(!acting);
        }
    }

    void ToggleChange(bool _isOn)
    {
        onFeature = _isOn;
        fullScreenPassRendererFeature.SetActive(onFeature);
    }
}
