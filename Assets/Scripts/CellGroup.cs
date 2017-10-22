public class CellGroup {
  public Cell Cell { get; private set; }
  public CellColor Color { get { return Cell.Color; } private set { } }
  public CellType Type { get { return Cell.Type; } private set { } }
  public int Column { get; set; } // column, width identify the leftmost bottom cell in a group
  public int Row { get; set; }
  public int Width { get; set; }
  public int Height { get; set; }
  public Point Position { get { return new Point(Row, Column); }  }

  public CellGroup(Cell cell, int col, int row) {
    this.Cell = cell;
    this.Column = col;
    this.Row = row;
    this.Width = 2;
    this.Height = 2;
  }

  public CellGroup(Cell cell, Point pos) {
    this.Cell = cell;
    this.Column = pos.Col;
    this.Row = pos.Row;
    this.Width = 2;
    this.Height = 2;
  }

  public override string ToString() {
    return "[CellGroup] Color: " + Color + ", Type:" + Type + " , Width:,"+ Width +" Height:" + Height;
  }
}
