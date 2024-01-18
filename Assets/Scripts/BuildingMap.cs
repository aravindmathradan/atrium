using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class Building {
  public int id = -1;
  public string uniqueCode;
  public string name;
  public string path;
  public string url;
  public bool isValid = true;
}

public class Buildings {
  public Building[] buildings;
}

[System.Serializable]
public class BuildingMap {
  private static Dictionary<string, Building> buildingMap;
  private static Building currentBuilding;

  public void Initialize(string jsonString) {
    Buildings buildings = JsonUtility.FromJson<Buildings>(jsonString);
    buildingMap = new Dictionary<string, Building>();
    foreach (Building building in buildings.buildings) {
      buildingMap.Add(building.uniqueCode, building);
    }
  }

  public Building GetCurrentBuilding() {
    if (currentBuilding is not null && currentBuilding.id > -1) {
      return currentBuilding;
    }
    Building invalidBuilding = new Building();
    invalidBuilding.isValid = false;
    return invalidBuilding;
  }

  public void SetCurrentBuilding(string uniqueCode, string downloadUrl) {
    currentBuilding = buildingMap[uniqueCode];
    currentBuilding.url = downloadUrl;
  }

  public Dictionary<string, Building> GetBuildingMap() {
    return buildingMap;
  }
}
