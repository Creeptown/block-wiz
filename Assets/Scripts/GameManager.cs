using System.Collections.Generic;
using UnityEngine;

/*
 * Game is responsible for passing information between boards
 * and generating pieces for all boards
 */
public class GameManager : MonoBehaviour {
  public static GameManager instance = null;
  [Tooltip("Whether Diamonds can spawn during this game")]
  public bool allowDiamond = true;
  [Tooltip("Random seed for generating cell sequence. Leave at zero to generate")]
  public static int seed = 0;
  [Tooltip("Number of cells to use when generating the player's piece")]
  public int size = 2;
  [Tooltip("Number of players in this game")]
  public int playerCount = 1;
  [Tooltip("Extra space between boards")]
  public int boardPadding = 10;
  [Tooltip("Board Prefab to use")]
  public GameObject boardPrefab;

  List<GameObject> boards = new List<GameObject>();
  List<Cell[]> spawned = new List<Cell[]>();
  CellType[] spawnableCellTypes = new CellType[2] { CellType.Normal, CellType.Bomb };

  void Awake() {
    if (instance == null) {
      instance = this;
    } else if (instance != this) {
      Destroy(gameObject);
    }

    if (seed == 0) seed = (int)System.DateTime.Now.Ticks;
    Random.InitState(seed);
    DontDestroyOnLoad(gameObject);

    for (int i = 0; i < playerCount; i++) {
      var board = Instantiate(boardPrefab, Vector3.zero, Quaternion.identity) as GameObject;
      board.GetComponent<Board>().GameManager = GetComponent<GameManager>();
      boards.Add(board);
    }
  }

  // TODO Probably can do some housekeeping to clear out older round spawns
  public Cell[] RequestCellsForRound(int round) {
    if (spawned.Count > round && spawned[round] != null) return spawned[round];
    CellColor color;
    CellType type;
    var cells = new Cell[size];

    for (int i = 0; i < size; i++) {
      type = spawnableCellTypes[Random.Range(0, spawnableCellTypes.Length)];
      color = (CellColor)Random.Range(0, System.Enum.GetValues(typeof(CellColor)).Length);
      cells[i] = new Cell(color, type, round);
    }

    spawned.Add(cells);

    return cells;
  }
}