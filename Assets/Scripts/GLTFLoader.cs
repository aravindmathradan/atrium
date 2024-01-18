using System.Collections.Generic;
using System.Linq;
using GLTFast;
using UnityEngine;
using static TimeLogger;

public class GLTFLoader : MonoBehaviour {
  private GltfImport gltf;
  private ImportSettings settings;
  private BuildingMap buildingMap;
  public GameObject terrainGameObject;
  private TerrainProcessor terrainProcessor;
  public static Bounds sceneBounds;

  private void Awake() {
    terrainProcessor = terrainGameObject.GetComponent<TerrainProcessor>();
    buildingMap = new BuildingMap();
  }

  private void Start() {
    Building currentBuilding = buildingMap.GetCurrentBuilding();
    if (currentBuilding.isValid) {
      ImportModel(currentBuilding.url);
    } else {
      // Test
      ImportModel("https://storage.googleapis.com/atrium-af29c.appspot.com/BuildingModels/room/scene.glb?X-Goog-Algorithm=GOOG4-RSA-SHA256&X-Goog-Credential=firebase-adminsdk-7jipo%40atrium-af29c.iam.gserviceaccount.com%2F20230318%2Fauto%2Fstorage%2Fgoog4_request&X-Goog-Date=20230318T091739Z&X-Goog-Expires=3601&X-Goog-SignedHeaders=host&X-Goog-Signature=aa9194e20290a18fd13a87a61752734284e5c09d28f03f0e8f42c0d6a2df5b9a5439802ac47a3c930600451825b82f4726c550d4553851dbe80bd9d926595d13634606540e68d7166b08d0d96e9335d8e2731bbc4c90b52b11fcdc450a7c978f8132af74eb80856b8dfdc2e098b52da499d55ffbb10ff54acfcdbdde6b03e8b8f5309314f89e430d41e409ed92fcc4536eaa775a3885666e6818ff02139c1e624d5774bf0945bb0b753f7401c9c053caf48d9075e92b4419e87ae888c2e02027a42f5315e16fc2f581e67eeede515df5da0e5ef444951d4a0c1cb5a6bf84e7f7148f1e6195159d47c6131178e381d28ea5478034ef001b6a03e6265b14df6dd5");
    }
  }

  private async void ImportModel(string url) {
    gltf = new GltfImport();
    settings = new ImportSettings {
      GenerateMipMaps = true,
      AnisotropicFilterLevel = 3,
      NodeNameMethod = NameImportMethod.OriginalUnique,
      AnimationMethod = AnimationMethod.None,
      DefaultMagFilterMode = GLTFast.Schema.Sampler.MagFilterMode.Linear,
      DefaultMinFilterMode = GLTFast.Schema.Sampler.MinFilterMode.Linear
    };

    var success = await gltf.Load(url, settings);
    if (success) {
      await gltf.InstantiateMainSceneAsync(gameObject.transform);
      TimeLogger.TimerStart();
      ProcessModelV2();
      TimeLogger.TimerEnd();
    } else {
      Debug.LogError("Loading glTF model failed");
    }
  }

  private void ProcessModel() {
    List<Bounds> bounds = new List<Bounds>();
    List<Transform> allChildrenObjects = GetComponentsInChildren<Transform>().ToList();

    foreach (var childObject in allChildrenObjects) {
      AttachColliders(childObject.gameObject);

      if (childObject.TryGetComponent<Renderer>(out Renderer renderer)) {
        bool skipBound = false;
        foreach (var bound in bounds) {
          if (bound.Contains(renderer.bounds.center)) {
            skipBound = true;
            break;
          }
        }
        if (!skipBound) {
          bounds.Add(renderer.bounds);
        }
      }
    }

    terrainProcessor.ClearTrees(bounds);
    terrainProcessor.FlattenTerrain(bounds);
    // terrainProcessor.PaintHoles(bounds);
  }

  private void ProcessModelV2() {
    List<Renderer> allChildrenRenderers = GetComponentsInChildren<Renderer>().ToList();

    Vector3 sceneCenter = Vector3.zero;
    foreach (Renderer childRenderer in allChildrenRenderers) {
      // AttachColliders(childRenderer.gameObject);
      sceneCenter += childRenderer.bounds.center;
    }
    sceneCenter /= allChildrenRenderers.Count;
    sceneBounds = CalculateSceneBounds(allChildrenRenderers, sceneCenter);
    terrainProcessor.ClearTreesV2(sceneBounds);
    terrainProcessor.FlattenTerrainV2(sceneBounds);
    // terrainProcessor.PaintHoles(bounds);
  }

  private Bounds CalculateSceneBounds(List<Renderer> allChildrenRenderers, Vector3 center) {
    Bounds newBounds = new Bounds(center, Vector3.zero);
    foreach (Renderer childRenderer in allChildrenRenderers) {
      newBounds.Encapsulate(childRenderer.bounds);
    }
    return newBounds;
  }

  private void AttachColliders(GameObject childObject) {
    if (childObject.TryGetComponent<MeshFilter>(out MeshFilter meshFilter) && meshFilter.mesh.vertexCount > 3) {
      MeshCollider meshCollider = childObject.AddComponent<MeshCollider>();
      meshCollider.sharedMesh = meshFilter.mesh;
    }
  }

  private void OnDestroy() {
    // terrainProcessor.ResetTerrain();
  }
}
