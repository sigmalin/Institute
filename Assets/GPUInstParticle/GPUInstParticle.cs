using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GPUInstParticle : MonoBehaviour, IGPUInstParticle
{
    public ComputeShader csParticle;

    public ComputeShader csCulling;

    public Material material;

    public bool enableGpuCulling;

    Mesh cube;

    int particlePosWorldID;

    int particleBufferID;
    int particleCountID;
    int noiseScaleID;
    int speedFactorID;
    int liftTimeID;
    int deltaTimeID;
    int elapsedTimeID;

    int perlinBufferID;
    int octavesID;

    int cullResultID;
    int frustumPlaneID;

    ComputeBuffer particleBuffer;
    ComputeBuffer argBuffer;
    ComputeBuffer perlinBuffer;
    ComputeBuffer cullResultBuffer;

    bool isInitial;

    const int ARG_BUFFER_SIZE = 5;    
    readonly int PARTICLE_SIZE = sizeof(float) * 12;

    const int PARTICLE_COUNT = 5000;
    const float NOISE_SCALE = 4f;
    const float SPEED_FACTOR = 4f;
    const float LIFT_TIME = 5f;

    // Start is called before the first frame update
    void Start()
    {
        particlePosWorldID = Shader.PropertyToID("_particlePosWorld");

        particleBufferID = Shader.PropertyToID("particleBuffer");
        particleCountID = Shader.PropertyToID("_ParticleCount");
        noiseScaleID = Shader.PropertyToID("_NoiseScale");
        speedFactorID = Shader.PropertyToID("_SpeedFactor");
        liftTimeID = Shader.PropertyToID("_LiftTime");
        deltaTimeID = Shader.PropertyToID("_DeltaTime");
        elapsedTimeID = Shader.PropertyToID("_ElapsedTime");

        perlinBufferID = Shader.PropertyToID("_Perm");
        octavesID = Shader.PropertyToID("_Octaves");

        cullResultID = Shader.PropertyToID("cullResult");
        frustumPlaneID = Shader.PropertyToID("_FrustumPlane");

        isInitial = false;
    }

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        Release();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isValid() == false) return;

        if (GPUInstParticleRenderPassFeature.Instance)
        {
            GPUInstParticleRenderPassFeature.Instance.Register(this);

            CommandBuffer cmd = CommandBufferPool.Get();

            try
            {
                Process(cmd);
                Cull(cmd, Camera.main);
                Graphics.ExecuteCommandBuffer(cmd);
            }
            catch
            {
                
            }

            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }        
    }

    private void Initialize()
    {
        Release();

        if (SystemInfo.supportsComputeShaders == false)
            return;

        CubeMaker.Generate(1f, out cube);
                
        argBuffer = new ComputeBuffer(ARG_BUFFER_SIZE, sizeof(uint), ComputeBufferType.IndirectArguments);

        initPartialBuffer();

        initCullResultBuffer();

        initPerlinNoise(Time.time);
    }

    private void Release()
    {
        if (cube)
        {
            cube.Clear();
            cube = null;
        }

        if (particleBuffer != null)
        {
            particleBuffer.Release();
            particleBuffer = null;
        }

        if(argBuffer != null)
        {
            argBuffer.Release();
            argBuffer = null;
        }

        if (perlinBuffer != null)
        {
            perlinBuffer.Release();
            perlinBuffer = null;
        }

        if(cullResultBuffer != null)
        {
            cullResultBuffer.Release();
            cullResultBuffer = null;
        }
    }

    private void initPartialBuffer()
    {
        particleBuffer = new ComputeBuffer(PARTICLE_COUNT, PARTICLE_SIZE);

        isInitial = false;
    }

    private void initCullResultBuffer()
    {
        cullResultBuffer = new ComputeBuffer(PARTICLE_COUNT, PARTICLE_SIZE, ComputeBufferType.Append);
    }

    private void initPerlinNoise(float _seed)
    {
        int[] perm = {
            151,160,137,91,90,15,
            131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
            190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
            88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
            77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
            102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
            135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
            5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
            223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
            129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
            251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
            49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
            138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        UnityEngine.Random.InitState(Mathf.FloorToInt(_seed));

        int[] shuffle = new int[256];
        for (int i = 0; i < 256; ++i)
            shuffle[i] = i;

        for (int i = 0; i < 256; ++i)
        {
            int target = UnityEngine.Random.Range(0, 256);
            int swap = shuffle[i];
            shuffle[i] = shuffle[target];
            shuffle[target] = swap;
        }

        int[] p = new int[512];
        for (int i = 0; i < 512; ++i)
        {
            int t = shuffle[i % 256];
            p[i] = perm[t];
        }

        perlinBuffer = new ComputeBuffer(512, sizeof(int));
        perlinBuffer.SetData(p);
    }

    private bool isValid()
    {
        return GPUInstParticleRenderPassFeature.Instance
            && (particleBuffer != null) && (argBuffer != null) 
            && (perlinBuffer != null) && (cullResultBuffer != null)
            && (material != null) && (cube != null) 
            && (csParticle != null) && (csCulling != null);
    }

    private void Process(CommandBuffer _cmd)
    {
        int kernel = isInitial ? csParticle.FindKernel("CSMain") : csParticle.FindKernel("CSInitial");

        isInitial = true;

        uint sizeX;
        csParticle.GetKernelThreadGroupSizes(
            kernel,
            out sizeX,
            out _,
            out _
        );

        // set perlin
        csParticle.SetBuffer(kernel, perlinBufferID, perlinBuffer);
        csParticle.SetInt(octavesID, 2);

        // set curl
        csParticle.SetBuffer(kernel, particleBufferID, particleBuffer);
        csParticle.SetInt(particleCountID, PARTICLE_COUNT);
        csParticle.SetFloat(noiseScaleID, NOISE_SCALE);
        csParticle.SetFloat(speedFactorID, SPEED_FACTOR);
        csParticle.SetFloat(liftTimeID, LIFT_TIME);

        csParticle.SetFloat(deltaTimeID, Time.deltaTime);
        csParticle.SetFloat(elapsedTimeID, Time.time);

        csParticle.SetVector(particlePosWorldID, this.transform.position);

        _cmd.DispatchCompute(csParticle, kernel, 
            Mathf.CeilToInt((PARTICLE_COUNT + sizeX - 1) / sizeX),
            1, 1);
    }

    private void Cull(CommandBuffer _cmd, Camera _cam)
    {
        if (enableGpuCulling == false) return;

        int kernel = csCulling.FindKernel("CSMain");

        uint sizeX;
        csCulling.GetKernelThreadGroupSizes(
            kernel,
            out sizeX,
            out _,
            out _
        );

        Vector4[] planes = CullingTool.GetFrustumPlane(_cam);

        cullResultBuffer.SetCounterValue(0);

        csCulling.SetBuffer(kernel, particleBufferID, particleBuffer);
        csCulling.SetBuffer(kernel, cullResultID, cullResultBuffer);

        csCulling.SetInt(particleCountID, PARTICLE_COUNT);
        csCulling.SetVectorArray(frustumPlaneID, planes);

        _cmd.DispatchCompute(csCulling, kernel,
            Mathf.CeilToInt((PARTICLE_COUNT + sizeX - 1) / sizeX),
            1, 1);
    }

    public bool onRender(CommandBuffer _cmd, Camera _cam)
    {
        // Indirect args
        uint[] args = new uint[ARG_BUFFER_SIZE]
        {
            0,0,0,0,0
        };

        const int SUBMESH_INDEX = 0;
        const int SHADER_PASS = 0;

        if (cube)
        {
            args[0] = (uint)cube.GetIndexCount(SUBMESH_INDEX);  // index count per instance
            args[1] = (uint)PARTICLE_COUNT;                     // instance count
            args[2] = (uint)cube.GetIndexStart(SUBMESH_INDEX);  // start index location
            args[3] = (uint)cube.GetBaseVertex(SUBMESH_INDEX);  // base vertex location
            args[4] = (uint)0;                                  // start instance location
        }

        argBuffer.SetData(args);

        // cull
        if(enableGpuCulling)
        {
            ComputeBuffer.CopyCount(cullResultBuffer, argBuffer, sizeof(uint));

            material.SetBuffer(particleBufferID, cullResultBuffer);
        }
        else
        {
            material.SetBuffer(particleBufferID, particleBuffer);
        }

        _cmd.DrawMeshInstancedIndirect(
            cube,
            SUBMESH_INDEX,
            material,
            SHADER_PASS,
            argBuffer
            );

        return true;
    }
}
