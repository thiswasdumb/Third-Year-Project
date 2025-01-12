using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public class TerrainReceiver : MonoBehaviour
{
    public Terrain terrain;
    private Light directionalLight;

    private void Start()
    {
        terrain = GetComponent<Terrain>();
        directionalLight = GameObject.FindObjectOfType<Light>();

        if (terrain == null)
        {
            Debug.LogError("Terrain component not found!");
            return;
        }

        if (directionalLight == null)
        {
            Debug.LogError("Directional Light not found! Please add one to the scene.");
            return;
        }

        // Start reading the file periodically
        InvokeRepeating("ReadTerrainDataFromFile", 1f, 1f);
    }

    private void ReadTerrainDataFromFile()
    {
        string filePath = "/Users/salrawi.22/3yp/Assets/terrain_data.json";

        if (File.Exists(filePath))
        {
            try
            {
                string jsonData = File.ReadAllText(filePath);
                ReceivedTerrainData terrainData = JsonConvert.DeserializeObject<ReceivedTerrainData>(jsonData);
                ApplyTerrainData(terrainData);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error reading file: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("File not found: " + filePath);
        }
    }

    private void ApplyTerrainData(ReceivedTerrainData terrainData)
    {
        Debug.Log("Applying terrain data with lighting and terrain adjustments...");

        // Apply heightmap and roughness effects
        ApplyHeightAndRoughness(terrainData);

        // Adjust lighting based on lighting_atmosphere value
        AdjustLighting(terrainData.lighting_atmosphere);
    }

    private void ApplyHeightAndRoughness(ReceivedTerrainData terrainData)
    {
        TerrainData terrainDataComponent = terrain.terrainData;
        int resolution = terrainDataComponent.heightmapResolution;
        float[,] heights = new float[resolution, resolution];

        // Adjust Perlin noise parameters based on heightmap and roughness values
        float scale = Mathf.Lerp(0.005f, 0.03f, terrainData.roughness); // Adjust roughness for terrain definition
        float amplitude = Mathf.Clamp(terrainData.heightmap, 0.1f, 1.0f);

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float xCoord = x * scale;
                float yCoord = y * scale;
                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord) * amplitude;
            }
        }

        terrainDataComponent.SetHeights(0, 0, heights);
        Debug.Log("Terrain height and roughness applied successfully!");
    }

private void AdjustLighting(float lightingLevel)
{
    // Ensure directional light is available
    if (directionalLight != null)
    {
        // Increase the intensity range to exaggerate lighting changes
        directionalLight.intensity = Mathf.Lerp(0.2f, 2.0f, lightingLevel);

        // Define colors to simulate distinct times of day with more contrast
        Color dawnColor = new Color(1.0f, 0.4f, 0.2f);    // Warmer orange for dawn
        Color noonColor = Color.white;                     // Bright white for noon
        Color duskColor = new Color(0.8f, 0.3f, 0.2f);     // Deeper red-orange for dusk
        Color nightColor = new Color(0.1f, 0.1f, 0.35f);   // Cool blue for nighttime

        // Adjust color based on `lightingLevel`
        if (lightingLevel < 0.25f)
        {
            directionalLight.color = Color.Lerp(dawnColor, noonColor, lightingLevel * 4);
        }
        else if (lightingLevel < 0.5f)
        {
            directionalLight.color = Color.Lerp(noonColor, duskColor, (lightingLevel - 0.25f) * 4);
        }
        else if (lightingLevel < 0.75f)
        {
            directionalLight.color = Color.Lerp(duskColor, nightColor, (lightingLevel - 0.5f) * 4);
        }
        else
        {
            directionalLight.color = Color.Lerp(nightColor, dawnColor, (lightingLevel - 0.75f) * 4);
        }

        Debug.Log($"Lighting Level: {lightingLevel}, Intensity: {directionalLight.intensity}, Color: {directionalLight.color}");
    }
    else
    {
        Debug.LogWarning("Directional light not found.");
    }
}


    [System.Serializable]
    public class ReceivedTerrainData
    {
        public float heightmap;
        public float texture;
        public float roughness;
        public float biome_type;
        public float object_placement;
        public float water_features;
        public float lighting_atmosphere;
    }
}