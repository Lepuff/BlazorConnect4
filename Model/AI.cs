using System;
using BlazorConnect4.Model;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace BlazorConnect4.AIModels
{
    [Serializable]
    public abstract class AI
    {
        // Funktion för att beskriva 
        public abstract int SelectMove(Cell[,] grid);

        // Funktion för att skriva till fil.
        public virtual void ToFile(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Create))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                bformatter.Serialize(stream, this);
            }
        }

        // Funktion för att att läsa från fil.
        public static AI FromFile(string fileName)
        {
            AI returnAI;
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                returnAI = (AI)bformatter.Deserialize(stream);
            }
            return returnAI;

        }
    }


    [Serializable]
    public class RandomAI : AI
    {
        [NonSerialized] Random generator;

        public RandomAI()
        {
            generator = new Random();
        }

        public override int SelectMove(Cell[,] grid)
        {
            int action = generator.Next(0, 7);
            bool valid = GameEngineTwo.IsValid(grid, action);

            while (!valid)
            {
                action = generator.Next(0, 7);
                valid = GameEngineTwo.IsValid(grid, action);

            }

            return action;
        }
    }

    [Serializable]
    public class QAgent : AI
    {

        public Dictionary<String, double[]> Qdict;
       
        private CellColor PlayerColor;
        // Reward values
        public float InvalidMoveReward = -0.5F;
        public float WinningMoveReward = 1F;
        public float LosingMoveReward = -1F;
        public float DrawMoveReward = 0F;

        // Statistics
        public int wins = 0;
        public int losses = 0;
        public int ties = 0;
        public int invalidMoves = 0;

        


        public QAgent(CellColor playerColor)
        {

            if(playerColor == CellColor.Red)
            {
                PlayerColor = CellColor.Red;
            }
            else if (playerColor == CellColor.Yellow)
            {
                PlayerColor = CellColor.Yellow;
            }
            else
            {
                throw new Exception("Something is wrong with the references this should not happend");
            }

            Qdict = new Dictionary<String, double[]>();
            
        }





        public double GetQValue(Cell[,] grid, int action)
        {
            Random rnd = new Random();
            String key = GameBoard.GetHashStringCode(grid);
            if (Qdict.ContainsKey(key))
            {
                return Qdict[key][action];
            }
            else
            {
                double[] actions = { rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() }; // fill array with Random numbers;
                Qdict.Add(key, actions);
                return 0;
            }
        }

        public void SetQValue(Cell[,] grid, int action, double value)
        {
            Random rnd = new Random();
            String key = GameBoard.GetHashStringCode(grid);
            if (!Qdict.ContainsKey(key))
            {
                double[] actions = { rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() }; // fill array with Random numbers;
                Qdict.Add(key, actions);

            }
            Qdict[key][action] = value;

        }


        public override int SelectMove(Cell[,] grid)
        {
            double epsilon = 0.9F;
            int action = EpsilonGreedyAction(epsilon, grid);
            bool validMove = GameEngineTwo.IsValid(grid, action);
            // in the case that the best move is not a validmove, isntead randomize a move until a valid is found
            Random randomGen = new Random();
            while (!validMove)
            {

                action = randomGen.Next(0,7);
                validMove = GameEngineTwo.IsValid(grid, action);
            }
            Debug.Assert(GameEngineTwo.IsValid(grid, action));
            return action;
        }

        public int GetBestAction(Cell[,] state)
        {


            int action = 0;

            double value = GetQValue(state, 0);
            for (int i = 1; i < 7; i++)
            {
                if (GetQValue(state, i) > value)
                {
                    action = i;
                    value = GetQValue(state, i);
                }
            }
            bool validMove = GameEngineTwo.IsValid(state, action);
            Random randomGen = new Random();
            while (!validMove)
            {

                action = randomGen.Next(0,7);
                validMove = GameEngineTwo.IsValid(state, action);
            }

            return action;
        }

        public int EpsilonGreedyAction(double epsilon, Cell[,] state)
        {
            Random random = new Random();
            int action = -1;
            if (random.NextDouble() < epsilon)
            {
                action = random.Next(0, 7);
                while (!GameEngineTwo.IsValid(state, action))
                {
                    action = random.Next(0, 7);
                }
                
            }
            else
            {
                // Make highest valued move
                action = GetBestAction(state);
                while (!GameEngineTwo.IsValid(state, action))
                {
                    action = random.Next(0, 7);
                }
            }
            Debug.Assert(GameEngineTwo.IsValid(state, action));
            return action;
        }

        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            //temp.generator = new Random();
            return temp;
        }


        public bool IsTerminal(GameBoard board, int action)
        {
            bool isTerminalState = false;
            if (GameEngineTwo.IsWin(board, action, CellColor.Red))
            {
                isTerminalState = true;
            }
            else if (GameEngineTwo.IsWin(board, action, CellColor.Yellow))
            {
                isTerminalState = true;
            }
            else if (GameEngineTwo.IsDraw(board, action))
            {
                isTerminalState = true;
            }
            return isTerminalState;
        }

        public bool IsTerminalAndUpdateValues(GameBoard board, int action, CellColor playerColor)
        {
            bool isTerminalState = false;
            if (GameEngineTwo.IsWin(board,action,playerColor)) // If the game is terminal quit here and give the reward for all actions in this state.
            {
                wins++;
                SetQValue(board.Grid, action, WinningMoveReward);
                isTerminalState = true;
            }
            else if (GameEngineTwo.IsWin(board, action, GameEngineTwo.OtherPlayer(playerColor)))
            {
                losses++;
                SetQValue(board.Grid, action, LosingMoveReward);
                isTerminalState = true;
            }
            else if (GameEngineTwo.IsDraw(board,action))
            {
                ties++;
                SetQValue(board.Grid, action, DrawMoveReward);
                isTerminalState = true;
            }
            return isTerminalState;
        }
        /*
        public void Workout(AI oppositeAi, int iterations)
        {
            double epsilon = 0.7F;
            float gamma = 0.9F;
            float alpha = 0.5F;

            GameEngineTwo gameEngine = new GameEngineTwo();
            int opponentAction;
            CellColor opponenentColor = GameEngineTwo.OtherPlayer(PlayerColor);
            for (int i = 0; i < iterations; i++)
            {
                // new iteration, reset the gameBoard and playerturn
                gameEngine.Reset();
                bool terminal = false;

                if (PlayerColor == CellColor.Yellow)
                {
                    //opponentAction = oppositeAi.SelectMove(gameEngine.Board.Grid);
                    opponentAction = EpsilonGreedyAction(1,gameEngine.Board.Grid); 
                    gameEngine.MakeMove(opponentAction);
                }

                int action = EpsilonGreedyAction(epsilon, gameEngine.Board.Grid);
                Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, action));

                while (!terminal)
                {

                    double currentVal = GetQValue(gameEngine.Board.Grid, action);  //Q(s,a)

                    //The Q value for best next move
                    //Q(a',s')
                    GameBoard nextBoardState = gameEngine.Board.Copy();
                    int bestAction1 = EpsilonGreedyAction(-1,gameEngine.Board.Grid); // best action by epsilon -1
                    if (IsTerminal(nextBoardState, bestAction1)) 
                    {
                        break;
                    }
                    
                    Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, bestAction1));
                    GameEngineTwo.MakeMove(ref nextBoardState, gameEngine.PlayerTurn, bestAction1);


                    int oppositeAction = oppositeAi.SelectMove(nextBoardState.Grid);
                    if (IsTerminal(nextBoardState, oppositeAction))
                    {
                        break;
                    }
                    Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, oppositeAction));
                    GameEngineTwo.MakeMove(ref nextBoardState, GameEngineTwo.OtherPlayer(gameEngine.PlayerTurn), oppositeAction);

                    int bestAction2 = EpsilonGreedyAction(-1, gameEngine.Board.Grid);
                    Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, bestAction2));
                    double maxQvalueNextState = GetQValue(nextBoardState.Grid, bestAction2);
                    //update value

                    //                                        Q(a,s)    + alpha * (gamma * Max(Q(a',s))       - Q(s,a)                                  
                    SetQValue(gameEngine.Board.Grid, action, currentVal + alpha * (gamma * maxQvalueNextState - currentVal));

                    //we should make a new move and then let the opponent make a move

                    action = EpsilonGreedyAction(epsilon, gameEngine.Board.Grid);
                    
                    Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, action));
                    terminal = IsTerminalAndUpdateValues(gameEngine.Board, action, PlayerColor);
                    gameEngine.MakeMove(action);
                    // If players move is not terminal, make opponentAi move
                    if (!terminal)
                    {
                        opponentAction = oppositeAi.SelectMove(gameEngine.Board.Grid);
                        Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, opponentAction));
                        terminal = IsTerminalAndUpdateValues(gameEngine.Board, opponentAction, opponenentColor);
                        gameEngine.MakeMove(opponentAction);
                        
                    }
                }
            }
        }
        */

        public  void WorkoutV2(AI opponentAi ,int iterations)
        {

            double epsilon = 0.7F;
            float gamma = 0.9F;
            float alpha = 0.5F;

            GameEngineTwo gameEngine = new GameEngineTwo();
            int opponentAction;
            CellColor opponenentColor = GameEngineTwo.OtherPlayer(PlayerColor);
            for (int i = 0; i < iterations; i++)
            {
                // new iteration, reset the gameBoard and playerturn
                gameEngine.Reset();
                bool terminal = false;

                if (PlayerColor == CellColor.Yellow)
                {
                    //opponentAction = oppositeAi.SelectMove(gameEngine.Board.Grid);
                    opponentAction = EpsilonGreedyAction(1, gameEngine.Board.Grid);
                    gameEngine.MakeMove(opponentAction);
                }

                int action = EpsilonGreedyAction(epsilon, gameEngine.Board.Grid);
                Debug.Assert(GameEngineTwo.IsValid(gameEngine.Board.Grid, action));

                while (!terminal)
                {

                    if (GameEngineTwo.IsWin(gameEngine.Board, action, gameEngine.PlayerTurn))
                    {
                        SetQValue(gameEngine.Board.Grid, action, WinningMoveReward);
                        terminal = true;
                    }
                    else if (GameEngineTwo.IsDraw(gameEngine.Board, action))
                    {
                        SetQValue(gameEngine.Board.Grid, action, DrawMoveReward);
                        terminal = true;
                    }
                    else
                    {
                        //Q(s,a)
                        double currentVal = GetQValue(gameEngine.Board.Grid, action);

                        
                        GameBoard temporaryBoard = gameEngine.Board.Copy(); // make a non-reference copy of the gameboard.
                        //take action Q(s,a)
                        GameEngineTwo.MakeMove(ref temporaryBoard, PlayerColor, action);

                        //let the opponent take a move
                        opponentAction = opponentAi.SelectMove(temporaryBoard.Grid);
                        //observe if the oppents move is terminal, in that case update the values for our move that led to that possibility
                        if (GameEngineTwo.IsWin(temporaryBoard, opponentAction, opponenentColor))
                        {
                            SetQValue(gameEngine.Board.Grid, action, LosingMoveReward); //set the q values for the move that led to the opponents win
                            
                            break;
                        }
                        else if (GameEngineTwo.IsDraw(temporaryBoard, opponentAction)) 
                        {
                            SetQValue(gameEngine.Board.Grid, action, DrawMoveReward); //set the q values for the move that led to opponents draw move.
                            
                            break;
                        }
                        //take the opponent move
                        GameEngineTwo.MakeMove(ref temporaryBoard, opponenentColor, opponentAction);

                        //the opponentMove  is not terminal  Continue by observing the Q value of the best move in this new state.

                        int bestACtion = EpsilonGreedyAction(2, temporaryBoard.Grid); //take the best action by choosing epsilon over > 1
                        // Max Q(s',a')
                        double maxQvalueNextState = GetQValue(temporaryBoard.Grid, bestACtion);
                        //                                        Q(a,s)    + alpha * (gamma * Max(Q(a',s))       - Q(s,a)                                  
                        SetQValue(gameEngine.Board.Grid, action, currentVal + alpha * (gamma * maxQvalueNextState - currentVal));



                        //now take  epsilion greedy move to a new state. We can do so becuase we have already checked that they are not terminal.
                        gameEngine.MakeMove(action);
                        gameEngine.MakeMove(opponentAction);

                        action = EpsilonGreedyAction(epsilon, gameEngine.Board.Grid);
            
                    }
                }
            }


        }

    }

}
