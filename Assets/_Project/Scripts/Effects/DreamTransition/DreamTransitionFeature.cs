using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Renderer feature that draws the DreamTransition effect over the game camera. The spin, blur
/// and glow run before post-processing, so they go through bloom; the white-out runs after it,
/// so the peak is pure white rather than vignetted and tonemapped. Costs nothing while
/// DreamTransition is off.
/// </summary>
public class DreamTransitionFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader _shader;

    private Material _material;
    private DreamTransitionPass _swirlPass;
    private DreamTransitionPass _whiteOutPass;

    public override void Create()
    {
        _swirlPass = new DreamTransitionPass(0, "Dream Transition") { renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing };
        _whiteOutPass = new DreamTransitionPass(1, "Dream White-Out") { renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!DreamTransition.IsActive || renderingData.cameraData.cameraType != CameraType.Game)
            return;

        if (_material == null)
        {
            if (_shader == null)
                return;
            _material = CoreUtils.CreateEngineMaterial(_shader);
        }

        _swirlPass.Setup(_material);
        renderer.EnqueuePass(_swirlPass);

        if (DreamTransition.White > 0.0001f)
        {
            _whiteOutPass.Setup(_material);
            renderer.EnqueuePass(_whiteOutPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_material);
        _material = null;
    }

    private class DreamTransitionPass : ScriptableRenderPass
    {
        private readonly int _shaderPass;
        private readonly string _name;
        private Material _material;

        public DreamTransitionPass(int shaderPass, string name)
        {
            _shaderPass = shaderPass;
            _name = name;
            requiresIntermediateTexture = true;
        }

        public void Setup(Material material) => _material = material;

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer)
                return;

            TextureHandle source = resources.activeColorTexture;
            var desc = renderGraph.GetTextureDesc(source);
            desc.name = "_DreamTransitionColor";
            desc.clearBuffer = false;
            TextureHandle destination = renderGraph.CreateTexture(desc);

            renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source, destination, _material, _shaderPass), _name);
            resources.cameraColor = destination;
        }
    }
}
