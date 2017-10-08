using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Board is responsible for controlling CellGroups
//
// The sequence of a given round goes like this:
// 1) If there is a pending counter drop, drop it on the player
// 2) Spawn player controlled Falling CellGroup
// 3) Once player controlled group is fixed resolve board positions (pop gems)
// 4) Set CellGroups that are now able to fall to fall
// 5) Repeat 3,4 until // all CellGroups are fixed then:
//   - calculate damage
//   - Reduce counter gem count by one
//   - Update Score
public class Board : MonoBehaviour {
  public enum Gravity { Up, Down, None }

  public Controller controller;
  [Tooltip("Top edge of the board")]
  public float top = 0;
  [Tooltip("Left edge of the board")]
  public float left = 0;
  [Tooltip("Total columns that make up the board")]
  public int columns = 6;
  [Tooltip("Total rows that make up the board")]
  public int rows = 12;
  [Tooltip("Width/Height of an individual cell")]
  public int cellSize = 20;
  [Tooltip("Column that CellGroups spawn from")]
  public int startColumn = 4;
  [Tooltip("Normal cells this board can spawn")]
  public Cell[] normalCells;
  // TODO Need to handle cells that can spawn due to particular conditions (e.g. diamond cell at every 25th round)
  [Tooltip("Speed in which cellgroup drops normally")]
  public float normalFallSpeed = 20f;
  [Tooltip("Speed in which cellgroup drops when player accelerates the drop")]
  public float acceleratedFallSpeed = 40f;
  [Tooltip("Direction CellGroups should move")]
  public Gravity gravity = Gravity.Down;

  private Game game;

  // Current round - is independent of other boards
  private int round = 0;
  // Total score for this board
  private int score = 0;
  // Track each gem clear separately to tally combos
  private List<int> clearedThisRound;
  // Speed in which cellgroup drops normally
  private float normalSpeed;
  // Speed in which cellgroup drops when player accelerates the drop
  private float acceleratedSpeed;
  // Parent SpawnedGroups to this locator to keep the scene clean
  private Transform boardHolder;
  // Active (read: player controllable cell groups)
  private CellGroup[] active;
  // Inactive (non-controllable cell groups)
  private CellGroup[,] landed;

  public static Board Initialize(Game game) {
    // copy initial values from game
    // save reference to game object
    return null;
  }

	void Start () {
    normalSpeed = normalFallSpeed;
    acceleratedSpeed = acceleratedFallSpeed;
	}
	
	void Update () {
	}

  void InitializeBoard() {
    landed = new CellGroup[rows, columns];
    boardHolder = new GameObject("Board").transform;
    // instance.transform.SetParent(boardHolder);
  }

  void SpawnActiveCellGroup() {
    Game.GenerateCellGroup(round);
    return;
  }

  void AccelerateActive() {
    for (int i = 0; i < active.Length; i++) {
      if (CanMoveDown(active[i])) {
      }
    }
  }

  void RotateActiveLeft() {
    for (int i = 0; i < active.Length; i++) {
      if (CanRotateLeft(active[i])) {
      }
    }
  }

  void RotateActiveRight() {
    for (int i = 0; i < active.Length; i++) {
      if (CanRotateRight(active[i])) {
      }
    }
  }

  bool CanSpawnGroup() {
    return true;
  }

  bool CanRotateLeft(CellGroup group) {
    return true;
  }

  bool CanRotateRight(CellGroup group) {
    return true;
  }

  bool CanMoveDown(CellGroup group) {
    return true;
  }

  IEnumerator Fall() {
    while (true) {
      yield return 0;
    }
  }

}
