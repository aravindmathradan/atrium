using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldMenuInputHandler : MonoBehaviour {
  [SerializeField] private GameObject worldMenu;
  public void MainMenuButtonClick() {
    SceneManager.LoadScene("MainMenu");
  }

  public void BackButtonClick() {
    worldMenu.SetActive(false);
  }

  public void ExitButtonClick() {
    Application.Quit();
  }
}
