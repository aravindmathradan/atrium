using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModelSelection : MonoBehaviour {
  private BuildingMap buildingMap;

  // Prod
  private string buildingMapUrl = "https://atrium-server.azurewebsites.net/structures/";

  // Test
  // private string buildingMapUrl = "https://atrium-server.azurewebsites.net/teststructures/";

  public void Start() {
    buildingMap = new BuildingMap();
    StartCoroutine(LoadMapFromServer(buildingMapUrl));
  }

  private IEnumerator LoadMapFromServer(string url) {
    UnityWebRequest request = UnityWebRequest.Get(url);
    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success) {
      Debug.Log(request.error);
      yield break;
    }

    buildingMap.Initialize(request.downloadHandler.text);
    Button[] buttons = gameObject.GetComponentsInChildren<Button>();
    foreach (Button btn in buttons) {
      if (btn.name != "EnterButton" && btn.name != "ExitButton") {
        btn.interactable = true;
      }
    }
  }

  private IEnumerator GetDownloadURL(string url, System.Action<string> callback) {
    UnityWebRequest request = UnityWebRequest.Get(url);
    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success) {
      Debug.Log(request.error);
      yield break;
    }
    callback(request.downloadHandler.text);
  }

  private void LoadCurrentModel(string structureName) {
    // Prod
    string requestUrl = buildingMapUrl + buildingMap.GetBuildingMap()[structureName].path;
    StartCoroutine(GetDownloadURL(requestUrl, (downloadUrl) => {
      buildingMap.SetCurrentBuilding(structureName, downloadUrl);
      GameObject.Find("EnterButton").GetComponent<Button>().interactable = true;
    }));

    // Test
    // string requestUrl = buildingMap.GetBuildingMap()[structureName].path;
    // Debug.Log(requestUrl);
    // buildingMap.SetCurrentBuilding(structureName, requestUrl);
    // GameObject.Find("EnterButton").GetComponent<Button>().interactable = true;
  }

  public void EnterVR() {
    SceneManager.LoadScene("World");
  }

  public void InteriorButtonClick() {
    LoadCurrentModel("interior");
  }

  public void House1ButtonClick() {
    LoadCurrentModel("house1");
  }

  public void House2ButtonClick() {
    LoadCurrentModel("house2");
  }

  public void BunglowButtonClick() {
    LoadCurrentModel("bunglow");
  }

  public void RoomButtonClick() {
    LoadCurrentModel("room");
  }

  public void ExitButtonClick() {
    Application.Quit();
  }
}
