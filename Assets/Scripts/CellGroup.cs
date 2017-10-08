using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CellGroups move as a unit and render their cells
public class CellGroup : MonoBehaviour {
  public enum State {
    Spawning, // Cell is about to become Falling
    Falling,  // Cell is moving towards target
    Landed,   // Touching another CellGroup but still able to be moved
    Fixed,    // No longer player controllable
    Preview   // Showing upcoming CellGroup
  }

  private Cell[,] cells;
  private Vector3 targetPosition;
  private State state;
  private bool isPlayerControlled = false;
  private int orientation = 0;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

  private void Render() {
  }
}
