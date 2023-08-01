using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RayTracingMaster : MonoBehaviour
{
    [SerializeField, Range(0.2f, 1f)]
    float resolution;
    public Light DirectionalLight;
    public ComputeShader RayTracingShader;
    private RenderTexture _target;
    private RenderTexture _converged;
    private Camera _camera;
    private uint _currentSample = 0;
    public Material _addMaterial;

    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100.0f;

    private ComputeBuffer _sphereBuffer;

    struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    };

    public int WIDTH
    {
        get { return Mathf.CeilToInt(Screen.width * resolution); }
    }

    public int HEIGHT
    {
        get { return Mathf.CeilToInt(Screen.height * resolution); }
    }
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        SetUpScene();
        _currentSample = 0;
    }

    private void OnDestroy()
    {
        _sphereBuffer.Release();
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            _currentSample = 0;
            transform.hasChanged = false;
        }
        if (DirectionalLight.transform.hasChanged)
        {
            _currentSample = 0;
            DirectionalLight.transform.hasChanged = false;
        }
    }

    private void SetUpScene()
    {
        List<Sphere> spheres = new List<Sphere>();
        // Add a number of random spheres
        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();
            // Radius and radius
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);
            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }
            // Albedo and specular color
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.0f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;
            sphere.smoothness = Random.value;
            if (Random.value < 0.5f)
            {
                Color emission = Color.Lerp(Color.black, color, Random.value);
                sphere.emission = new Vector3(emission.r, emission.g, emission.b) * Random.value * 5;
            }
            else
            {
                sphere.emission = Vector3.zero;
            }
            // Add the sphere to the list
            spheres.Add(sphere);
        SkipSphere:
            continue;
        }
        /*// sun
        Vector3 sunColor = new Vector3(DirectionalLight.color.r, DirectionalLight.color.g, DirectionalLight.color.b);
        spheres.Add(new Sphere 
            {
                position = DirectionalLight.transform.position,
                radius = DirectionalLight.transform.localScale.x,
                albedo = sunColor,
                specular = Vector3.zero,
                smoothness = 0,
                emission = sunColor * DirectionalLight.intensity,
            }
        );*/
        // Assign to compute buffer
        _sphereBuffer = new ComputeBuffer(spheres.Count, 14 * 4); // # of floats * 4
        _sphereBuffer.SetData(spheres);
    }

    private void SetShaderParameters()
    {
        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);
        RayTracingShader.SetFloat("_Seed", Random.value);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }
    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture(ref _target);
        InitRenderTexture(ref _converged);
        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(WIDTH / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(HEIGHT / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_target, _converged, _addMaterial);
        Graphics.Blit(_converged, destination);
        _currentSample++;
    }
    private void InitRenderTexture(ref RenderTexture tex)
    {
        if (tex == null || tex.width != WIDTH || tex.height != HEIGHT)
        {
            // Release render texture if we already have one
            if (tex != null)
                tex.Release();
            // Get a render target for Ray Tracing
            tex = new RenderTexture(WIDTH, HEIGHT, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            tex.enableRandomWrite = true;
            tex.Create();
            _currentSample = 0;
        }
    }
}