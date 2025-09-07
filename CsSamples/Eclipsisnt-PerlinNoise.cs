
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PerlinNoise
{
    public int width;
    public int height;
    public int numLandLayers;
    public int numSpireLayers;
    public float scale;
    public float scaleLayerCoef;
    public float[,] offsets;
    public int roundingNum;
    public int halfMapSize;
    public int mapSizeSq;
    public float gapSize;

    public PerlinNoise(int _width, int _height, int _numLandLayers, int _numSpireLayers, float _scale, float _scaleLayerCoef, float[,] _offsets, int _roundingNum, int _halfMapSize, int _mapSizeSq, float _gapSize)
    {
        width = _width;
        height = _height;
        numLandLayers = _numLandLayers;
        numSpireLayers = _numSpireLayers;
        scale = _scale;
        scaleLayerCoef = _scaleLayerCoef;
        offsets = _offsets;
        roundingNum = _roundingNum;
        halfMapSize = _halfMapSize;
        mapSizeSq = _mapSizeSq;
        gapSize = _gapSize;
    }
    
    public float[,] GenerateLandGrid(int layer)
    {
        float[,] map = new float[width, height];

        // Generate a perlin noise map for the texture
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x,y] = CalculateCell(x, y, layer);
            }
        }

        return map;
    }

    public Texture2D GenerateLandTexture(float[,] map, int[,] islands, int numIslands)
    {
        Texture2D texture = new Texture2D(width, height);

        // Generate a perlin noise map for the texture
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Debug.Log(islands[x, y] + " " + numIslands);
                Color color = CalculateMapPixel(map[x,y], islands[x, y], numIslands);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    public float CalculateCell(int x, int y, int layer)
    {
        int circleX = (x - halfMapSize);
        int circleY = (y - halfMapSize);

        if ((circleX * circleX + circleY * circleY) < mapSizeSq)
        {
            float sample = CalculateNoise(x, y, layer);

            if (sample <= gapSize)
            { // Ocean
                return 2f;
            }

            float landSample = (1-(sample - gapSize) / 1f); // Get a new sample exclusively for land to give a broader range of values.

            // Normal land -- TODO: (1) make grass appear?? (2) spires??
            //Debug.Log("landSample" + landSample);
            return landSample;
        }
        else
        {
            return -1f;
        }
    }

    public Color CalculateMapPixel(float value, int islandValue = 0, int numIslands = 1)
    {
        if (value == -1f)                       // -1f: No map here
            return new Color(0, 0, 0);
        else if (value >= 0f && value <= 1f) {  // 0f to 1f: Land mass 
            //return new Color(0.05f, value, 0.2f);
            return new Color(0.05f, ((float)islandValue) / ((float)numIslands), 0.2f);
        }
        else if (value == 2f)                   // 2f: Ocean
            return new Color(0f, 0.3f, 0.6f);
        else if (value == 3f)                   // 3f: Pump
            return new Color(0.6f, 0.25f, 0.15f);
        else if (value == 4f)                   // 4f: Beacon
            return new Color(0.5f, 0.5f, 0.1f);
        else if (value >= 5f && value <= 6f)    // 5f to 6f: Spire height range????? Prob needs another value but whatever
            return new Color(0.75f, 0.25f, 0.15f);
        else
            return new Color(1, 1, 1); // Uhhh??? Shouldn't happen so let's make it white.
    }

    public float CalculateNoise(int x, int y, int layer)
    {
        float xCoord = (float)x / width;
        float yCoord = (float)y / height;

        float endSample;


        if (layer == -1)
        {
            float sampleTotal = 0f;
            int countSamples = 0;

            for (int i = 0; i < numLandLayers; i++)
            {
                float scaleModifier = scale + (i + 1) * scale * scaleLayerCoef;

                sampleTotal += Mathf.PerlinNoise(xCoord * scaleModifier + offsets[i,0], yCoord * scaleModifier + offsets[i,1]) * (numLandLayers-(i));
                countSamples += (numLandLayers - (i));
            }

            endSample = sampleTotal / (float)countSamples;
        }
        else
        {
            float scaleModifier = scale + (layer + 1) * scale * scaleLayerCoef;

            float sample = Mathf.PerlinNoise(xCoord * scaleModifier + offsets[layer,0], yCoord * scaleModifier + offsets[layer, 1]);

            endSample = sample;
        }

        return Mathf.Floor(endSample * (float)roundingNum) / (float)roundingNum;
    }
}
