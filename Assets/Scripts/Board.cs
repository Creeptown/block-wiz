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

  [Tooltip("Total columns that make up the board")]
  public int columnCount = 6;
  [Tooltip("Total rows that make up the board")]
  public int rowCount = 12;
  [Tooltip("Pixel dimensions of an individual cell")]
  public int cellSize = 20;
  [Tooltip("Space between cells")]
  public int cellPadding = 2;
  [Tooltip("Column that CellGroups spawn from (currently ignored)")]
  public int startColumn = 4;
  [Tooltip("Speed in which cellgroup drops normally")]
  public float normalSpeed = 20f;
  [Tooltip("Speed in which cellgroup drops when player accelerated")]
  public float dropSpeed = 40f;
  [Tooltip("Speed in which fixed cellgroups fall")]
  public float fallSpeed = 50f;
  [Tooltip("Length in seconds cells can touch before they become fixed")]
  public float touchTime = 0.2f;
  [Tooltip("Time to wait between rotations")]
  public float rotationDelay = 0.1f;
  [Tooltip("Time to wait between horizontal moves")]
  public float moveDelay = 0.2f;
  [Tooltip("Direction CellGroups should move")]
  public Gravity gravity = Gravity.Down;
  [Tooltip("Cell Renderer Prefab to use")]
  public GameObject cellRendererPrefab;
  [Tooltip("Background Sprite")]
  public Sprite backgroundSprite;

  public GameManager GameManager { get; set; }
  public BoardState State { get; private set; }
  public int BoardIndex = -1;

  int round = 0; // Current round - independent of other boards
  int score = 0; // Total score for this board
  List<int> clearedThisRound; // Track each gem clear separately to tally combos
  float speed; // Current speed in which cellgroup is dropping
  List<GameObject> falling = new List<GameObject>(); // Currently falling cells
  List<GameObject> alive = new List<GameObject>(); // All cells
  CellGrid cellGrid;
  List<CellSpawn> pendingCounter = new List<CellSpawn>(); // Whether or not we have pending counters to drop
  SpriteRenderer spriteRenderer;

  void Awake() {
    State = BoardState.RoundStart;
    gameObject.AddComponent<SpriteRenderer>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = backgroundSprite;
  }

	void Start() {
    InitializeGrid();
    speed = normalSpeed;
    StartCoroutine("Fall");
  }
	
  void InitializeGrid() {
    cellGrid = new CellGrid(rowCount, columnCount);
  }

  // Main Loop
  IEnumerator Fall() {
    while (State != BoardState.Inactive && State != BoardState.Won && State != BoardState.Lost) {
      switch (State) {
        case BoardState.RoundStart:
          if (pendingCounter.Count > 0) {
            State = BoardState.Countering;
          } else {
            SpawnPlayerCells();
            State = BoardState.Playing;
          }
          break;
        case BoardState.Countering:
          State = BoardState.Playing;
          // Do counter - basically just add a bunch of counter gems to the falling list
          pendingCounter.Clear();
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

  // Spawns a column of Cells in a random column at the top of the board
  void SpawnPlayerCells() {
    var col = Random.Range(0, columnCount);
    var cells = GameManager.RequestCellsForRound(round);

    for (int i = cells.Length - 1; i > -1; i--) {
      CreateRenderer(cells[i], i - cells.Length, col);
    }

    //Debug.Log(cellGrid.ToString());
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
    if (falling.Count == 0) cellGrid.OnFixed();
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

  void CreateRenderer(CellSpawn spawn, int row, int col) {
    var cell = new Cell(spawn);

    // If we can't add any more cells we've reached the game over state
    if (!cellGrid.AddCell(cell, col)) {
      State = BoardState.Lost;
      return;
    }
    
    var pos = GridToWorldSpace(row, col);
    // TODO Probably should be in the renderer's Init method
    var obj = Instantiate(cellRendererPrefab, pos, Quaternion.identity) as GameObject;
    var renderer = obj.GetComponent<CellRenderer>();

    obj.transform.parent = transform;
    renderer.Initialize(cell, this);
    alive.Add(obj);
    falling.Add(obj);
  }

  internal Vector3 GridToWorldSpace(Point p) {
    return GridToWorldSpace(p.Row, p.Col);
  }

  // Coverts a grid position (row, column) to world space coordinate (Vector3)
  internal Vector3 GridToWorldSpace(float row, float col) {
    row = gravity == Gravity.Down ? rowCount - (row + 1) : row;
    var gridUnits = GridToWorldUnit();
    var parentWidth = (columnCount - 1) * gridUnits;
    var parentHeight = (rowCount - 1) * gridUnits;
    var x = (transform.position.x - parentWidth / 2) + (col * gridUnits);
    var y = (transform.position.y - parentHeight / 2) + (row * gridUnits);
    return new Vector3(x, y, transform.position.z + GameManager.zOffset);
  }

  internal float GridToWorldUnit() {
    return ((cellSize + cellPadding) / (float)GameManager.PPU);
  }

  int WorldYtoGridRow(GameObject o) {
    var pos = o.transform.position;
    return Mathf.FloorToInt(pos.y / GridToWorldUnit()) * (int)gravity;
  }

  #endregion Grid

  #region Control
  
  // Note: If the state is Playing then any falling pieces by nature are player controllable
  internal WaitForSeconds Drop(bool dropping) {
    if (State == BoardState.Playing) {
      speed = dropping ? dropSpeed : normalSpeed;
    }
    return new WaitForSeconds(0);
  }

  // Starting from the lowest cell determine if each cell is capable of
  // being shifted horizontally based on it's current row and colujn. 
  // If true, update the grid and then update the renderer
  internal WaitForSeconds MoveHorizontal(float delta) {
    bool canMove = false;
    int dir = delta < 0 ? -1 : 1;
    List<Cell> cells;

    if (State == BoardState.Playing) {
      // Preflight test - check if the neighbooring cell is empty
      cells = falling.ConvertAll(o => o.GetComponent<CellRenderer>().Cell);

      canMove = falling.TrueForAll(o => {
        var row = WorldYtoGridRow(o);
        return cellGrid.IsEmpty(row, o.GetComponent<CellRenderer>().Cell.Position.Col + dir);
      });

      if (canMove && (dir < 0 ? cellGrid.ShiftCellsLeft(cells) : cellGrid.ShiftCellsRight(cells))) {
        falling.ForEach(o => {
          var renderer = o.GetComponent<CellRenderer>();
          var pos = o.transform.position;
          o.transform.position = new Vector3(pos.x + (GridToWorldUnit() * dir), pos.y);
          renderer.UpdateTarget();
        });
      }
    }

    return new WaitForSeconds(dir != 0f && canMove ? moveDelay : 0f);
  }

  // The first block is always the rotating lever, the second block is the pivot
  // Note this is only designed to work with a 2-block piece
  internal WaitForSeconds Rotate(int rotation) {
    if (State != BoardState.Playing || falling.Count < 2) return new WaitForSeconds(0f);
    int translate = 0;
    bool clockwise = rotation < 0;
    GameObject lever = falling[0];
    GameObject pivot = falling[1];
    Cell leverCell = lever.GetComponent<CellRenderer>().Cell;
    Cell pivotCell = pivot.GetComponent<CellRenderer>().Cell;
    Point leverInGrid = new Point(WorldYtoGridRow(lever), leverCell.Position.Col);
    Point pivotInGrid = new Point(WorldYtoGridRow(pivot), pivotCell.Position.Col);
    Vector3 pivotPos = pivot.transform.position;
    Vector3 leverPos = lever.transform.position;
    bool onBottom = leverInGrid.Row > pivotInGrid.Row;
    bool onLeft = leverInGrid.Col < pivotInGrid.Col;

    // Update lever grid position
    leverInGrid = leverInGrid.Col == pivotInGrid.Col
      // Currently vertically aligned -> same row, diff col
      ? new Point(pivotInGrid.Row, pivotInGrid.Col + (clockwise && onBottom || !clockwise && !onBottom ? -1 : 1))
      // Currently horizontally aligned -> diff row, same col
      : new Point(pivotInGrid.Row + (clockwise && onLeft ? -1 : 1), pivotInGrid.Col);

    // Attempt to translate piece if we've rotated into an invalid cell (i.e. wallkick)
    if (!cellGrid.IsEmpty(leverInGrid)) {
      // determine if blocked on the left or right
      translate = leverInGrid.Col < pivotInGrid.Col ? 1 : -1;
    }

    // We don't actually need to get a point, just the order (if vertical) and the row
    leverInGrid = new Point(leverInGrid.Row, leverInGrid.Col + translate);
    pivotInGrid = new Point(pivotInGrid.Row, pivotInGrid.Col + translate);

    if (cellGrid.IsEmpty(leverInGrid) && cellGrid.IsEmpty(pivotInGrid)) {
      // Order matters, set the grid position of the lowest cell first
      if (WorldYtoGridRow(lever) > WorldYtoGridRow(pivot)) {
        cellGrid.SetRow(leverCell, leverInGrid.Col);
        cellGrid.SetRow(pivotCell, pivotInGrid.Col);
      } else {
        cellGrid.SetRow(pivotCell, pivotInGrid.Col);
        cellGrid.SetRow(leverCell, leverInGrid.Col);
      }

      // Rotate, translate renderer
      var dir = leverPos - pivotPos; // get point direction relative to pivot
      dir = Quaternion.Euler(0, 0, rotation) * dir; // rotate it
      var rotatedPos = dir + pivotPos; // calculate rotated point
      var offsetX = GridToWorldUnit() * translate;

      lever.transform.position = new Vector3(rotatedPos.x + offsetX , rotatedPos.y, 0);
      pivot.transform.position = new Vector3(pivotPos.x + offsetX, pivotPos.y, 0);

      falling.ForEach(o => {
        var renderer = o.GetComponent<CellRenderer>();
        renderer.UpdateTarget();
      });

      return new WaitForSeconds(rotationDelay);
    } 

    return new WaitForSeconds(0f);
  }

  #endregion Control

  #region Scoring and Countering
  public void SendAttack() {
    //GameManager.Attack()
  }

  public void ReceiveAttack(List<CellSpawn> cellsToSpawn) {
    pendingCounter = cellsToSpawn;
  }

  int ScoreRound() {
    return Scoring.Score();
  }

  #endregion Scoring and Countering
}
