using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIHandler : MonoBehaviour {
  [SerializeField] private InputActionProperty menuToggle;
  [SerializeField] private GameObject worldMenu;
  private bool isMenuOpen = false;

  private void Update() {
    if (menuToggle.action.triggered) {
      isMenuOpen = !isMenuOpen;
      if (isMenuOpen) {
        worldMenu.SetActive(true);
      } else {
        worldMenu.SetActive(false);
      }
    }
  }
}
