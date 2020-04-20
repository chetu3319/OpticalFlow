using UnityEngine;
using UnityEngine.Video;

[ExecuteAlways]
public class OpticalFlow : MonoBehaviour
{
    public VideoPlayer video;

    public ComputeShader cs;

    [Range(1.0f, 3.0f)]
    public float Scale = 1.0f;
    [Range(0.0f, 0.1f)]
    public float Lambda = 0.01f;
    [Range(0.00001f, 0.1f)]
    public float Threshold = 0.01f;

    public Material outputMat;
    private RenderTexture outputTex;

    private Texture currentFrame;
    private RenderTexture previousFrame, flowCalculationFrame;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (video.texture != null)
            Calculate(video.texture);


    }

    public void Calculate(Texture current)
    {
        currentFrame = current;

        if (previousFrame == null)
        {
            Debug.Log("Setup");

            Setup(current.width, current.height);

        }
        int flowCalcKernel = cs.FindKernel("flowCalculation");
        cs.SetTexture(flowCalcKernel, "currentTex", currentFrame);
        cs.SetTexture(flowCalcKernel, "previousTex", previousFrame);
        cs.SetTexture(flowCalcKernel, "flowFrame", flowCalculationFrame);
        cs.SetFloat("Scale", Scale);
        cs.SetFloat("Lambda", Lambda);
        cs.SetFloat("Threshold", Threshold);
        cs.Dispatch(flowCalcKernel, current.width / 8, current.height / 8, 1);

        RenderOutput();
    }


    void Setup(int width, int height)
    {
        previousFrame = CreateTexture(width, height, RenderTextureFormat.ARGBFloat);
        previousFrame.name = "prevFrame";
        flowCalculationFrame = CreateTexture(width, height, RenderTextureFormat.ARGBFloat);
        flowCalculationFrame.name = "FlowCalc";
        outputTex = CreateTexture(width, height, RenderTextureFormat.ARGBFloat);
        GPUResetKernel(width, height);

    }




    protected RenderTexture CreateTexture(int width, int height, RenderTextureFormat format)
    {
        RenderTexture texture = new RenderTexture(width, height, 1, format);
        texture.enableRandomWrite = true;
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.useMipMap = false;
        texture.Create();
        return texture;
    }

    protected void RenderOutput()
    {
        outputMat.SetTexture("_UnlitColorMap", flowCalculationFrame);
    }

    void GPUResetKernel(int width, int height)
    {
        Debug.Log("GPUReset");
        int k = cs.FindKernel("ResetKernel");
        cs.SetTexture(k, "previousTex", previousFrame);
        cs.SetTexture(k, "currentTex", currentFrame);
        cs.SetTexture(k, "flowFrame", flowCalculationFrame);
        cs.SetInt("width", width);
        cs.SetInt("height", height);
        cs.Dispatch(k, width / 8, height / 8, 1);
        outputMat.SetTexture("_UnlitColorMap", flowCalculationFrame);

    }

}
