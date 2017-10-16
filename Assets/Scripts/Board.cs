using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
	[Tooltip ("Top edge of the board")]
	public int top = 0;
	[Tooltip ("Left edge of the board")]
	public int left = 0;

	[Tooltip ("Number of columns")]
	public int columns = 6;
	[Tooltip ("Number of rows")]
	public int rows = 12;

	[Tooltip ("Width of a board cell")]
	public int cellWidth = 20;

	[Tooltip ("Speed in which cellgroup drops normally")]
	public float fallSpeed = 20f;
	[Tooltip ("Speed in which cellgroup drops when player accelerates the drop")]
	public float fallSpeedFast = 40f;

	private GameManager game;

	void Start () {

	}

	void Update () {
	}

	void InitializeBoard () {

	}
}
