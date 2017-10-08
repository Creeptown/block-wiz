using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game is responsible for passing information between boards
// and generating pieces for all boards
public class Game : MonoBehaviour {
  public static Game instance = null;
  public static int seed = 0;
  private static List<CellGroup> spawned = new List<CellGroup>();

  public int playerCount;
  public Board boardScript;

  // Either we need a way of idempotently generating
  // a group based on the round and seed or we need to store
  // previously generated results
  // IDEA: canSpawn determines whether not we can spawn, so
  // while we have less than 2 blocks we keep randomly generating
  // until we have two that canSpawn - however that wouldn't 
  // make it 
  public static CellGroup GenerateCellGroup(int round) {
    if (spawned[round] != null) return spawned[round];
    // Generate new group and add it to the spawned
    return null;
  }

  void Awake() {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
    }
    if (seed == 0) seed = (int)System.DateTime.Now.Ticks;
    DontDestroyOnLoad(gameObject);
    boardScript = GetComponent<Board>();
    // spawn boards
  }
	
	// Update is called once per frame
	void Update () {
	}
}
