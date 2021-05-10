using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class cameraManager : MonoBehaviour
{
  public Camera globalCamera;
  public Camera boidCamera;
  public Camera predatorCamera;

  void Start() {
    OnCamera1();
  }

  private void OnCamera1() {
    globalCamera.enabled = true;
    boidCamera.enabled = false;
    predatorCamera.enabled = false;
  }
  private void OnCamera2() {
    globalCamera.enabled = false;
    boidCamera.enabled = true;
    predatorCamera.enabled = false;
  }
  private void OnCamera3() {
    globalCamera.enabled = false;
    boidCamera.enabled = false;
    predatorCamera.enabled = true;
  }
}
