using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Game is responsible for passing information between boards
// and generating pieces for all boards
public class GameManager : MonoBehaviour {
    public static GameManager instance = null;

    public int playerCount = 2;
    public Board boardPrefab;
	public GameObject playArea;

    private Board[] boards;

    void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

    }

    // Update is called once per frame
    void Update () {
    }

    void SpawnBoards() {
    	boards = new Board[playerCount];

    	for (int n = 0; n < playerCount; n++) {
			boards[n] = SpawnSingleBoard(n);
    	}
    }

    Board SpawnSingleBoard(int boardNum) {
 //   	Vector2 pos = GetOriginForBoardNumber(boardNum);
		Board board = Instantiate(boardPrefab);
		return board;
    }

//    Vector2 GetOriginForBoardNumber(int boardNum) {
//    	//playAreaPrefab.renderer.bounds.size.x;
//		return new Vector2(boardNum * 500, 0);
//    }
}
