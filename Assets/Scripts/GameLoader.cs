using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour {
  public GameObject gameController;

	void Awake () {
    if (Game.instance == null) Instantiate(gameController);
	}
}
