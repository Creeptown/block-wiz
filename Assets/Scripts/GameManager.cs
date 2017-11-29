using System.Collections.Generic;
using UnityEngine;

/*
 * Game is responsible for passing information between boards
 * and generating pieces for all boards
 */
public class GameManager : MonoBehaviour {
  public static GameManager instance = null;
  public static float zOffset = -0.1f;

  [Tooltip("Whether Diamonds can spawn during this game")]
  public bool allowDiamond = true;
  [Tooltip("Random seed for generating cell sequence. Leave at zero to generate")]
  public static int seed = 0;
  [Tooltip("Number of cells to use when generating the player's piece")]
  public int pieceCount = 2;
  [Tooltip("Number of players in this game")]
  public int playerCount = 1;
  [Tooltip("Sprite Pixels Per Unit")]
  public int PPU = 100;
  [Tooltip("Board Prefab to use")]
  public GameObject boardPrefab;

  List<Board> boards = new List<Board>();
  List<CellSpawn[]> spawned = new List<CellSpawn[]>();
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
    DistributeBoards();
  }

  void DistributeBoards() {
    // Get width of screen
    Vector3 leftBottom = Camera.main.ViewportToWorldPoint(Vector3.zero);
    Vector3 rightTop = Camera.main.ViewportToWorldPoint(Vector3.one);
    Vector3 pos = Camera.main.ViewportToWorldPoint(Vector3.one * 0.5f);

    // Calculate spacing
    float space = (Mathf.Abs(rightTop.x) + Mathf.Abs(leftBottom.x)) / playerCount;

    // Calculate position of first board
    pos.x -= space * (playerCount - 1) / 2f;

    for (int i = 0; i < playerCount; i++) {
      var board = Instantiate(boardPrefab, pos, Quaternion.identity) as GameObject;
      var boardComponent = board.GetComponent<Board>();
      boardComponent.GameManager = GetComponent<GameManager>();
      boardComponent.boardIndex = i;
      boards.Add(boardComponent);
      pos.x += space;
    }
  }

  public void Attack(int[] boardIndexes, List<CellSpawn> cellsToDrop) {
    for (int i = 0; i < boardIndexes.Length; i++) {
      var board = boards[i];
      board.ReceiveAttack(cellsToDrop);
    }
  }

  // TODO Probably can do some housekeeping to clear out older round spawns
  public CellSpawn[] RequestCellsForRound(int round) {
    if (spawned.Count > round && spawned[round] != null) return spawned[round];
    CellColor color;
    CellType type;
    var cells = new CellSpawn[pieceCount];

    for (int i = 0; i < pieceCount; i++) {
      type = spawnableCellTypes[Random.Range(0, spawnableCellTypes.Length)];
      color = (CellColor)Random.Range(0, System.Enum.GetValues(typeof(CellColor)).Length);
      cells[i] = new CellSpawn(color, type, round);
    }

    spawned.Add(cells);

    return cells;
  }
}