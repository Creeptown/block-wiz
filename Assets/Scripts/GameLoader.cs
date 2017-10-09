using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour {
  public GameObject gameManager;

  void Awake() {
    Settings settings = GetComponent<Settings>();

    if (GameManager.instance == null) {
      gameManager = Instantiate(gameManager);

      if (settings != null) {
        gameManager.GetComponent<GameManager>().playerCount = settings.numPlayers;
        Debug.Log("Loading game with " + settings.numPlayers + " players");
      }
    }
  }
}
