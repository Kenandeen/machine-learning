using System;
using System.Collections.Generic;
using System.Linq;

namespace TowerOfHanoi
{
    class StateGenerator
    {
        private string A, B, C;
        string currMove;
        int bIndex, cIndex;
        List<int> topDisks;

        public StateGenerator() {}

        public List<string> GenerateStates(int numberOfDisks)
        {
            List<string> _States = new List<string>();
            topDisks = new List<int>();

            string A = "1";

            for (int a = 2; a <= numberOfDisks; a++)
            {
                A += "-" + a.ToString();
            }

            GetNextStates(_States, string.Format("A{0}B0C0", A), true);

            return _States;
        }

        public void StateActionMapping(double[,] R, List<string> _States, int dim, int FinalStateIndex)
        {
            Dictionary<int, string> dctStates = new Dictionary<int, string>();
            List<string> nextStepStates = new List<string>();
            List<int> nextStepIndeces = new List<int>();

            for (int i = 0; i < _States.Count; i++)
                dctStates.Add(i, _States[i]);

            for (int i = 0; i < dctStates.Count; i++)
            {
                GetNextStates(nextStepStates, dctStates[i], false);

                for (int j = 0; j < nextStepStates.Count; j++)
                {
                    nextStepIndeces.Add(dctStates.FirstOrDefault(state => state.Value == nextStepStates[j]).Key);
                }

                for (int j = 0; j < nextStepIndeces.Count; j++)
                {
                    R[i, nextStepIndeces[j]] = nextStepIndeces[j] == FinalStateIndex ? 100 : 0;
                }

                nextStepStates.Clear();
                nextStepIndeces.Clear();
            }

            R[FinalStateIndex, FinalStateIndex] = 100;
        }

        private void GetNextStates(List<string> States, string state, bool recursive)
        {
            List<string> availableMoves = new List<string>();

            topDisks.Clear();
            availableMoves.Clear();

            bIndex = state.IndexOf('B');
            cIndex = state.IndexOf('C');

            A = state.Substring(1, bIndex - 1);
            B = state.Substring(bIndex + 1, cIndex - bIndex - 1);
            C = state.Substring(cIndex + 1, state.Length - cIndex - 1);

            if (A.IndexOf('-') == -1)
                topDisks.Add(Convert.ToInt32(A));
            else
                topDisks.Add(Convert.ToInt32(A.Substring(0, 1)));

            if (B.IndexOf('-') == -1)
                topDisks.Add(Convert.ToInt32(B));
            else
                topDisks.Add(Convert.ToInt32(B.Substring(0, 1)));

            if (C.IndexOf('-') == -1)
                topDisks.Add(Convert.ToInt32(C));
            else
                topDisks.Add(Convert.ToInt32(C.Substring(0, 1)));

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (topDisks[i] == 0 || topDisks[i] == topDisks[j]) continue;
                    if (topDisks[i] < topDisks[j] || topDisks[j] == 0)
                    {
                        availableMoves.Add(string.Format("{0}{1}", Convert.ToChar(i + 65), Convert.ToChar(j + 65)));
                    }
                }
                if (availableMoves.Count == 3) break;
            }

            foreach (string move in availableMoves)
            {
                bIndex = state.IndexOf('B');
                cIndex = state.IndexOf('C');

                A = state.Substring(1, bIndex - 1);
                B = state.Substring(bIndex + 1, cIndex - bIndex - 1);
                C = state.Substring(cIndex + 1, state.Length - cIndex - 1);

                char sourceRod = move[0];
                char destRod = move[1];

                char notChangedRod = 'B';

                string format = "";

                int diff = destRod - sourceRod;

                switch (Math.Abs(diff))
                {
                    case 2:
                        notChangedRod = 'B';
                        if (sourceRod == 'A')
                            format = "A{0}B{2}C{1}";
                        else
                            format = "A{1}B{2}C{0}";
                        break;
                    case 1:
                        if (destRod == 'B' && sourceRod == 'A')
                        {
                            notChangedRod = 'C';
                            format = "A{0}B{1}C{2}";
                        }
                        else if (destRod == 'B' && sourceRod == 'C')
                        {
                            notChangedRod = 'A';
                            format = "A{2}B{1}C{0}";
                        }

                        else if (destRod == 'C')
                        {
                            notChangedRod = 'A';
                            format = "A{2}B{0}C{1}";
                        }
                        else
                        {
                            notChangedRod = 'C';
                            format = "A{1}B{0}C{2}";
                        }
                        break;
                }

                string notChangedRodContent = RodContent(notChangedRod);

                string srcRodContent = RodContent(sourceRod);
                string destRodContent = RodContent(destRod);

                if (destRodContent == "0")
                {
                    destRodContent = srcRodContent.Substring(0, 1);
                }
                else
                {
                    destRodContent = string.Format("{0}-{1}", srcRodContent.Substring(0, 1), destRodContent);
                }

                if (srcRodContent.IndexOf('-') > -1)
                {
                    srcRodContent = srcRodContent.Substring(2, srcRodContent.Length - 2);
                }
                else
                {
                    srcRodContent = "0";
                }

                currMove = string.Format(format, srcRodContent, destRodContent, notChangedRodContent);
                if (!States.Contains(currMove))
                {
                    States.Add(currMove);
                    if (recursive)
                        GetNextStates(States, currMove, true);
                }
            }
        }

        private string RodContent(char rod)
        {
            switch (rod)
            {
                case 'A':
                    return A;
                case 'B':
                    return B;
                case 'C':
                    return C;
                default:
                    return "";
            }
        }
    }
}