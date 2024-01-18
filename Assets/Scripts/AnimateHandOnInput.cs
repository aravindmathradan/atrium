using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour {
  [SerializeField] private InputActionProperty pinchAnimationAction;
  [SerializeField] private InputActionProperty gripAnimationAction;
  [SerializeField] private Animator handAnimator;

  private void Start() {

  }

  private void Update() {
    float triggerValue = pinchAnimationAction.action.ReadValue<float>();
    float gripValue = gripAnimationAction.action.ReadValue<float>();
    handAnimator.SetFloat("Trigger", triggerValue);
    handAnimator.SetFloat("Grip", gripValue);
  }
}
