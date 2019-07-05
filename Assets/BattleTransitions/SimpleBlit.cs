using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SimpleBlit : MonoBehaviour
{
    public Material TransitionMaterial;
    private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");

    private float cutoff = 0f;

    public ChangeScene sceneChanger;

    void Start()
    {
        TransitionMaterial.SetFloat(Cutoff,0f);
    }
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (TransitionMaterial != null)
            Graphics.Blit(src, dst, TransitionMaterial);
    }

    public IEnumerator StartTransition()
    {
        while (TransitionMaterial.GetFloat(Cutoff) < 1f)
        {
            cutoff += Time.deltaTime;
            TransitionMaterial.SetFloat(Cutoff,cutoff);
            yield return null;
        }

        sceneChanger.FadeToLevel("Battle");
    }
}
