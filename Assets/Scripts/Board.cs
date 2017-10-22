using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
  public enum Gravity { Up = 1, Down = -1 }
  public enum BoardState {
    Inactive,   // Board is inactive
    RoundStart, // New Round has begun
    Countering, // If there are pending counter blocks, drop them
    Playing,    // User is manipulating an active piece
    Grouping,   // Recalculate cellGroup membership
    Resolving,  // Player can no longer manipulate the board this round. Recursively Resolve Cell Connections
    Destroying, // Animate destroy, prevents immediate successive resolves
    Combining,  // Update CellGroups
    Scoring,    // Calculating final score and assessing Attack Damage
    RoundEnd,   // Safe to transition to Round Start
    Won,        // Player has won
    Lost,       // Player has lost
  }

  [Tooltip("Top edge of the board")]
  public float top = 0;
  [Tooltip("Left edge of the board")]
  public float left = 0;
  [Tooltip("Total columns that make up the board")]
  public int columnCount = 6;
  [Tooltip("Total rows that make up the board")]
  public int rowCount = 12;
  [Tooltip("Width/Height of an individual cell")]
  public int cellSize = 20;
  [Tooltip("Column that CellGroups spawn from")]
  public int startColumn = 4;
  [Tooltip("Speed in which cellgroup drops normally")]
  public float normalSpeed = 20f;
  [Tooltip("Speed in which cellgroup drops when player accelerated")]
  public float dropSpeed = 40f;
  [Tooltip("Speed in which fixed cellgroups fall")]
  public float fallSpeed = 50f;
  [Tooltip("Length in seconds cells can touch before they become fixed")]
  public float touchTime = 0.2f;
  [Tooltip("Time to wait between horizontal moves")]
  public float horizontalMoveDelay = 0.2f;
  [Tooltip("Direction CellGroups should move")]
  public Gravity gravity = Gravity.Down;
  [Tooltip("Cell Renderer Prefab to use")]
  public GameObject cellRendererPrefab;

  public GameManager GameManager { get; set; }
  public BoardState State { get; private set; }

  int round = 0; // Current round - independent of other boards
  int score = 0; // Total score for this board
  List<int> clearedThisRound; // Track each gem clear separately to tally combos
  float speed; // Current speed in which cellgroup is dropping
  List<GameObject> falling = new List<GameObject>(); // Currently falling cells
  List<GameObject> alive = new List<GameObject>(); // All cells
  CellGrid cellGrid;
  bool pendingCounter = false; // Whether or not we need drop counters

  void Awake() {
    State = BoardState.RoundStart;
    var pos = transform.position;
    transform.position = new Vector3(pos.x + left, pos.y + top);
  }

	void Start() {
    InitializeBoard();
    speed = normalSpeed;
    StartCoroutine("Fall");
  }
	
  void InitializeBoard() {
    cellGrid = new CellGrid(rowCount, columnCount);
  }

  // Main Loop
  IEnumerator Fall() {
    while (State != BoardState.Inactive && State != BoardState.Won && State != BoardState.Lost) {
      switch (State) {
        case BoardState.RoundStart:
          if (pendingCounter) {
            State = BoardState.Countering;
          } else {
            SpawnCellGroup();
            State = BoardState.Playing;
          }
          break;
        case BoardState.Countering:
          State = BoardState.Playing;
          break;
        case BoardState.Playing:
          MakeFixed();
          if (falling.Count == 0) { 
            State = BoardState.Resolving;
          } else {
            MoveActive();
          }
          break;
        case BoardState.Resolving:
          MakeFixed();
          if (Resolve()) {
            State = BoardState.Combining;
          }
          MoveActive();
          break;
        case BoardState.Combining:
          Combine();
          State = BoardState.Scoring;
          break;
        case BoardState.Scoring:
          score += ScoreRound();
          State = BoardState.RoundEnd;
          break;
        case BoardState.RoundEnd:
          round++;
          State = BoardState.RoundStart;
          break;
      }
      yield return 0;
    }
  }

  #region Grid

  // Spawn a random Cell in a random column at the top of the board
  void SpawnCellGroup() {
    var col = Random.Range(0, columnCount);
    var cells = GameManager.RequestCellsForRound(round);
    for (int i = 0; i < cells.Length; i++) {
      if (cellGrid.AddCell(cells[i], col)) {
        CreateRenderer(cells[i], new Point(0, col));
      }
    }
    Debug.Log(cellGrid.ToString());
  }

  // Once all cells are fixed, resolve any connections between bombs and normal
  // cells until cells are no longer fallings
  // Returns true if all connections are resolved, false if we're still resolving
  bool Resolve() {
    if (falling.Count > 0) return false;
    var toRemove = new List<GameObject>();

    cellGrid.DestroyConnected();

    alive.ForEach(o => {
      if (!cellGrid.CellExists(o.GetComponent<CellRenderer>().Cell)) toRemove.Add(o);
    });
    toRemove.ForEach(o => {
      alive.Remove(o);
      falling.Remove(o);
      o.GetComponent<CellRenderer>().Destroy();
    });

    MakeFalling();

    return falling.Count == 0;
  }

  // Move all active cell groups according to their current speed to their current targets
  void MoveActive() {
    falling.ForEach(g => {
      var target = g.GetComponent<CellRenderer>().TargetPosition;
      g.transform.position = Vector3.MoveTowards(g.transform.position, target, speed * Time.deltaTime);
    });
  }

  // Starting from the second to last row find all the cells that need to be 
  // activated due to removal of cells beneath them
  void MakeFalling() {
    CellRenderer renderer;
    alive.ForEach(obj => {
      renderer = obj.GetComponent<CellRenderer>();
      renderer.UpdateTarget();
      if (obj.transform.position != renderer.TargetPosition) {
        falling.Add(obj);
      }
    });
  }

  // Removes the CellRenderers from the active list if the cell has reached it's target
  // TODO Need to wait until the state is Landed
  void MakeFixed() {
    var toFixed = new List<GameObject>();
    falling.ForEach(o => {
      if (o.transform.position == o.GetComponent<CellRenderer>().TargetPosition) {
        toFixed.Add(o);
      }
    });
    toFixed.ForEach(obj => falling.Remove(obj));
  }

  // After we have resolved, we'll need to combine any newly created groups and
  // remove unused renderers 
  // Note: Once cells become part of a group, only the first cell in the group is 
  // responsible for rendering the entire group
  void Combine() {
    var toRemove = new List<GameObject>();
    var toRender = new List<GameObject>();
    alive.ForEach(o => {
      var r = o.GetComponent<CellRenderer>();
      if (r.Cell.InGroup) {
        // If the renderer's cell is in a group, and that group's cell
        // is not the same as the renderer, then this renderer can be removed
        // Or if the group has been removed from the board
        (r.Cell.Group.Cell != r.Cell || !cellGrid.GroupExists(r.Cell.Group) ? toRemove : toRender).Add(o);
      }
    });
    toRemove.ForEach(o => {
      alive.Remove(o);
      falling.Remove(o);
      o.GetComponent<CellRenderer>().Destroy();
    });
    toRender.ForEach(o => o.GetComponent<CellRenderer>().RenderGroup());
  }

  void CreateRenderer(Cell cell, Point p) {
    var obj = Instantiate(cellRendererPrefab, new Vector3(p.Col * cellSize, p.Row, 0), Quaternion.identity) as GameObject;
    var renderer = obj.GetComponent<CellRenderer>();
    obj.transform.parent = transform;
    renderer.Initialize(cell, cellSize, (int)gravity);
    alive.Add(obj);
    falling.Add(obj);
  }

  #endregion Grid

  #region Control

  internal WaitForSeconds Drop(bool dropping) {
    if (State == BoardState.Playing) {
      speed = dropping ? dropSpeed : normalSpeed;
    }
    return new WaitForSeconds(0);
  }

  internal WaitForSeconds MoveHorizontal(float dir) {
    bool canMove = false;
    if (State == BoardState.Playing) {
    }
    return new WaitForSeconds(dir != 0f && canMove ? horizontalMoveDelay : 0f);
  }

  internal WaitForSeconds Rotate(int dir) {
    if (State == BoardState.Playing) {
    }
    return new WaitForSeconds(0);
  }

  #endregion Control

  #region Scoring

  int ScoreRound() {
    return Scoring.Score();
  }

  #endregion Scoring
}
