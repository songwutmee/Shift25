using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Shift25.Visuals
{
    public class PSXRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class PSXSettings
        {
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
            public Material psxMaterial = null;
            [Range(64, 512)] public int targetResolution = 240;
        }

        public PSXSettings settings = new PSXSettings();

        class PSXPass : ScriptableRenderPass
        {
            private PSXSettings settings;
            private RTHandle _tempTexture;

            public PSXPass(PSXSettings settings)
            {
                this.settings = settings;
            }

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var desc = renderingData.cameraData.cameraTargetDescriptor;
                desc.depthBufferBits = 0; 
                RenderingUtils.ReAllocateIfNeeded(ref _tempTexture, desc, name: "_PSXTempTexture");
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // Safety check: Exit if material is not assigned or camera is invalid
                if (settings.psxMaterial == null || renderingData.cameraData.isPreviewCamera) return;

                CommandBuffer cmd = CommandBufferPool.Get("PSX_Pass");

                // Modern URP way to get the current camera target
                RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

                // Resolution and Aspect Ratio calculation
                float w = renderingData.cameraData.cameraTargetDescriptor.width;
                float h = renderingData.cameraData.cameraTargetDescriptor.height;
                float aspect = w / h;

                Vector4 resVector = new Vector4(settings.targetResolution * aspect, settings.targetResolution, 0, 0);
                settings.psxMaterial.SetVector("_Res", resVector);

                // [Blit Logic] Double blit: source -> temp (with material) -> source
                // This is the industry standard for full-screen post-effects in URP
                Blit(cmd, source, _tempTexture, settings.psxMaterial);
                Blit(cmd, _tempTexture, source);

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            public void Cleanup() => _tempTexture?.Release();
        }

        PSXPass _psxPass;

        public override void Create()
        {
            _psxPass = new PSXPass(settings);
            _psxPass.renderPassEvent = settings.renderPassEvent;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Apply only to the main game window
            if (renderingData.cameraData.cameraType == CameraType.Game)
            {
                renderer.EnqueuePass(_psxPass);
            }
        }

        protected override void Dispose(bool disposing) => _psxPass?.Cleanup();
    }
}