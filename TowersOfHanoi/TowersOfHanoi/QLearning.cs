using System;
using System.Collections.Generic;

namespace TowerOfHanoi
{
    class QLearning
    {
        private double[,] R;
        private double[,] Q;

        private List<string> _States;
        
        public List<string> States
        {
            get
            {
                return _States;
            }
        }

        private TowerOfHanoi _Puzzle;
        private StateGenerator _Generator;
        private int FinalStateIndex;
        private int _NumberOfDisks;

        private List<int> optimalPath = new List<int>();

        public QLearning(TowerOfHanoi puzzle, int numberOfDisks)
        {
            _Puzzle = puzzle;
            _NumberOfDisks = numberOfDisks;

            _Generator = new StateGenerator();

            Init(_NumberOfDisks);
        }

        public void Init(int numberOfDisks)
        {
            _States = _Generator.GenerateStates(numberOfDisks);

            for (int i = 0; i < _States.Count; i++)
            {
                if (_States[i].StartsWith("A0B0"))
                {
                    FinalStateIndex = i;
                    break;
                }
            }

            int dim = Convert.ToInt32(Math.Pow(3, numberOfDisks));

            Learn(dim);
        }

        private void Learn(int dim)
        {
            InitQMatrix(dim);

            InitRMatrix(dim);

            //for (int i = 0; i < dim; i++)
            //{
            //    for (int j = 0; j < dim; j++)
            //        Console.Write(R[i, j].ToString("0.##") + " ");
            //    Console.WriteLine();
            //}

            TrainQMatrix(dim);

            NormalizeQMatrix(dim);

            Test(R, Q, dim);
        }

        private void InitQMatrix(int dim)
        {
            Q = new double[dim, dim];
        }

        private void InitRMatrix(int dim)
        {
            R = new double[dim, dim];

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                    R[i, j] = -1;
            }

            _Generator.StateActionMapping(R, _States, dim, FinalStateIndex);
        }

        private void TrainQMatrix(int dim)
        {
            // list of available moves (will be based on R matrix which
            // contains the allowed next moves as 0 values in the array)
            List<int> availableMoves = new List<int>();
            int nextStep = -1;

            int counter = 0;
            int init = -1;

            // dim is the number of all possible states of a puzzle
            // from my experience with this application, 4 times the number
            // of all possible moves has enough episodes to train Q matrix
            while (counter < 4 * dim)
            {
                init = Utility.GetRandomNumber(0, dim);

                do
                {
                    // get available actions
                    availableMoves = GetNextActions(dim, init);

                    // Choose any action out of the available actions randomly
                    nextStep = Utility.GetRandomNumber(0, availableMoves.Count);
                    nextStep = availableMoves[nextStep];

                    // get available actions
                    availableMoves = GetNextActions(dim, nextStep);

                    // this is the value iteration update rule
                    // discount factor is 0.8
                    Q[init, nextStep] = R[init, nextStep] + 0.8 * Utility.GetMax(Q, nextStep, availableMoves);

                    // set the next step as the current step
                    init = nextStep;
                }
                while (init != dim - 1);

                counter++;
            }
        }

        private List<int> GetNextActions(int dim, int init)
        {
            List<int> availableMoves = new List<int>();

            for (int i = 0; i < dim; i++)
            {
                // if the action i is availabe from the state init
                if (R[init, i] > -1)
                {
                    // add it to the available moves list
                    availableMoves.Add(i);

                    // the maximum number of availabe actions from any state is 3
                    // so when they reach 3, break to save computation time
                    if (availableMoves.Count == 3) break;
                }
            }

            return availableMoves;
        }

        private void NormalizeQMatrix(int dim)
        {
            double maxQ = 0;

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    if (Q[i, j] > maxQ)
                        maxQ = Q[i, j];
                }
            }

            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    Q[i, j] = Q[i, j] / maxQ * 100;
                }
            }
        }

        private void Test(double[,] R, double[,] Q, int dim)
        {
            string strStartingState = "";
            int start = 0;

            do
            {
                optimalPath.Clear();

                Console.WriteLine("Enter the starting state in format A#B#C#. For example, the starting state usually is A1-2-3B0C0\n(or type \"new\" to change the number of disks):");

                string A = "1";

                for (int a = 2; a <= _NumberOfDisks; a++)
                {
                    A += "-" + a.ToString();
                }

                try
                {
                    strStartingState = Console.ReadLine();
                }
                catch
                {
                    strStartingState = string.Format("A{0}B0C0", A);
                }

                if (strStartingState.ToLower() == "new")
                    _Puzzle.Init();

                for (int i = 0; i < dim; i++)
                {
                    if (strStartingState == States[i])
                    {
                        start = i;
                        break;
                    }
                }

                Implement(R, Q, start, dim);

                string strState = "";

                Console.WriteLine(string.Format("Optimal Solution: {0} Steps", optimalPath.Count - 1));
                for (int i = 0; i < optimalPath.Count; i++)
                {
                    strState = States[optimalPath[i]].Replace("A", "A ").Replace("B", "\t\t\tB ").Replace("C", "\t\t\tC ");
                    Console.WriteLine(strState);
                }
            } while (start >= 0 && start < dim);
        }

        private void Implement(double[,] R, double[,] Q, int start, int dim)
        {
            optimalPath.Add(start);

            if (start == FinalStateIndex) return;

            List<int> availableMoves = GetNextActions(dim, start);

            int maxQindex = 0;

            if (availableMoves.Count > 1)
            {
                maxQindex = availableMoves[0];

                if (Q[start, availableMoves[1]] > Q[start, availableMoves[0]])
                    maxQindex = availableMoves[1];

                if (availableMoves.Count > 2)
                {
                    if (Q[start, availableMoves[2]] > Q[start, availableMoves[1]])
                        maxQindex = availableMoves[2];
                }
            }

            Implement(R, Q, maxQindex, dim);
        }
    }
}