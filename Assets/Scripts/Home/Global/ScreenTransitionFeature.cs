using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using System.Collections;

public class ScreenTransitionFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class ScreenTransitionSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Shader shader;
        
        [Header("Transition Properties")]
        public Texture2D transitionTexture;
        [Range(0f, 1f)]
        public float progress = 0f;
        [Range(0f, 1f)]
        public float smoothness = 0.1f;
        public Color transitionColor = Color.black;
        public bool invert = false;
    }

    public ScreenTransitionSettings settings = new ScreenTransitionSettings();
    private ScreenTransitionPass transitionPass;
    private Material transitionMaterial;
    private bool isTransitioning = false;
    private MonoBehaviour targetMonoBehaviour;

    // The class that will run the effect
    class ScreenTransitionPass : ScriptableRenderPass
    {
        private Material material;
        private ScreenTransitionSettings settings;
        private RTHandle sourceTexture;
        private RTHandle tempTexture;
        private string profilerTag;

        public ScreenTransitionPass(string tag, ScreenTransitionSettings settings, Material material)
        {
            this.settings = settings;
            this.material = material;
            profilerTag = tag;
        }

        public void Setup(RTHandle source)
        {
            this.sourceTexture = source;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Create or update temporary RT handle
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            
            // Ensure we have a temp texture of the right size
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TempTransitionTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
            {
                Debug.LogError("Screen Transition material is null!");
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
            
            // Set shader properties
            material.SetTexture("_TransitionTex", settings.transitionTexture);
            material.SetFloat("_Progress", settings.progress);
            material.SetFloat("_Smoothness", settings.smoothness);
            material.SetFloat("_Invert", settings.invert ? 1.0f : 0.0f);
            material.SetColor("_Color", settings.transitionColor);

            using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
            {
                // Use BlitCameraTexture for RTHandle compatibility
                Blitter.BlitCameraTexture(cmd, sourceTexture, tempTexture, material, 0);
                Blitter.BlitCameraTexture(cmd, tempTexture, sourceTexture);
            }
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // No need to manually release temp texture as it's handled by RTHandle system
        }

        public void Dispose()
        {
            tempTexture?.Release();
        }
    }

    public override void Create()
    {
        // Create the pass
        if (settings.shader == null)
        {
            Debug.LogError("Screen Transition shader is not assigned!");
            return;
        }

        transitionMaterial = CoreUtils.CreateEngineMaterial(settings.shader);
        transitionPass = new ScreenTransitionPass("Screen Transition", settings, transitionMaterial);
        transitionPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (transitionMaterial == null)
        {
            Debug.LogError("Screen Transition material could not be created!");
            return;
        }

        // Only render the effect if the progress is not 0 (invisible)
        if (settings.progress > 0.001f)
        {
            // Get camera target using RTHandle
            var cameraColorTarget = renderer.cameraColorTargetHandle;
            transitionPass.Setup(cameraColorTarget);
            renderer.EnqueuePass(transitionPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (transitionMaterial != null)
        {
            CoreUtils.Destroy(transitionMaterial);
        }
        
        // Clean up the pass resources
        if (transitionPass != null)
        {
            (transitionPass as ScreenTransitionPass)?.Dispose();
        }
    }

    // Create a default transition texture
    public void CreateDefaultTransitionTexture()
    {
        if (settings.transitionTexture != null)
            return;

        int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.R8, false);
        texture.filterMode = FilterMode.Bilinear;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Create a radial gradient
                float u = (x / (float)size) * 2 - 1;
                float v = (y / (float)size) * 2 - 1;
                float distance = Mathf.Sqrt(u * u + v * v);
                float value = Mathf.Clamp01(distance);
                
                // Set pixel
                texture.SetPixel(x, y, new Color(value, value, value, 1));
            }
        }
        
        texture.Apply();
        settings.transitionTexture = texture;
    }

    // Setup a MonoBehaviour to handle transitions
    public void SetupTransitionController(MonoBehaviour mono)
    {
        targetMonoBehaviour = mono;
    }

    // Fade in (from transition color to scene)
    public void FadeIn(float duration = 1.0f, AnimationCurve curve = null, Action onComplete = null)
    {
        if (targetMonoBehaviour == null)
        {
            Debug.LogError("No MonoBehaviour set for transitions! Call SetupTransitionController first.");
            return;
        }

        targetMonoBehaviour.StartCoroutine(TransitionCoroutine(1.0f, 0.0f, duration, curve, onComplete));
    }

    // Fade out (from scene to transition color)
    public void FadeOut(float duration = 1.0f, AnimationCurve curve = null, Action onComplete = null)
    {
        if (targetMonoBehaviour == null)
        {
            Debug.LogError("No MonoBehaviour set for transitions! Call SetupTransitionController first.");
            return;
        }

        targetMonoBehaviour.StartCoroutine(TransitionCoroutine(0.0f, 1.0f, duration, curve, onComplete));
    }

    // Transition coroutine
    private IEnumerator TransitionCoroutine(float startValue, float endValue, float duration, AnimationCurve curve, Action onComplete)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Another transition is already in progress.");
            yield break;
        }

        // Use default curve if none provided
        if (curve == null)
            curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        isTransitioning = true;
        float startTime = Time.time;
        float endTime = startTime + duration;
        
        while (Time.time < endTime)
        {
            float normalizedTime = (Time.time - startTime) / duration;
            float curveValue = curve.Evaluate(normalizedTime);
            settings.progress = Mathf.Lerp(startValue, endValue, curveValue);
            yield return null;
        }
        
        // Ensure we end at the exact target value
        settings.progress = endValue;
        isTransitioning = false;
        
        // Execute callback if provided
        onComplete?.Invoke();
    }
}