using UnityEngine;
using UnityEngine.SceneManagement;

public static class AndroidGameSceneOptimizer
{
    private const string GameSceneName = "GameScene";
    private const int TargetFrameRate = 30;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (!Application.isMobilePlatform)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyIfNeeded(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyIfNeeded(scene);
    }

    private static void ApplyIfNeeded(Scene scene)
    {
        if (!scene.IsValid() || scene.name != GameSceneName)
            return;

        ApplyMobileQuality();
        OptimizeLights();
        OptimizeTerrains();
        OptimizeCameras();
    }

    private static void ApplyMobileQuality()
    {
        Application.targetFrameRate = TargetFrameRate;
        QualitySettings.vSyncCount = 0;

        int lowQualityIndex = FindQualityLevelIndex("Low");
        if (lowQualityIndex >= 0 && QualitySettings.GetQualityLevel() != lowQualityIndex)
            QualitySettings.SetQualityLevel(lowQualityIndex, true);

        QualitySettings.globalTextureMipmapLimit = Mathf.Max(QualitySettings.globalTextureMipmapLimit, 1);
        QualitySettings.shadowDistance = 20f;
        QualitySettings.lodBias = Mathf.Min(QualitySettings.lodBias, 0.5f);
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
    }

    private static void OptimizeLights()
    {
        Light[] lights = Object.FindObjectsOfType<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            Light light = lights[i];
            if (light == null || !light.enabled)
                continue;

            if (light.type == LightType.Directional)
            {
                light.shadows = LightShadows.Hard;
                light.shadowStrength = Mathf.Min(light.shadowStrength, 0.75f);
                light.renderMode = LightRenderMode.Auto;
            }
            else
            {
                light.shadows = LightShadows.None;
                light.renderMode = LightRenderMode.ForceVertex;
            }
        }
    }

    private static void OptimizeTerrains()
    {
        Terrain[] terrains = Object.FindObjectsOfType<Terrain>(true);
        for (int i = 0; i < terrains.Length; i++)
        {
            Terrain terrain = terrains[i];
            if (terrain == null)
                continue;

            terrain.drawInstanced = true;
            terrain.heightmapPixelError = Mathf.Max(terrain.heightmapPixelError, 32f);
            terrain.basemapDistance = Mathf.Min(terrain.basemapDistance, 64f);
            terrain.detailObjectDistance = Mathf.Min(terrain.detailObjectDistance, 20f);
            terrain.detailObjectDensity = Mathf.Min(terrain.detailObjectDensity, 0.35f);
            terrain.treeDistance = Mathf.Min(terrain.treeDistance, 300f);
            terrain.treeBillboardDistance = Mathf.Min(terrain.treeBillboardDistance, 30f);
            terrain.treeMaximumFullLODCount = Mathf.Min(terrain.treeMaximumFullLODCount, 12);
        }
    }

    private static void OptimizeCameras()
    {
        Camera[] cameras = Object.FindObjectsOfType<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (camera == null)
                continue;

            camera.allowHDR = false;
            camera.allowMSAA = false;

            if (camera.farClipPlane > 500f)
                camera.farClipPlane = 500f;
        }
    }

    private static int FindQualityLevelIndex(string qualityName)
    {
        string[] qualityNames = QualitySettings.names;
        for (int i = 0; i < qualityNames.Length; i++)
        {
            if (qualityNames[i] == qualityName)
                return i;
        }

        return -1;
    }
}