using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Most of this logic is taken from a GameFaqs guide for Puzzle Fighter: 
// https://www.gamefaqs.com/ps3/939073-super-puzzle-fighter-ii-turbo-hd-remix/faqs/60766
public class Scoring {
  private static int[] Squares = new int[] { 9, 16, 25, 36, 49, 64, 81, 100, 121, 144, 169, 196, 225 };

  public static int ScoreRound(int round, List<int> scores) {
    if (scores.Count == 0) return 0;
    int score = scores.Sum();
    score += ComboBonus(scores);
    score += TimeBonus(round);
    return score;
  }

  public static int ScoreCells(int round, List<Cell> cells) {
    return TotalCellBonus(cells) + CellBonus(cells);
  }

  // 2nd attack of chain: no diamond penalty hereafter, +2 extra counter gems
  // 3rd attack: +4 counter gems
  // 4th attack: +10
  // 5th attack: +16 
  // 6th attack: +22
  // 7th attack: +28
  // 8th attack: +34
  static int ComboBonus(List<int> scores) {
    if (scores.Count == 1) return 2;
    else if (scores.Count == 3) return 4;
    else return 4 + (scores.Count - 3) * 6;
  }

  // Bonus for the total number of gems destroyed
  // Regular gems have a 1:1 ratio of destroyed to sent over. 
  // Regular gem bonus occurs whenever you hit a (10n + 1) number of gems destroyed. So, if you
  // destroy between 11 and 20 gems, you get a +1 in addition to whatever you
  // destroyed. If you destroyed 21-30 gems, it’s +2, and so forth. 
  // Attack Gems don't count towards this bonus. 
  static int TotalCellBonus(List<Cell> cells) {
    var nrml = cells.Where(c => c.Type != CellType.Bomb).ToList();
    return nrml.Count > 0 ? (nrml.Count - 1) / 10 : 0;
  }

  // Any time you destroy a perfect square power gem the multiplier is a simple 2x, relative to the 
  // number of regular gems necessary to build that power gem. 

  // The amount of damage you deliver shoots up every time your power gem
  // becomes 1 more than a perfect square number (e.g. 9, 16, 25…).
  //actual dimensions don’t factor into it, just the # of cells in
  // the power gem, bonuses kick in after every square
  // 10-16 -> 2n + 0.5(n)
  // 17-25 -> 2n + 1(n)
  // 26-36 -> 2n + 1.5(n)
  // 37-49 -> 2n + 2(n)
  // etc
  // destroying multiple gems in the same chain/move as destroying one
  // power gem of the combined size destroy two 2x2 power gems counts 
  // as destroying and 8x2 gem + some unspecified bonus
  static int CellBonus(List<Cell> cells) {
    var singleScore = cells.Where(c => !c.InGroup).Count();
    var combinedScore = cells.Where(c => c.InGroup).Count();
    int i = 0;

    if (singleScore == 0 && combinedScore == 0) return 0;

    for (i = 0; i < Squares.Length; i++) {
      if (combinedScore <= Squares[i] + 1) break;
    }

    float multiplier = 0.5f * i;

    combinedScore = 2 * combinedScore + Mathf.RoundToInt(multiplier * combinedScore);

    // There's also some other bonus that applies but not quite sure what it is

    return singleScore + combinedScore;
  }

  // The longer you play the greater the damage
  static int TimeBonus(int round) {
    return round / 20;
  }

  // Bonus for destroying gems near the top of your screen.
  // TODO DEFINE
  static int HeightBonus(List<Cell> cells) {
    return 0;
  }

  // All Clear adds a +6 counter gem drop at the end of you attack.
  static int AllClearBonus(int activeCount) {
    return activeCount == 0 ? 6 : 0;
  }

  // Bonus for first attack
  // TODO DEFINE
  static int FirstAttackBonus() {
    return 0;
  }
}