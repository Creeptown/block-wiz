using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour {
  public GameObject gameManager;

  void Awake() {
    Settings settings = GetComponent<Settings>();

    if (GameManager.instance == null) {
      gameManager = Instantiate(gameManager);

      if (GameManager.instance == null) {
        Instantiate(gameManager);

        // TODO: somehow after the above line, this still doesn't have an instance
        GameManager.instance.playerCount = settings.numPlayers;

        Debug.Log("Loading game with " + settings.numPlayers + " players");
      }
    }
  }
}
