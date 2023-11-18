using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AiChess : MonoBehaviour
{
    public static AiChess Instance { set; get; }

    /* ------------------------- Cloning the Environment ----------------------*/
    private List<Chessman> ActiveChessmans;
    public Chessman[,] Chessmans;
    // One refernece of actual Chessmans[,]
    // As we will have to change the main Chessmans[,] array in BoardManager with its clone
    // (Because class Chessman and its descendent classes are using BoardManager.Instance.Chessmans)
    // And restore that again after thinking about NPC's next move (After Think() call gets over in NPCMove())
    // (Same goes for all Kings and Rooks, EnPassant move array)
    private Chessman[,] ActualChessmansReference;

    // Stack to store state history
    private Stack< State> History;

    // Maximum depth of exploration (No of total further moves to see the outcomes)
    private int maxDepth;

    // NPC chessman to be moved and position to where to move
    private Chessman NPCSelectedChessman = null;
    private int moveX = -1;
    private int moveY = -1;
    private int winningValue = 0;

    // Variable to count Avg Response Time 
    private long totalTime = 0;
    private long totalRun = 0;
    public  long averageResponseTime = 0;

    string detail, board;
    public List<Chessman> dseat = new List<Chessman>();
    List<Chessman> _blackPieces = new List<Chessman>();   // for a certain round, current left black pieces
    List<Chessman> _whitePieces = new List<Chessman>();   // for a certain round, current left white pieces

    int _whiteScore = 0;
    int _blackScore = 0;
    weightMatrix _weight = new weightMatrix();

    private void Start()
    {
        Instance = this;
    }

    // Funtion that makes NPC move
    public void NPCMove()
    {
        // New State History Stack
        History = new Stack< State>();

        /* --------------------- Sense --------------------- */
        
        ActualChessmansReference = BoardManager.Instance.Chessmans;

        ActiveChessmans = new List<Chessman>();
        Chessmans = new Chessman[5, 5];

        for(int x=0; x<5; x++)
            for(int y=0; y<5; y++)
            {
                if(ActualChessmansReference[x, y] != null)
                {
                    Chessman currChessman = ActualChessmansReference[x, y].Clone();
                    ActiveChessmans.Add(currChessman);
                    Chessmans[x, y] = currChessman;
                }
                else
                {
                    Chessmans[x, y] = null;
                }
            }

        Shuffle(ActiveChessmans);
        
        
        /* --------------------- Think --------------------- */

        // Critical Part:
        // We are about to change the heart of the Game Management which is taken care by BoardManager script
        // And that is Chessmans[,] array located in BoardManager script
        // We need to change it because in some functions which are declared in this script, which will think of NPC's next move,
        // Are using some other class functions also. These other class functions are using BoardManager.Instance.Chessmans array
        // As it is storing pointers to all the chessmans present on the board at some position.
        // Hence already stored reference to the actual Chessmans[,] array and now assigning it the different clone we made
        // (Same goes for all Kings and Rooks, EnPassant move array)
        BoardManager.Instance.Chessmans = Chessmans;

        // Think for which favourable move to make
        Think();

        // Restoring the Chessmans[,] array
        BoardManager.Instance.Chessmans = ActualChessmansReference;
        

        /* ---------------------- Act ---------------------- */
        // For most favourable move
        // select chessman
        Debug.Log(NPCSelectedChessman.GetType() + " to (" + moveX + ", " + moveY + ") " + winningValue + "\n"); // remove this line
        BoardManager.Instance.selectedChessman = BoardManager.Instance.Chessmans[NPCSelectedChessman.CurrentX, NPCSelectedChessman.CurrentY];
        BoardManager.Instance.allowedMoves = BoardManager.Instance.selectedChessman.PossibleMove();
        
        // move chessman
        BoardManager.Instance.MoveChessEssenceLogic(moveX, moveY);
    }

    private void Think()
    {
        maxDepth = 2;
        int depth = maxDepth-1;
        // winningValue = MiniMax(depth, true);
        //winningValue = AlphaBeta(depth, true, System.Int32.MinValue, System.Int32.MaxValue);
        winningValue = MiniMax(depth, true);
    }
    
    private int MiniMax(int depth, bool isMax)
    {
        // If max depth is reached or Game is Over
        if(depth == 0 || isGameOver())
        {
            // Static Evaluation Function
            int value = StaticEvaluationFunction();
            
            return value;
        }
        

        // If it is max turn(NPC turn : Black)
        if(isMax)
        {
            int hValue = System.Int32.MinValue;
            // int ind = 0;
            // Get list of all possible moves with their heuristic value
            // For all chessmans
            foreach(Chessman chessman in ActiveChessmans.ToArray())
            {
                if(chessman.isWhite) continue;

                bool[,] allowedMoves = chessman.PossibleMove();
                
                // For all possible moves
                for(int x=0; x<5; x++)
                {
                    for(int y=0; y<5; y++)
                    {
                        if(allowedMoves[x, y])
                        {
                            // Critical Section : 
                            // 1) Making the current move to see next possible moves after this move in next calls
                            Move(chessman, x, y, depth);

                            // 2 ) Calculate heuristic value current move
                            int thisMoveValue = MiniMax(depth-1, !isMax);

                            if(hValue < thisMoveValue) 
                            {
                                hValue = thisMoveValue;

                                // Remember which move gave the highest hValue
                                if(depth == maxDepth-1)
                                {
                                    NPCSelectedChessman = chessman;
                                    moveX = x;
                                    moveY = y;
                                }
                            }
                            

                            // 3 ) Undo the current move to get back the same state that was there before making the current move
                            Undo(depth);
                        }
                    }
                }
            }
            
            return hValue;
        }
        // If it is min turn(Player turn : White)
        else
        {
            int hValue = System.Int32.MaxValue;
            // int ind = 0;

            // Get list of all possible moves with their heuristic value
            // For all chessmans
            foreach(Chessman chessman in ActiveChessmans.ToArray())
            {
                if(!chessman.isWhite) continue;

                bool[,] allowedMoves = chessman.PossibleMove();
                
                // For all possible moves
                for(int x=0; x<5; x++)
                {
                    for(int y=0; y<5; y++)
                    {
                        if(allowedMoves[x, y])
                        {

                            // Critical Section : 
                            // 1) Making the current move to see next possible moves after this move in next calls
                            Move(chessman, x, y, depth);

                            // 2 ) Calculate heuristic value current move
                            int thisMoveValue = MiniMax(depth-1, !isMax);

                            if(hValue > thisMoveValue) 
                            {
                                hValue = thisMoveValue;
                                // The following 6-7 lines are commented, that is suggesting that 
                                // We won't update NPCSelectedChessman, moveX and moveY in min turn
                                 if(depth == maxDepth-1)
                                 {
                                     //NPCSelectedChessman = chessman;
                                     //moveX = x;
                                     //moveY = y;
                                 }
                            }

                            // 3 ) Undo the current move to get back the same state that was there before making the current move
                            Undo(depth);
                        }
                    }
                }
            }

            // if(depth == maxDepth-1) detail += "ActiveChessmans : \n" + ActiveChessmansDetail + "\n";

            return hValue;
        }
    }

    private int AlphaBeta(int depth, bool isMax, int alpha, int beta)
    {
        //getBoardState();
        // If max depth is reached or Game is Over
        if(depth == 0 || isGameOver())
        {
            // Static Evaluation Function
            int value = StaticEvaluationFunction();
            
            return value;
        }

        // string ActiveChessmansDetail = "";

        // If it is max turn(NPC turn : Black)
        if(isMax)
        {
            int hValue = System.Int32.MinValue;
            // int ind = 0;
            // Get list of all possible moves with their heuristic value
            // For all chessmans
            //for (int i = 0; i < _blackPieces.Count; i++)
            foreach(Chessman chessman in ActiveChessmans.ToArray())
            {
                // ActiveChessmansDetail = ActiveChessmansDetail + "(" + ++ind + ")" + (chessman.isWhite?"White":"Black") + chessman.GetType() + "(" + chessman.currentX + ", " + chessman.currentY + ")" + "\t\t ";
                //Chessman chessman = _blackPieces[i];
                if(chessman.isWhite) continue;

                bool[,] allowedMoves = chessman.PossibleMove();

                // detail = detail + "(" + ind + ") " + (chessman.isWhite?"White":"Black") + chessman.GetType() + " at (" + chessman.currentX + ", " + chessman.currentY + ") moves :" + printMoves(allowedMoves);

                // For all possible moves
                for(int x=0; x<5; x++)
                {
                    for(int y=0; y<5; y++)
                    {
                        if(allowedMoves[x, y])
                        {
                            // detail = detail + printTabs(maxDepth - depth) + "(" + ind + ") " + " " + (depth + " Moving Black " + chessman.GetType() + " to (" + x + ", " + y + ")");
                            
                            // Critical Section : 
                            // 1) Making the current move to see next possible moves after this move in next calls
                            Move(chessman, x, y, depth);

                            // 2 ) Calculate heuristic value current move
                            int thisMoveValue = AlphaBeta(depth-1, !isMax, alpha, beta);
                            

                            // 3 ) Undo the current move to get back the same state that was there before making the current move
                            Undo(depth);

                            if(hValue < thisMoveValue) 
                            {
                                hValue = thisMoveValue;

                                // Remember which move gave the highest hValue
                                if(depth == maxDepth-1)
                                {
                                    NPCSelectedChessman = chessman;
                                    moveX = x;
                                    moveY = y;
                                }
                            }

                            if(hValue > alpha) 
                                alpha = hValue;

                            if(beta <= alpha)
                                break;
                        }
                    }

                    if(beta <= alpha)
                        break;
                }

                if(beta <= alpha)
                    break;
            }

            // if(depth == maxDepth-1) detail += "ActiveChessmans : \n" + ActiveChessmansDetail + "\n";

            return hValue;
        }
        // If it is min turn(Player turn : White)
        else
        {
            int hValue = System.Int32.MaxValue;
            // int ind = 0;

            // Get list of all possible moves with their heuristic value
            // For all chessmans
            foreach(Chessman chessman in ActiveChessmans.ToArray())
            //for (int i = 0; i < _whitePieces.Count; i++)
            {
                // ActiveChessmansDetail = ActiveChessmansDetail + "\n(" + ++ind + ")" + (chessman.isWhite?"White":"Black") + chessman.GetType() + "(" + chessman.currentX + ", " + chessman.currentY + ")" + "\t\t ";
                //Chessman chessman = _whitePieces[i];
                if(!chessman.isWhite) continue;

                bool[,] allowedMoves = chessman.PossibleMove();

                // if(depth == 2) detail = detail + "(" + ind + ") " + (chessman.isWhite?"White":"Black") + chessman.GetType() + " at (" + chessman.currentX + ", " + chessman.currentY + ") moves :" + printMoves(allowedMoves);

                // For all possible moves
                for(int x=0; x<5; x++)
                {
                    for(int y=0; y<5; y++)
                    {
                        if(allowedMoves[x, y])
                        {
                            // detail = detail + printTabs(maxDepth - depth) + "(" + ind + ") " + " " + (depth + " Moving White " + chessman.GetType() + " to (" + x + ", " + y + ")\n");
                            
                            // Critical Section : 
                            // 1) Making the current move to see next possible moves after this move in next calls
                            Move(chessman, x, y, depth);

                            // 2 ) Calculate heuristic value current move
                            int thisMoveValue = AlphaBeta(depth-1, !isMax, alpha, beta);

                            // if(depth-1 == 0) detail = detail + " " + thisMoveValue + "\n";
                            // else detail = detail + "\n";

                            // 3 ) Undo the current move to get back the same state that was there before making the current move
                            Undo(depth);

                            if(hValue > thisMoveValue) 
                            {
                                hValue = thisMoveValue;
                                // The following 6-7 lines are commented, that is suggesting that 
                                // We won't update NPCSelectedChessman, moveX and moveY in min turn
                                 //if(depth == maxDepth-1)
                                 //{
                                 //    NPCSelectedChessman = chessman;
                                 //    moveX = x;
                                 //   moveY = y;
                                 //}
                            }

                            if(hValue < beta) 
                                beta = hValue;

                            if(beta <= alpha)
                                break;
                        }
                    }

                    if(beta <= alpha)
                        break;
                }

                if(beta <= alpha)
                    break;
            }

            // if(depth == maxDepth-1) detail += "ActiveChessmans : \n" + ActiveChessmansDetail + "\n";

            return hValue;
        }
    }

    // This function is simply calculating the summation of Chessman values
    private int StaticEvaluationFunction()
    {
        int TotalScore = 0;
        int curr = 10;
        foreach(Chessman chessman in ActiveChessmans)
        {
            if(chessman.isWhite)
                TotalScore -= curr;
            else
                TotalScore += curr;
        }
        return TotalScore;
    }

    

    int _Evaluate()
    {
        int total = 0;
        float pieceDifference = 0;
        float whiteWeight = 0;
        float blackWeight = 0;

        foreach (Chessman cm in _whitePieces)
        {
            Vector2 position = new Vector2((int)cm.CurrentX, (int)cm.CurrentY);
            //whiteWeight -= _weight.GetBoardWeight(position, "white");
            total += _weight.GetBoardWeight(position, "white");
        }
        foreach (Chessman cm in _blackPieces)
        {
            Vector2 position = new Vector2((int)cm.CurrentX, (int)cm.CurrentY);
            //blackWeight += _weight.GetBoardWeight(position, "black");
            total -= _weight.GetBoardWeight(position, "black");
        }
        pieceDifference = (_blackScore + (blackWeight / 100)) - (_whiteScore + (whiteWeight / 100));
        //return Mathf.RoundToInt(pieceDifference * 100);
        return total;
    }

    // Checking for checkmate (Game Over)
    private bool isGameOver()
    {
        // To be implemented
        int currScore = StaticEvaluationFunction();
        if((currScore < -150 ) || (currScore > 150))
            return true;
        return false;
    }

    private void Move(Chessman chessman, int x, int y, int depth)
    {
        // Current state variables to be stored
        // About chessman to be moved
        (Chessman chessman, (int x, int y) oldPosition, (int x, int y) newPosition, bool isMoved) movedChessman;
        // Chessman to be captured
        List<Chessman> capturedChessman = null;


        movedChessman.chessman = chessman;
        movedChessman.oldPosition = (chessman.CurrentX, chessman.CurrentY);
        movedChessman.newPosition = (x, y);
        movedChessman.isMoved = chessman.isMoved;
        
        // Capturing
        Chessman opponent = Chessmans[x, y];
        // Capture an opponent piece
            //capturedChessman.chessman = opponent;
            //capturedChessman.Position = (x, y);



        Chessmans[x, y] = null;
        //ActiveChessmans.Remove(opponent);
        
        // Now moving
        Chessmans[chessman.CurrentX, chessman.CurrentY] = null;
        Chessmans[x, y] = chessman;
        chessman.SetPosition(x, y);
        chessman.isMoved = true;
        Ganh(Chessmans[x, y]);
        Chet(Chessmans[x, y].isWhite);      
        
        capturedChessman = new List<Chessman>();
        foreach (var ob in dseat)
        {
            capturedChessman.Add(ob);
        }
        dseat.Clear();

        // Save the current state to the History Stack
        State currentState = new State();
        currentState.SetState(movedChessman, capturedChessman, depth);
        History.Push(currentState);
    }

    private void Undo(int depth)
    {
        // Get current state from the top of the stack
        State currentState = History.Pop();

        // Current depth should be matched with the currentState.depth from the stack
        if(depth != currentState.depth)
        {
            Debug.Log("Depth not matched!!!");
            return;
        }

        // Current state variables
        // About chessman to be moved
        var movedChessman = currentState.movedChessman;
        // Chessman to be captured
        var capturedChessman = currentState.capturedChessman;
        
        // Restore the moved chessman from newPosition to oldPosition
        Chessman chessman = movedChessman.chessman;
        chessman.isMoved = movedChessman.isMoved;
        chessman.SetPosition(movedChessman.oldPosition.x, movedChessman.oldPosition.y);
        Chessmans[movedChessman.oldPosition.x, movedChessman.oldPosition.y] = chessman;
        Chessmans[movedChessman.newPosition.x, movedChessman.newPosition.y] = null;
        
        // Restore the captured piece to its position
        var opponent = capturedChessman;
        if(opponent.Count > 0)
        {
            foreach (var chessman1 in opponent)
            {
                Chessmans[chessman1.CurrentX, chessman1.CurrentY] = chessman1;
                Chessmans[chessman1.CurrentX, chessman1.CurrentY].isWhite = !chessman1.isWhite;
                chessman1.SetPosition(chessman1.CurrentX, chessman1.CurrentY);
                ActiveChessmans.Add(chessman1);
            }
            
        }
    }
    
    private void Ganh(Chessman x)
    {
        List<Tuple<int, int>> temp = x.Ganh(x.isWhite,true);
        List<Tuple<int, int>> temp1 = new List<Tuple<int, int>>();
        foreach (var valuePair in temp)
        {
            dseat.Add(Chessmans[valuePair.Item1, valuePair.Item2]);
            Chessmans[valuePair.Item1, valuePair.Item2].isWhite = ! Chessmans[valuePair.Item1, valuePair.Item2].isWhite;
            temp1.Add(valuePair);
		    
        }

        foreach (var valuePair in temp1)
        {
            Ganh(Chessmans[valuePair.Item1, valuePair.Item2]);
        }
    }

    private void Chet(bool isWhiteTurn)
    {
        List<Chessman> y = Vay(isWhiteTurn);
        if (y.Count == 0)
        {
            //Debug.Log(isWhiteTurn ? "white" : "black");
        }
        else
        {
            foreach (Chessman chessman in y)
            {
                List<Chessman> checkedChessmans = new List<Chessman>();
                if (!CheckDD(chessman,checkedChessmans))
                {
                    dseat.Add(Chessmans[chessman.CurrentX, chessman.CurrentY]);
                    chessman.isWhite = ! chessman.isWhite;
			        
                    Ganh(Chessmans[chessman.CurrentX, chessman.CurrentY]);
                }
            }
        }
    }
    private List<Chessman> Vay(bool kt)
    {
        List<Chessman> x = new List<Chessman>();
        foreach (Chessman o in ActiveChessmans)
        {
            Chessman y = o;
            if (y.isWhite != kt && y.KhongDiDuoc(true))
            {
                x.Add(y);
            }
        }
        return x;
    }

    private bool CheckDD(Chessman x,List<Chessman> checkedChessmans)
    {
        checkedChessmans.Add(x);
        
        List<Tuple<int, int>> y = BoardManager.Instance.dske[Tuple.Create(x.CurrentX, x.CurrentY)];
        List<Chessman> alliesWithPath = new List<Chessman>();

        foreach (var temp in y)
        {
            if (Chessmans[temp.Item1, temp.Item2] != null)
            {
                if (Chessmans[temp.Item1,temp.Item2].isWhite == x.isWhite && Chessmans[temp.Item1,temp.Item2].KhongDiDuoc(true))
                {
                    alliesWithPath.Add(Chessmans[temp.Item1,temp.Item2]);
                }else if (Chessmans[temp.Item1,temp.Item2].isWhite == x.isWhite && !Chessmans[temp.Item1, temp.Item2].KhongDiDuoc(true))
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

    public void Shuffle(List<Chessman> list)  
    {  
        System.Random rng = new System.Random();

        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            Chessman value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}