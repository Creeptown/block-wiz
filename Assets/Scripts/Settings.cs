using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour {
  public int numPlayers = 2;

  public static void GetInstance() {

  }

  void Awake() {
    DontDestroyOnLoad(this.gameObject);
  }

  public void StartGame() {
    Debug.Log("Starting with " + numPlayers.ToString() + " players");
    SceneManager.LoadScene("Main");
  }

  public void Toggle2v2(bool enabled) {
    if (enabled) {
      numPlayers = 2;
    }
  }

  public void Toggle3v3(bool enabled) {
    if (enabled) {
      numPlayers = 3;
    }
  }

  public void Toggle4v4(bool enabled) {
    if (enabled) {
      numPlayers = 4;
    }
  }
}
