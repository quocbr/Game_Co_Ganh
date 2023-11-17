using System;
 using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
 using Random = UnityEngine.Random;

 public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }
    public Dictionary<Tuple<int, int>, List<Tuple<int, int>>> dske;
    public bool[,] allowedMoves { set; get; }

    public Chessman[,] Chessmans { set; get; }  //Chessman array and a property
    public Chessman selectedChessman;
    
    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;
    
    public bool smartOpponent = false;

    private Quaternion orientation = Quaternion.Euler(0, 0, 0);

    private bool isWhiteTurn = true;
    private bool pause = false;

    private void Start()
    {
        if (PlayerPrefs.GetInt("ChoiVsMay") == 1)
        {
            smartOpponent = true;
        }
        else
        {
            smartOpponent = false;
        }
        
        Instance = this;
        InitKe();
        SpawnAllChessmans();
    }


    private void Update()
    {
        if(pause) return;
        DrawChessBoard();
        UpdateSelection();


        if (Input.GetMouseButtonDown(0)) // todo: add bool to let this step wait for ai's move finish then allow click
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedChessman == null)
                {
                    //select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    //move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
            else
            {
                selectedChessman = null;
                BoardHighlights.Instance.HideHighlights();
            } 
        }
        else if(!isWhiteTurn && smartOpponent)
        {
            // NPC will make a move
            AiChess.Instance.NPCMove();
        }
        EndGame();
    }
    
    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x, y] == null) { // 选中的位置没有棋子
            return;
        }

        if (Chessmans[x, y].isWhite != isWhiteTurn) // Once pick a black piece while it is the white turn so that does not work
        {
            return;
        }
        allowedMoves = Chessmans[x, y].PossibleMove();  // possible moves is a 8*8 2d array initial value false， 重要🌟：since this function is override by the subchild , so it wont return orginal 8*8 false bool matrix , but a meaning one followed the rules

        selectedChessman = Chessmans[x, y];

        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);

    }

    public void MoveChessman(int x, int y)  // 棋子落点坐标
    {
        MoveChessEssenceLogic(x, y);
    }

    public void MoveChessEssenceLogic(int x, int y)
    {
        
        if (allowedMoves[x, y])
        {
            Chessmans[x, y] = Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY];
                Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
                Chessmans[x, y].gameObject.transform.position = new Vector3(x, y, 0);
                Chessmans[x, y].CurrentX = x;
                Chessmans[x, y].CurrentY = y;
                Ganh(Chessmans[x, y]);
                Chet();

                isWhiteTurn = !isWhiteTurn;
                SoundController.Instance.Danhco();
            Game.Instance.SetLuot(isWhiteTurn?"quân xanh":"quân đỏ");

        }

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;//Select next Chessman
    }

    private void Ganh(Chessman x)
    {
        List<Tuple<int, int>> temp = x.Ganh(x.isWhite);
        List<Tuple<int, int>> temp1 = new List<Tuple<int, int>>();
        foreach (var valuePair in temp)
        {
            Chessman c = Chessmans[valuePair.Item1, valuePair.Item2];
            activeChessman.Remove(c.gameObject);
            Destroy(c.gameObject);
            SpawnChessman(isWhiteTurn?0:1,valuePair.Item1, valuePair.Item2);
            //Ganh(Chessmans[valuePair.Item1, valuePair.Item2]);
            temp1.Add(valuePair);
        }

        foreach (var valuePair in temp1)
        {
            Ganh(Chessmans[valuePair.Item1, valuePair.Item2]);
        }
    }

    private void Chet()
    {
        List<Chessman> y = Vay(isWhiteTurn);
        if (y.Count == 0)
        {
            Debug.Log(isWhiteTurn ? "white" : "black");
        }
        else
        {
            foreach (Chessman chessman in y)
            {
                List<Chessman> checkedChessmans = new List<Chessman>();
                if (!CheckDD(chessman,checkedChessmans))
                {
                    Chessman c = chessman;
                    activeChessman.Remove(c.gameObject);
                    Destroy(c.gameObject);
                    SpawnChessman(isWhiteTurn?0:1,chessman.CurrentX, chessman.CurrentY);
                    Debug.Log("Vay:" + chessman.CurrentX+" "+ chessman.CurrentY);
                    Ganh(Chessmans[chessman.CurrentX, chessman.CurrentY]);
                }
            }
        }
    }
    private List<Chessman> Vay(bool kt)
    {
        List<Chessman> x = new List<Chessman>();
        foreach (GameObject o in activeChessman)
        {
            Chessman y = o.GetComponent<Chessman>();
            if (y.isWhite != kt && y.KhongDiDuoc())
            {
                x.Add(y);
            }
        }
        return x;
    }

    private bool CheckDD(Chessman x,List<Chessman> checkedChessmans)
    {
        checkedChessmans.Add(x);
        
        List<Tuple<int, int>> y = dske[Tuple.Create(x.CurrentX, x.CurrentY)];
        List<Chessman> alliesWithPath = new List<Chessman>();

        foreach (var temp in y)
        {
            if (Chessmans[temp.Item1, temp.Item2] != null)
            {
                if (Chessmans[temp.Item1,temp.Item2].isWhite == x.isWhite && Chessmans[temp.Item1,temp.Item2].KhongDiDuoc())
                {
                    alliesWithPath.Add(Chessmans[temp.Item1,temp.Item2]);
                }else if (Chessmans[temp.Item1,temp.Item2].isWhite == x.isWhite && !Chessmans[temp.Item1, temp.Item2].KhongDiDuoc())
                {
                    return true;
                }
            }
        }

        if (alliesWithPath.Count == 0) {
            return false;
        }

        bool isBlocked = false;

        for (int i = 0; i < alliesWithPath.Count; i++) {
            if(checkedChessmans.Contains(alliesWithPath[i])) continue;
            
            if (CheckDD(alliesWithPath[i],checkedChessmans)) {
                isBlocked = true;
            }
        }

        return isBlocked;
    }

    private void InitKe()
    {
        dske = new Dictionary<Tuple<int, int>, List<Tuple<int, int>>>();
        dske[Tuple.Create(0, 0)] = new List<Tuple<int, int>>() {Tuple.Create(1,1), Tuple.Create(0,1),Tuple.Create(1,0)};
        dske[Tuple.Create(1, 0)] = new List<Tuple<int, int>>() {Tuple.Create(0,0), Tuple.Create(1,1),Tuple.Create(2,0)};
        dske[Tuple.Create(2, 0)] = new List<Tuple<int, int>>() {Tuple.Create(1,0), Tuple.Create(1,1),Tuple.Create(2,1),Tuple.Create(3,1),Tuple.Create(3,0)};
        dske[Tuple.Create(3, 0)] = new List<Tuple<int, int>>() {Tuple.Create(2,0), Tuple.Create(3,1),Tuple.Create(4,0)};
        dske[Tuple.Create(4, 0)] = new List<Tuple<int, int>>() {Tuple.Create(3,0), Tuple.Create(3,1),Tuple.Create(4,1)};
        
        dske[Tuple.Create(0, 1)] = new List<Tuple<int, int>>() {Tuple.Create(0,0), Tuple.Create(1,1),Tuple.Create(0,2)};
        dske[Tuple.Create(1, 1)] = new List<Tuple<int, int>>() {Tuple.Create(0,0), Tuple.Create(0,1),Tuple.Create(0,2),Tuple.Create(1,2),Tuple.Create(2,2),Tuple.Create(2,1),Tuple.Create(2,0),Tuple.Create(1,0)};
        dske[Tuple.Create(2, 1)] = new List<Tuple<int, int>>() {Tuple.Create(1,1), Tuple.Create(2,0),Tuple.Create(3,1),Tuple.Create(2,2)};
        dske[Tuple.Create(3, 1)] = new List<Tuple<int, int>>() {Tuple.Create(2,0), Tuple.Create(2,1),Tuple.Create(2,2),Tuple.Create(3,2),Tuple.Create(4,2),Tuple.Create(4,1),Tuple.Create(4,0),Tuple.Create(3,0)};
        dske[Tuple.Create(4, 1)] = new List<Tuple<int, int>>() {Tuple.Create(4,0), Tuple.Create(3,1),Tuple.Create(4,2)};
        
        dske[Tuple.Create(0, 2)] = new List<Tuple<int, int>>() {Tuple.Create(0,1), Tuple.Create(1,1),Tuple.Create(1,2),Tuple.Create(1,3),Tuple.Create(0,3)};
        dske[Tuple.Create(1, 2)] = new List<Tuple<int, int>>() {Tuple.Create(0,2), Tuple.Create(1,1),Tuple.Create(2,2),Tuple.Create(1,3)};
        dske[Tuple.Create(2, 2)] = new List<Tuple<int, int>>() {Tuple.Create(1,1), Tuple.Create(1,2),Tuple.Create(1,3),Tuple.Create(2,3),Tuple.Create(3,3),Tuple.Create(3,2),Tuple.Create(3,1),Tuple.Create(2,1)};
        dske[Tuple.Create(3, 2)] = new List<Tuple<int, int>>() {Tuple.Create(2,2), Tuple.Create(3,3),Tuple.Create(4,2),Tuple.Create(3,1)};
        dske[Tuple.Create(4, 2)] = new List<Tuple<int, int>>() {Tuple.Create(4,1), Tuple.Create(3,1),Tuple.Create(3,2),Tuple.Create(3,3),Tuple.Create(4,3)};
        
        dske[Tuple.Create(0, 3)] = new List<Tuple<int, int>>() {Tuple.Create(0,2), Tuple.Create(1,3),Tuple.Create(0,4)};
        dske[Tuple.Create(1, 3)] = new List<Tuple<int, int>>() {Tuple.Create(0,2), Tuple.Create(0,3),Tuple.Create(0,4),Tuple.Create(1,4),Tuple.Create(2,4),Tuple.Create(2,3),Tuple.Create(2,2),Tuple.Create(1,2)};
        dske[Tuple.Create(2, 3)] = new List<Tuple<int, int>>() {Tuple.Create(1,3), Tuple.Create(2,4),Tuple.Create(3,3),Tuple.Create(2,2)};
        dske[Tuple.Create(3, 3)] = new List<Tuple<int, int>>() {Tuple.Create(2,2), Tuple.Create(2,3),Tuple.Create(2,4),Tuple.Create(3,4),Tuple.Create(4,4),Tuple.Create(4,3),Tuple.Create(4,2),Tuple.Create(3,2)};
        dske[Tuple.Create(4, 3)] = new List<Tuple<int, int>>() {Tuple.Create(4,4), Tuple.Create(3,3),Tuple.Create(4,2)};
        
        dske[Tuple.Create(0, 4)] = new List<Tuple<int, int>>() {Tuple.Create(0,3), Tuple.Create(1,4),Tuple.Create(1,3)};
        dske[Tuple.Create(1, 4)] = new List<Tuple<int, int>>() {Tuple.Create(0,4), Tuple.Create(1,3),Tuple.Create(2,4)};
        dske[Tuple.Create(2, 4)] = new List<Tuple<int, int>>() {Tuple.Create(1,4), Tuple.Create(1,3),Tuple.Create(2,3),Tuple.Create(3,3),Tuple.Create(3,4)};
        dske[Tuple.Create(3, 4)] = new List<Tuple<int, int>>() {Tuple.Create(2,4), Tuple.Create(3,3),Tuple.Create(4,4)};
        dske[Tuple.Create(4, 4)] = new List<Tuple<int, int>>() {Tuple.Create(4,3), Tuple.Create(3,4),Tuple.Create(3,3)};
    }

    //hoan thanh
    private void DrawChessBoard()
    {
        Vector3 widthLine = Vector3.right * 4;
        Vector3 heightLine = Vector3.up * 4;

        for (int i = 0; i <= 4; i++)
        {
            Vector3 start = Vector3.up * i;
            Debug.DrawLine(start, start + widthLine);
            for (int j = 0; j <= 4; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);
            }
        }

        //Draw the selection
        if (selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(Vector3.up * selectionY + Vector3.right * selectionX,
                Vector3.up * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(Vector3.up * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.up * selectionY + Vector3.right * (selectionX + 1));
        }
    }
    
    //Hoan thanh
    public void SpawnChessman(int index, int x, int y)    // index represent chess piece type
    {
        GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x, y), orientation) as GameObject;
        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    //Hioan thanh
    public void SpawnAllChessmans()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[5, 5];
        //EnPassantMove = new int[2] { -1, -1 };

        //Spawn the white team
        //Pawns
        SpawnChessman(0, 0, 1);
        SpawnChessman(0, 4, 1);
        SpawnChessman(0, 4, 2);
        for (int i = 0; i < 5; i++)
        {
            SpawnChessman(0, i, 0);
        }

        //Spawn the black team

        //Pawns
        SpawnChessman(1, 0, 2);
        SpawnChessman(1, 0, 3);
        SpawnChessman(1, 4, 3);
        for (int i = 0; i < 5; i++)
        {
            SpawnChessman(1, i, 4);
        }
    }
    
    //Hoan thanh
    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = new Vector3(x,y,0);
        //origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        //origin.z += (TILE_SIZE * y) + TILE_OFFSET;
        return origin;
    }
    //Hoan thanh
    private void UpdateSelection()
    {
        if (!Camera.main)
        {
            return;
        }
            
        Vector3 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit;//, LayerMask.GetMask("ChessPlane")
        hit = Physics2D.Raycast(ray, Vector2.zero, Mathf.Infinity);
        if (hit)
        {
            //Debug.Log(hit.point);
            selectionX = (int)Mathf.Round(hit.point.x);
            selectionY = (int)Mathf.Round(hit.point.y);
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }
    
    public void EndGame()
    {
        int trang = 0;
        int den = 0;
        foreach (var ob in activeChessman)
        {
            if (ob.GetComponent<Chessman>().isWhite)
            {
                trang++;
            }
            else
            {
                den++;
            }
        }
        if(den > 0 && trang > 0) return;
        
        Game.Instance.EndGame(isWhiteTurn);
        this.PlayBack();
    }

    public void PlayBack()
    {
        foreach (GameObject go in activeChessman)
            Destroy(go);
        //Rebegin the game and white piece first /who wins who goes first if not write "isWhiteTurn = true;" 
        isWhiteTurn = true;
        BoardHighlights.Instance.HideHighlights();
        SpawnAllChessmans();
    }
    
    public void SetPause(int pause)
    {
        this.pause = pause == 0;
        Time.timeScale = pause;
    }

   
}