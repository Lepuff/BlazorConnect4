using BlazorConnect4.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
            bool valid = AiGameEngine.IsValid(grid, action);

            while (!valid)
            {
                action = generator.Next(0, 7);
                valid = AiGameEngine.IsValid(grid, action);

            }

            return action;
        }
    }

    [Serializable]
    public class QAgent : AI
    {

        private Dictionary<String, double[]> Qdict;

        private CellColor PlayerColor;
        // Reward values
        private float InvalidMoveReward = -0.5F;
        private float WinningMoveReward = 1F;
        private float LosingMoveReward = -1F;
        private float DrawMoveReward = 0F;

        // Statistics
        private long wins = 0;
        private long losses = 0;
        private long ties = 0;
        private long playedGames = 0;





        public QAgent(CellColor playerColor)
        {

            if (playerColor == CellColor.Red)
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




        //Gets a Qvalue from the dictionary, if the state does not yet exist, fill it with random numbers.
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

        // Sets the Qvalues in the dictionary, if a state does not yet exist, fill it with random numbers. 
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


        //Select a epsilon greedy move, if the the move is not valid, randomize the move.
        public override int SelectMove(Cell[,] grid)
        {
            double epsilon = 0.9F;
            int action = EpsilonGreedyAction(epsilon, grid);
            bool validMove = AiGameEngine.IsValid(grid, action);
            // in the case that the best move is not a validmove, isntead randomize a move until a valid is found
            Random randomGen = new Random();
            while (!validMove)
            {

                action = randomGen.Next(0, 7);
                validMove = AiGameEngine.IsValid(grid, action);
            }
            Debug.Assert(AiGameEngine.IsValid(grid, action));
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
            bool validMove = AiGameEngine.IsValid(state, action);
            Random randomGen = new Random();
            while (!validMove)
            {

                action = randomGen.Next(0, 7);
                validMove = AiGameEngine.IsValid(state, action);
            }

            return action;
        }


        // Get an epsilon greedy move, 
        public int EpsilonGreedyAction(double epsilon, Cell[,] state)
        {
            Random random = new Random();
            int action = -1;
            if (random.NextDouble() < epsilon)
            {
                action = random.Next(0, 7);
                while (!AiGameEngine.IsValid(state, action))
                {
                    action = random.Next(0, 7);
                }

            }
            else
            {
                // Make highest valued move
                action = GetBestAction(state);
                while (!AiGameEngine.IsValid(state, action))
                {
                    action = random.Next(0, 7);
                }
            }
            Debug.Assert(AiGameEngine.IsValid(state, action));
            return action;
        }

        public static QAgent ConstructFromFile(string fileName)
        {
            QAgent temp = (QAgent)(AI.FromFile(fileName));
            // Eftersom generatorn inte var serialiserad.
            //temp.generator = new Random();
            return temp;
        }

        //Train the current ai versus another AI for a chosen numbers of iterations.
        public void Workout(AI opponentAi, int iterations)
        {

            double epsilon = 0.7F;
            float gamma = 0.9F;
            float alpha = 0.5F;

            AiGameEngine gameEngine = new AiGameEngine();
            int opponentAction;
            CellColor opponenentColor = AiGameEngine.OtherPlayer(PlayerColor);
            for (int i = 0; i < iterations; i++)
            {
                playedGames++;
                // new iteration, reset the gameBoard and playerturn
                gameEngine.Reset();
                bool terminal = false;

                if (PlayerColor == CellColor.Yellow)
                {
                    //opponentAction = oppositeAi.SelectMove(gameEngine.Board.Grid);
                    opponentAction = EpsilonGreedyAction(1, gameEngine.Board.Grid); //Select a random move the first time to make sure we exercise versus all possible starting moves.
                    gameEngine.MakeMove(opponentAction);
                }

                int action = EpsilonGreedyAction(epsilon, gameEngine.Board.Grid);
                Debug.Assert(AiGameEngine.IsValid(gameEngine.Board.Grid, action));

                while (!terminal)
                {

                    if (AiGameEngine.IsWin(gameEngine.Board, action, gameEngine.PlayerTurn))
                    {
                        SetQValue(gameEngine.Board.Grid, action, WinningMoveReward);
                        terminal = true;
                        wins++;
                    }
                    else if (AiGameEngine.IsDraw(gameEngine.Board, action))
                    {
                        SetQValue(gameEngine.Board.Grid, action, DrawMoveReward);
                        terminal = true;
                        ties++;
                    }
                    else
                    {
                        //Q(s,a)
                        double currentVal = GetQValue(gameEngine.Board.Grid, action);


                        GameBoard temporaryBoard = gameEngine.Board.Copy(); // make a non-reference copy of the gameboard.
                        //take action Q(s,a)
                        AiGameEngine.MakeMove(ref temporaryBoard, PlayerColor, action);

                        //let the opponent take a move
                        opponentAction = opponentAi.SelectMove(temporaryBoard.Grid);
                        //observe if the oppents move is terminal, in that case update the values for our move that led to that possibility
                        if (AiGameEngine.IsWin(temporaryBoard, opponentAction, opponenentColor))
                        {
                            SetQValue(gameEngine.Board.Grid, action, LosingMoveReward); //set the q values for the move that led to the opponents win
                            losses++;
                            break;
                        }
                        else if (AiGameEngine.IsDraw(temporaryBoard, opponentAction))
                        {
                            SetQValue(gameEngine.Board.Grid, action, DrawMoveReward); //set the q values for the move that led to opponents draw move.
                            ties++;
                            break;
                        }
                        //take the opponent move
                        AiGameEngine.MakeMove(ref temporaryBoard, opponenentColor, opponentAction);

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
