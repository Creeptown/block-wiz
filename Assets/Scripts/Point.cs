public class Point {
  public int row { get; private set; }
  public int col { get; private set; }

  public Point(int row, int col) {
    this.row = row;
    this.col = col;
  }

  public override string ToString() {
    return "[Point] row: " + row + ", col: " + col;
  }
}
