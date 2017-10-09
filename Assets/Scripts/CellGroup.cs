using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGroup {
  internal Cell Cell { get; private set; }
  internal Cell.Color Color { get { return Cell.color; } private set { } }
  // col, width are for the leftmost bottom cell in a group
  internal int col;
  internal int row;
  internal int width = 2;
  internal int height = 2;

  public CellGroup(Cell cell, int col, int row) {
    this.Cell = cell;
    this.col = col;
    this.row = row;
  }

  public CellGroup(Cell cell, Point pos) {
    this.Cell = cell;
    this.col = pos.col;
    this.row = pos.row;
  }
}
