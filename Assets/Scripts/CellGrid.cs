using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * Maintains the internal state of the game
 * Cells are always placed in the grid at their final position
 * Primarily used for collision detection

 * We need to mark every cell that is being used, it's computationally cheaper and less complex to mark
 * cells rather than specify a width/height and a point of origin
 * 
 * We don't need to keep a reference of cells because it's very easy to grab it from the grid, that way
 * we aren't maintaining the data in two places
 */
public class CellGrid {
  Cell[,] grid;
  HashSet<CellGroup> groups = new HashSet<CellGroup>(new CellGroupEqualityComparer());
  HashSet<Cell> cells = new HashSet<Cell>();
  int columnCount;
  int rowCount;

  public CellGrid(int row, int col) {
    rowCount = row;
    columnCount = col;
    grid = new Cell[rowCount, columnCount];
  }

  // Cells are always added from the top
  public bool AddCell(Cell cell, int col) {
    var pos = new Point(TargetRow(col, 0), col);
    if (grid[pos.Row, pos.Col] == null) {
      grid[pos.Row, pos.Col] = cell;
      cells.Add(cell);
      cell.Position = pos;
      Update();
      return true;
    }
    return false;
  }

  public Cell CellAt(int row, int col) {
    return grid[row, col];
  }

  public bool CellExists(Cell cell) {
    return cells.Contains(cell);
  }

  public bool GroupExists(CellGroup grp) {
    return groups.Contains(grp);
  }

  public void DestroyConnected() {
    CellsToDestroy().ForEach(c => {
      // Needed in case it gets destroyed in a prev loop
      // TODO Change to hashset
      if (c != null) {
        cells.Remove(c);
        grid[c.Position.Row, c.Position.Col] = null;
        if (c.InGroup) {
          groups.Remove(c.Group);
        }
      }
    });
    Update();
  }

  // Find all the connected cells we can currently destroy
  // TODOD Currently possible to add dupes here
  List<Cell> CellsToDestroy() {
    int row;
    Cell cell;
    var connected = new List<Cell>();
    for (row = rowCount - 1; row >= 0; row--) {
      for (int col = 0; col < columnCount; col++) {
        cell = grid[row, col];
        if (cell != null && cell.Type == CellType.Bomb) {
          var chain = FindConnected(row, col);
          if (chain.Count > 1) {
            connected.AddRange(chain);
          }
        }
      }
    }
    return connected;
  }

  // Given a column and starting row determines the next open row
  int TargetRow(int col, int row) {
    for (int i = rowCount - 1; i > row; i--) {
      if (i >= rowCount || col >= columnCount) Debug.Log("col: "+col+", row:" + i);
      if (grid[i, col] == null) return i;
    }
    return -1;
  }

  // Given a column and starting row determines the highest open row
  // in a series of columns. Used for determing the position of cells within a group
  int HighestTargetRow(int[] cols, int row) {
    var rows = new int[cols.Length];
    for (int i = 0; i < cols.Length; i++) {
      rows[i] = TargetRow(cols[i], row);
    }
    Array.Sort(rows);
    return rows[rows.Length - 1];
  }

  bool Detect2x2() {
    bool changed = false;
    CellGroup group;
    for (int row = rowCount - 1; row > 0; row--) {
      for (int col = 0; col < columnCount - 1; col++) {
        if (grid[row, col] != null &&
          grid[row, col + 1] != null && // right
          grid[row - 1, col] != null && // top
          grid[row - 1, col + 1] != null && // right+top
          grid[row, col].Color == grid[row, col + 1].Color &&
          grid[row, col].Color == grid[row - 1, col].Color &&
          grid[row, col].Color == grid[row - 1, col + 1].Color &&
          !grid[row, col].InGroup &&
          !grid[row, col + 1].InGroup &&
          !grid[row - 1, col].InGroup &&
          !grid[row - 1, col + 1].InGroup &&
          grid[row, col].Type == CellType.Normal &&
          grid[row, col + 1].Type == CellType.Normal &&
          grid[row - 1, col].Type == CellType.Normal &&
          grid[row - 1, col + 1].Type == CellType.Normal) {
          group = new CellGroup(grid[row, col], col, row);
          if (groups.Add(group)) {
            // This is finding the bottomLeft
            grid[row, col].Group = group;
            grid[row, col + 1].Group = group;
            grid[row - 1, col].Group = group;
            grid[row - 1, col + 1].Group = group;
            changed = true;
          }
        }
      }
    }
    return changed;
  }

  bool ExpandCellGroups() {
    bool changed = false;
    foreach (CellGroup grp in groups) {
      bool expandUp = true;
      bool expandDown = true;
      bool expandRight = true;
      bool expandLeft = true;

      for (int i = 0; i < grp.Width; i++) {
        if (!CheckValid(grp.Row - grp.Height, grp.Column + i) ||
          grid[grp.Row - grp.Height, grp.Column + i] == null ||
          grid[grp.Row - grp.Height, grp.Column + i].Color != grp.Color ||
          grid[grp.Row - grp.Height, grp.Column + i].InGroup ||
          grid[grp.Row - grp.Height, grp.Column + i].Type != CellType.Normal) {
          expandUp = false;
        }
        if (!CheckValid(grp.Row + 1, grp.Column + i) ||
          grid[grp.Row + 1, grp.Column + i] == null ||
          grid[grp.Row + 1, grp.Column + i].Color != grp.Color ||
          grid[grp.Row + 1, grp.Column + i].InGroup ||
          grid[grp.Row + 1, grp.Column + i].Type != CellType.Normal) {
          expandDown = false;
        }
      }
      if (expandUp) {
        for (int i = 0; i < grp.Width; i++) {
          grid[grp.Row - grp.Height, grp.Column + i].Group = grp;
        }
        grp.Height++;
      }
      if (expandDown) {
        for (int i = 0; i < grp.Width; i++) {
          grid[grp.Row + 1, grp.Column + i].Group = grp;
        }
        grp.Row++;
        grp.Height++;
      }

      for (int i = 0; i < grp.Height; i++) {
        if (!CheckValid(grp.Row - i, grp.Column + grp.Width) ||
          grid[grp.Row - i, grp.Column + grp.Width] == null ||
          grid[grp.Row - i, grp.Column + grp.Width].Color != grp.Color ||
          grid[grp.Row - i, grp.Column + grp.Width].InGroup ||
          grid[grp.Row - i, grp.Column + grp.Width].Type != CellType.Normal) {
          expandRight = false;
        }
        if (!CheckValid(grp.Row - i, grp.Column - 1) ||
          grid[grp.Row - i, grp.Column - 1] == null ||
          grid[grp.Row - i, grp.Column - 1].Color != grp.Color ||
          grid[grp.Row - i, grp.Column - 1].InGroup ||
          grid[grp.Row - i, grp.Column - 1].Type != CellType.Normal) {
          expandLeft = false;
        }
      }
      if (expandRight) {
        for (int i = 0; i < grp.Height; i++) {
          grid[grp.Row - i, grp.Column + grp.Width].Group = grp;
        }
        grp.Width++;
      }
      if (expandLeft) {
        for (int i = 0; i < grp.Height; i++) {
          grid[grp.Row - i, grp.Column - 1].Group = grp;
        }
        grp.Column--;
        grp.Width++;
      }
      changed = expandUp || expandDown || expandLeft || expandRight;
    }
    return changed;
  }

  bool CombineCellGroups() {
    bool changed = false;
    List<CellGroup> toRemove = new List<CellGroup>();
    foreach (CellGroup g1 in groups) {
      foreach (CellGroup g2 in groups) {
        if (g1.Color == g2.Color && g1.Column == g2.Column && g1.Row - g1.Height == g2.Row && g1.Width == g2.Width) {
          toRemove.Add(g2);
          g1.Height += g2.Height;
        }
        if (g1.Color == g2.Color && g1.Row == g2.Row && g1.Column + g1.Width == g2.Column && g1.Height == g2.Height && g1.Color == g2.Color) {
          toRemove.Add(g2);
          g1.Width += g2.Width;
        }
      }
    }
    foreach (CellGroup p in toRemove) {
      groups.Remove(p);
      changed = true;
    }
    return changed;
  }

  bool CheckValid(int row, int col) {
    return (col >= 0 && col < columnCount && row >= 0 && row < rowCount);
  }

  void Update() {
    UpdateCellPositions();
    Detect2x2();
    ExpandCellGroups();
    CombineCellGroups();
  }

  // Iterate over all the cells and determine if we can move a cell
  // down the grid
  void UpdateCellPositions() {
    Cell c;

    for (int col = 0; col < columnCount - 1; col++) {
      for (int row = rowCount - 2; row > 0; row--) {
        c = grid[row, col];
        if (c == null) continue;
        if (c.Group != null) {
          UpdateCellGroupPosition(c, col, row);
        } else {
          UpdateCellPosition(c, col, row);
        }
      }
    }
  }

  void UpdateCellPosition(Cell c, int col, int row) {
    Point pos = new Point(TargetRow(col, row), col);
    if (pos.Row > -1 && pos != c.Position) {
      grid[row, col] = null;
      grid[pos.Row, pos.Col] = c;
      c.Position = pos;
    }
  }

  // Find the the highest point of contact and use that for
  // every cell in the group
  void UpdateCellGroupPosition(Cell c, int col, int row) {
    var grp = c.Group;
    int[] cols = new int[grp.Width];
    for (int i = 0; i < grp.Width; i++) cols[i] = grp.Column + i;

    Point pos = new Point(HighestTargetRow(cols, row), col);

    if (pos != c.Position && pos.Row > -1 && pos.Col > -1) {
      grid[row, col] = null;
      grid[pos.Row, pos.Col] = c;
      c.Position = pos;
    }
  }

  List<Cell> FindConnected(int row, int col) {
    Cell initial = grid[row, col];
    var alreadySeen = new bool[grid.GetLength(0), grid.GetLength(1)];
    var queue = new Queue<KeyValuePair<int, int>>();
    var found = new List<Cell>();

    if (initial == null) return found;

    queue.Enqueue(new KeyValuePair<int, int>(col, row));

    while (queue.Any()) {
      KeyValuePair<int, int> point = queue.Dequeue();
      var cell = grid[point.Value, point.Key];

      if (cell == null || (cell.Color != initial.Color))
        continue;

      if (alreadySeen[point.Value, point.Key])
        continue;

      alreadySeen[point.Value, point.Key] = true;

      found.Add(grid[point.Value, point.Key]);

      EnqueueIfMatches(grid, queue, point.Key - 1, point.Value, initial);
      EnqueueIfMatches(grid, queue, point.Key + 1, point.Value, initial);
      EnqueueIfMatches(grid, queue, point.Key, point.Value - 1, initial);
      EnqueueIfMatches(grid, queue, point.Key, point.Value + 1, initial);
    }

    return found;
  }

  void EnqueueIfMatches(Cell[,] array, Queue<KeyValuePair<int, int>> queue, int col, int row, Cell initial) {
    if (col < 0 || col >= array.GetLength(1) || row < 0 || row >= array.GetLength(0)) return;

    var cell = array[row, col];
    if (cell != null && cell.Type != CellType.Counter && cell.Color == initial.Color) {
      queue.Enqueue(new KeyValuePair<int, int>(col, row));
    }
  }

  public override string ToString() {
    var ret = "";
    Cell cell;
    for (int j = 0; j < rowCount; j++) {
      for (int i = 0; i < columnCount; i++) {
        cell = grid[j, i];
        ret += cell == null ? "_" : (cell.InGroup ? "g" : "x");
      }
      ret += System.Environment.NewLine;
    }
    return ret;
  }
}
