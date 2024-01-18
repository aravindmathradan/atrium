using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TestTerrainProcessor : MonoBehaviour {
  [SerializeField] private int terrainFlatRadius = 10;
  [SerializeField] private int terrainSmoothRadius = 4;
  [SerializeField] private float treeEraseRadius = 15f;

  private Terrain terrain;
  private TerrainData terrainData;
  private Vector3 terrainSize;
  private int heightmapRes;
  private int holesmapRes;
  private static float[,] initialHeightmap;
  private static bool[,] initialHolesmap;
  private string heightmapFileName = "test_terrain_heights.dat";
  private string holesmapFileName = "test_terrain_holes.dat";
  private static TreeInstance[] initialTreeInstances;

  private void Start() {
    terrain = gameObject.GetComponent<Terrain>();
    terrainData = terrain.terrainData;
    terrainSize = terrainData.size;
    heightmapRes = terrainData.heightmapResolution;
    holesmapRes = terrainData.holesResolution;
    initialTreeInstances = terrainData.treeInstances;


    LoadTerrainHeightmap(heightmapFileName);
    LoadTerrainHolesmap(holesmapFileName);

    // TerrainTest();
  }

  private void TerrainTest() {
    Bounds bound = GameObject.Find("Cube").GetComponent<Renderer>().bounds;
    List<Bounds> bounds = new List<Bounds>();
    bounds.Add(bound);

    // Bounds bound2 = GameObject.Find("Capsule").GetComponent<Renderer>().bounds;
    // Debug.Log(bound2.center);
    // bounds.Add(bound2);

    // Bounds bound3 = GameObject.Find("GameObject").GetComponent<Renderer>().bounds;
    // Debug.Log(bound3.center);
    // Debug.Log(bound3.min);
    // Debug.Log(bound3.max);
    // Debug.Log(bound3.size);
    // FlattenTerrain(bounds);
    // PaintHoles(bounds);
  }

  private void LoadTerrainHeightmap(string fileName) {
    // Save the initial terrain heightmap to a file
    string filePath = Application.persistentDataPath + "/" + fileName;

    BinaryFormatter bf = new BinaryFormatter();
    FileStream file;

    if (!File.Exists(filePath)) {
      initialHeightmap = terrainData.GetHeights(0, 0, heightmapRes, heightmapRes);
      file = File.Create(filePath);
      bf.Serialize(file, initialHeightmap);
    } else {
      file = File.Open(filePath, FileMode.Open);
      initialHeightmap = (float[,])bf.Deserialize(file);
    }

    file.Close();
  }


  private void LoadTerrainHolesmap(string fileName) {
    // Save the initial terrain heightmap to a file
    string filePath = Application.persistentDataPath + "/" + fileName;

    BinaryFormatter bf = new BinaryFormatter();
    FileStream file;

    if (!File.Exists(filePath)) {
      initialHolesmap = terrainData.GetHoles(0, 0, holesmapRes, holesmapRes);
      file = File.Create(filePath);
      bf.Serialize(file, initialHolesmap);
    } else {
      file = File.Open(filePath, FileMode.Open);
      initialHolesmap = (bool[,])bf.Deserialize(file);
    }

    file.Close();
  }

  public void FlattenTerrain(List<Bounds> bounds) {
    float[,] heightmap = (float[,])initialHeightmap.Clone();
    foreach (var bound in bounds) {
      Vector2Int boundHeightmapPos = new Vector2Int(
        Mathf.RoundToInt(((bound.center.x - terrain.transform.position.x) / terrainSize.x) * heightmapRes),
        Mathf.RoundToInt(((bound.center.z - terrain.transform.position.z) / terrainSize.z) * heightmapRes)
      );
      for (int x = -terrainFlatRadius; x <= terrainFlatRadius; x++) {
        for (int y = -terrainFlatRadius; y <= terrainFlatRadius; y++) {
          int terrainX = boundHeightmapPos.x + x;
          int terrainY = boundHeightmapPos.y + y;
          heightmap[terrainY, terrainX] = 0;
        }
      }
    }
    heightmap = GaussianBlur(heightmap);
    terrainData.SetHeights(0, 0, heightmap);
  }

  public void PaintHoles(List<Bounds> bounds) {
    bool[,] holesmap = (bool[,])initialHolesmap.Clone();
    foreach (var bound in bounds) {
      Vector2Int boundHolesmapPos = new Vector2Int(
        Mathf.RoundToInt(((bound.center.x - terrain.transform.position.x) / terrainSize.x) * holesmapRes),
        Mathf.RoundToInt(((bound.center.z - terrain.transform.position.z) / terrainSize.z) * holesmapRes)
      );
      holesmap[boundHolesmapPos.y, boundHolesmapPos.x] = false;
    }
    terrainData.SetHoles(0, 0, holesmap);
  }

  public void ClearTrees(List<Bounds> bounds) {
    List<TreeInstance> treeInstances = new List<TreeInstance>();
    treeInstances.AddRange((TreeInstance[])initialTreeInstances.Clone());
    foreach (var bound in bounds) {
      for (int i = 0; i < treeInstances.Count; i++) {
        Vector3 treeWorldPosition = Vector3.Scale(treeInstances[i].position, terrainSize) + terrain.transform.position;

        // Because y axis position will not be 0 since it was placed before flattening the terrain.
        float horizontalDistanceFromCenterOfBound = Vector2.Distance(
          new Vector2(treeWorldPosition.x, treeWorldPosition.z),
          new Vector2(bound.center.x, bound.center.z)
        );

        if (horizontalDistanceFromCenterOfBound < treeEraseRadius) {
          treeInstances.Remove(treeInstances[i]);
        }
      }
    }

    terrainData.treeInstances = treeInstances.ToArray<TreeInstance>();
    terrain.Flush();
  }

  private void OnDestroy() {
    ResetTerrain();
  }

  public void ResetTerrain() {
    terrain.terrainData.SetHeights(0, 0, initialHeightmap);
    terrain.terrainData.SetHoles(0, 0, initialHolesmap);
    terrain.terrainData.treeInstances = initialTreeInstances;
    terrain.Flush();
  }

  // Written by chatGPT
  private float[,] GaussianBlur(float[,] inputHeightMap) {
    int width = inputHeightMap.GetLength(0);
    int height = inputHeightMap.GetLength(1);
    float[,] outputHeightMap = new float[width, height];

    // The size of the kernel used for the Gaussian blur
    int kernelSize = (2 * terrainSmoothRadius) + 1;
    // The standard deviation used for the Gaussian blur
    float sigma = Mathf.Max((float)(terrainSmoothRadius / 2), 1f);

    // Create the Gaussian kernel
    float[,] kernel = new float[kernelSize, kernelSize];
    float twoSigmaSquared = 2 * sigma * sigma;
    float kernelSum = 0f;

    int halfKernelSize = kernelSize / 2;

    for (int x = -halfKernelSize; x <= halfKernelSize; x++) {
      for (int y = -halfKernelSize; y <= halfKernelSize; y++) {
        float distanceSquared = x * x + y * y;
        kernel[x + halfKernelSize, y + halfKernelSize] = Mathf.Exp(-distanceSquared / twoSigmaSquared);
        kernelSum += kernel[x + halfKernelSize, y + halfKernelSize];
      }
    }

    // Normalize the kernel
    for (int x = 0; x < kernelSize; x++) {
      for (int y = 0; y < kernelSize; y++) {
        kernel[x, y] /= kernelSum;
      }
    }

    // Apply the kernel to each pixel in the input array
    for (int x = 0; x < width; x++) {
      for (int y = 0; y < height; y++) {
        float sum = 0f;
        for (int i = -halfKernelSize; i <= halfKernelSize; i++) {
          for (int j = -halfKernelSize; j <= halfKernelSize; j++) {
            int pixelX = Mathf.Clamp(x + i, 0, width - 1);
            int pixelY = Mathf.Clamp(y + j, 0, height - 1);
            sum += inputHeightMap[pixelX, pixelY] * kernel[i + halfKernelSize, j + halfKernelSize];
          }
        }
        outputHeightMap[x, y] = sum;
      }
    }

    return outputHeightMap;
  }
}
