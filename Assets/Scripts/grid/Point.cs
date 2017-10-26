public class Point {
  public int Row { get; private set; }
  public int Col { get; private set; }

  public Point(int row, int col) {
    this.Row = row;
    this.Col = col;
  }

  public override string ToString() {
    return "[Point] row: " + Row + ", col: " + Col;
  }
}
