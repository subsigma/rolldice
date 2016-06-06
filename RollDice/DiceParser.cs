using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RollDice
{
    public struct RollResult
    {
        public string description;
        public List<int> results;
        public List<int> drops;
        public int rerolls;
        public int total;
        public int plus;
        public double? grandTotal;
    }

    public struct TraceItem
    {
        public string Item;
        public string Value;
        public RollResult? Roll;
    }

    public struct RollStats
    {
        public RollStats(string inputString)
        {
            DiceParser dp = new DiceParser();
            Results = new List<double>();

            Total = 0;
            Max = double.MinValue;
            Min = double.MaxValue;

            if(dp.GetResults(inputString).Count > 0)
            {
                for(int i = 0; i < 1000; i++)
                {
                    double result = dp.GetSimpleResult(inputString);

                    Total += result;

                    if (result > Max)
                    {
                        Max = result;
                    }

                    if (result < Min)
                    {
                        Min = result;
                    }

                    Results.Add(result);
                }

                Results.Sort();

                Median = Results.ElementAt(Results.Count / 2) + Results.ElementAt((Results.Count - 1) / 2);
                Median /= 2;

                Average = Total / 1000;

                Mode = Results.GroupBy(v => v)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;

                ResultString = "Statistics for 1000 rolls of: " + inputString + System.Environment.NewLine
                    + "Average | Median | Mode | Max | Min" + System.Environment.NewLine
                    + Average + " | " + Median + " | " + Mode + " | " + Max + " | " + Min;
            }
            else
            {
                throw new FormatException();
            }
        }

        public List<double> Results;
        public double Total;
        public double Max;
        public double Min;
        public double Average;
        public double Median;
        public double Mode;
        public string ResultString;
    }
    public class DiceParser
    {
        private enum AssociativityTypes { Left, Right, None }
        private struct Token
        {
            public string TokenString;
            public AssociativityTypes Associativity;
            public int Precedence;
            public int RequiredValues;
            public bool IsFunction;
        }

        private List<Token> TokenList = new List<Token>();

        private Random r = new Random();

        public DiceParser()
        {
            Token powToken = new Token
            {
                TokenString = "^",
                Associativity = AssociativityTypes.Right,
                Precedence = 4,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(powToken);
            Token plusToken = new Token
            {
                TokenString = "+",
                Associativity = AssociativityTypes.Left,
                Precedence = 2,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(plusToken);
            Token subToken = new Token
            {
                TokenString = "-",
                Associativity = AssociativityTypes.Left,
                Precedence = 2,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(subToken);
            Token multToken = new Token
            {
                TokenString = "*",
                Associativity = AssociativityTypes.Left,
                Precedence = 3,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(multToken);
            Token divToken = new Token
            {
                TokenString = "/",
                Associativity = AssociativityTypes.Left,
                Precedence = 3,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(divToken);
            Token rollDToken = new Token
            {
                TokenString = "d",
                Associativity = AssociativityTypes.Left,
                Precedence = 5,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(rollDToken);
            Token rollEToken = new Token
            {
                TokenString = "e",
                Associativity = AssociativityTypes.Left,
                Precedence = 5,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(rollEToken);
            Token rollFToken = new Token
            {
                TokenString = "f",
                Associativity = AssociativityTypes.Left,
                Precedence = 5,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(rollFToken);
            Token rollGToken = new Token
            {
                TokenString = "g",
                Associativity = AssociativityTypes.Left,
                Precedence = 5,
                RequiredValues = 2,
                IsFunction = false
            };
            TokenList.Add(rollGToken);
            Token rollXToken = new Token
            {
                TokenString = "x",
                Associativity = AssociativityTypes.Left,
                Precedence = 7,
                RequiredValues = 1,
                IsFunction = false
            };
            TokenList.Add(rollXToken);
            Token rollRToken = new Token
            {
                TokenString = "r",
                Associativity = AssociativityTypes.Left,
                Precedence = 6,
                RequiredValues = 1,
                IsFunction = false
            };
            TokenList.Add(rollRToken);
            Token rollPToken = new Token
            {
                TokenString = "p",
                Associativity = AssociativityTypes.Left,
                Precedence = 7,
                RequiredValues = 1,
                IsFunction = false
            };
            TokenList.Add(rollPToken);
            Token funCeiling = new Token
            {
                TokenString = "zCeiling",
                Associativity = AssociativityTypes.None,
                Precedence = 0,
                RequiredValues = 1,
                IsFunction = true
            };
            TokenList.Add(funCeiling);
        }

        private int GetPrecedence(string token)
        {
            return (TokenList.First(s => s.TokenString == token).Precedence);
        }

        private int GetReqValuesCount(string token)
        {
            return (TokenList.First(s => s.TokenString == token).RequiredValues);
        }

        private bool IsOpToken(string token)
        {
            return (TokenList.Any(s => s.TokenString == token));
        }

        private bool IsFunction(string token)
        {
            return (TokenList.Where(s => s.IsFunction == true).Any(t => t.TokenString == token));
        }

        private Token GetTokenFromString(string token)
        {
            return (TokenList.First(s => s.TokenString == token));
        }

        public double GetSimpleResult(string inputString)
        {
            foreach(TraceItem t in GetResults(inputString))
            {
                if(t.Roll.HasValue)
                {
                    if (t.Roll.Value.grandTotal.HasValue)
                    {
                        return (t.Roll.Value.grandTotal.GetValueOrDefault());
                    }
                }
            }
            throw new FormatException();
        }

        public string GetTrace(string inputString)
        {
            string detailedResults = "";

            foreach(TraceItem t in GetResults(inputString))
            {
                detailedResults += t.Item + " | " + t.Value;
                if (t.Roll.HasValue)
                {
                    if (t.Roll.Value.grandTotal == null)
                    {
                        detailedResults += " | " + t.Roll.Value.description + " | ";

                        foreach (int i in t.Roll.Value.results)
                        {
                            int iTotal = i + t.Roll.Value.plus;

                            if (t.Roll.Value.plus > 0)
                            {
                                detailedResults += iTotal + "[" + i + "+" + t.Roll.Value.plus + "] ";
                            }
                            else
                            {
                                detailedResults += i + " ";
                            }
                        }

                        foreach (int i in t.Roll.Value.drops)
                        {
                            detailedResults += "(" + i + ") ";
                        }

                        detailedResults = detailedResults.Trim();

                        detailedResults += " | " + "Total: " + t.Roll.Value.total;

                        if (t.Roll.Value.rerolls > 0)
                        {
                            detailedResults += " | " + "Rerolls: " + t.Roll.Value.rerolls;
                        }
                    }
                    else
                    {
                        detailedResults += " | " + "Grand Total: " + t.Roll.Value.grandTotal;
                    }
                }

                detailedResults += System.Environment.NewLine;
            }

            return detailedResults;
        }

        public string GetDetailedResults(string inputString)
        {
            string detailedResults = "";

            foreach (TraceItem t in GetResults(inputString))
            {
                if (t.Roll.HasValue)
                {
                    if (t.Roll.Value.grandTotal == null)
                    {
                        detailedResults += "Roll " + t.Roll.Value.description + ": ";

                        foreach (int i in t.Roll.Value.results)
                        {
                            int iTotal = i + t.Roll.Value.plus;

                            if(t.Roll.Value.plus > 0)
                            {
                                detailedResults += iTotal + "[" + i + "+" + t.Roll.Value.plus + "] ";
                            }
                            else
                            {
                                detailedResults += i + " ";
                            }
                        }

                        foreach (int i in t.Roll.Value.drops)
                        {
                            detailedResults += "(" + i + ") ";
                        }

                        detailedResults += " | Total: " + t.Roll.Value.total;

                        if (t.Roll.Value.rerolls > 0)
                        {
                            detailedResults += " | Total rerolls: " + t.Roll.Value.rerolls;
                        }

                        detailedResults += System.Environment.NewLine;
                    }
                    else
                    {
                        detailedResults += "Grand Total: " + t.Roll.Value.grandTotal;
                    }
                }
            }

            return (detailedResults);
        }

        public List<TraceItem> GetResults(string inputString)
        {
            List<TraceItem> results = new List<TraceItem>();
            LinkedList<string> RPN = GetRPN(inputString);
            Stack<string> TheStack = new Stack<string>();

            int x = 0;
            int p = 0;

            TraceItem tStart = new TraceItem();
            tStart.Item = "Postfix";
            tStart.Value = "";

            foreach(string s in RPN)
            {
                tStart.Value += s + " ";
            }

            tStart.Value = tStart.Value.Trim();
            results.Add(tStart);

            while(RPN.Count > 0)
            {
                TraceItem t = new TraceItem();

                string nextToken = RPN.First();
                RPN.RemoveFirst();

                double tryInt;

                if(double.TryParse(nextToken, out tryInt))
                {
                    TheStack.Push(nextToken);
                }
                else
                {
                    if (TheStack.Count < GetReqValuesCount(nextToken))
                    {
                        throw new FormatException();
                    }
                    else
                    {
                        t.Item = "";

                        Stack<string> rStack = new Stack<string>();

                        while(rStack.Count < GetReqValuesCount(nextToken))
                        {
                            rStack.Push(TheStack.Pop());
                        }

                        foreach(string s in rStack.Reverse())
                        {
                            t.Item += s + " ";
                        }

                        t.Item += nextToken;

                        switch(nextToken)
                        {
                            case "^":
                                TheStack.Push((Math.Pow(double.Parse(rStack.Pop()), double.Parse(rStack.Pop()))).ToString());
                                break;
                            case "+":
                                TheStack.Push((double.Parse(rStack.Pop()) + double.Parse(rStack.Pop())).ToString());
                                break;
                            case "-":
                                TheStack.Push((double.Parse(rStack.Pop()) - double.Parse(rStack.Pop())).ToString());
                                break;
                            case "*":
                                TheStack.Push((double.Parse(rStack.Pop()) * double.Parse(rStack.Pop())).ToString());
                                break;
                            case "/":
                                TheStack.Push((double.Parse(rStack.Pop()) / double.Parse(rStack.Pop())).ToString());
                                break;
                            case "d":
                            case "e":
                            case "f":
                            case "g":
                                int num = (int)Math.Ceiling(double.Parse(rStack.Pop()));
                                int type = (int)Math.Ceiling(double.Parse(rStack.Pop()));
                                t.Roll = (SimpleRoll(num, type, nextToken[0], x, p));
                                TheStack.Push((t.Roll.Value.total).ToString());
                                x = 0;
                                p = 0;
                                break;
                            case "x":
                                x = (int)Math.Ceiling(double.Parse(rStack.Pop()));
                                break;
                            case "r":
                                int r = (int)Math.Ceiling(double.Parse(rStack.Pop()));

                                string rollX = TheStack.Pop();
                                string rollY = TheStack.Pop();
                                string rollZ = RPN.First();
                                RPN.RemoveFirst();

                                int i;

                                for(i = 0; i < r; i++)
                                {
                                    RPN.AddFirst("+");
                                }

                                for (i = 0; i <= r; i++)
                                {
                                    if(x > 0)
                                    {
                                        RPN.AddFirst("x");
                                        RPN.AddFirst(x.ToString());
                                    }
                                    if(p > 0)
                                    {
                                        RPN.AddFirst("p");
                                        RPN.AddFirst(p.ToString());
                                    }
                                    RPN.AddFirst(rollZ);
                                    RPN.AddFirst(rollX);
                                    RPN.AddFirst(rollY);
                                }
                                break;
                            case "p":
                                p = (int)Math.Ceiling(double.Parse(rStack.Pop()));
                                break;
                            case "zCeiling":
                                TheStack.Push((Math.Ceiling(double.Parse(rStack.Pop()))).ToString());
                                break;
                        }

                        foreach(string s in TheStack)
                        {
                            t.Value += s + " ";
                        }

                        results.Add(t);
                    }
                }
            }

            if(TheStack.Count != 1)
            {
                throw new FormatException();
            }

            TraceItem tEnd = new TraceItem();
            tEnd.Item = "GT";
            tEnd.Value = "GT";
            tEnd.Roll = new RollResult { grandTotal = double.Parse(TheStack.Pop()) };
            results.Add(tEnd);

            return (results);
        }

        private LinkedList<string> GetRPN(string inputString)
        {
            TokenParser tokens = new TokenParser(inputString);
            LinkedList<string> Output = new LinkedList<string>();
            Stack<string> OperatorStack = new Stack<string>();

            for (string token = tokens.GetNextToken(); token != null; token = tokens.GetNextToken())
            {
                double throwaway;

                if (token.Length > 0)
                {
                    if (double.TryParse(token, out throwaway))
                    {
                        Output.AddLast(token);
                    }
                    else if (IsFunction(token))
                    {
                        OperatorStack.Push(token);
                    }
                    else if (token == ",")
                    {
                        while (OperatorStack.Peek() != "(")
                        {
                            if (OperatorStack.Count == 0)
                            {
                                throw new FormatException();
                            }
                            Output.AddLast(OperatorStack.Pop());
                        }
                    }
                    else if (IsOpToken(token))
                    {
                        if (OperatorStack.Count > 0)
                        {
                            while ((OperatorStack.Count > 0)
                                && (IsOpToken(OperatorStack.Peek()))
                                && (((GetTokenFromString(token).Associativity == AssociativityTypes.Left)
                                && (GetPrecedence(token) <= GetPrecedence(OperatorStack.Peek())))
                                || ((GetTokenFromString(token).Associativity == AssociativityTypes.Right)
                                && (GetPrecedence(token) < GetPrecedence(OperatorStack.Peek())))))
                            {
                                Output.AddLast(OperatorStack.Pop());
                            }
                        }
                        OperatorStack.Push(token);
                    }
                    else if (token == "(")
                    {
                        OperatorStack.Push(token);
                    }
                    else if (token == ")")
                    {
                        while (OperatorStack.Peek() != "(")
                        {
                            if (OperatorStack.Count == 0)
                            {
                                throw new FormatException();
                            }
                            Output.AddLast(OperatorStack.Pop());
                        }

                        OperatorStack.Pop();

                        if (OperatorStack.Count > 0)
                        {
                            if (IsFunction(OperatorStack.Peek()))
                            {
                                Output.AddLast(OperatorStack.Pop());
                            }
                        }
                    }
                }
            }
            while (OperatorStack.Count > 0)
            {
                if (OperatorStack.Peek() == "(")
                {
                    throw new FormatException();
                }
                else
                {
                    Output.AddLast(OperatorStack.Pop());
                }
            }

            return Output;
        }

        private RollResult SimpleRoll(int numDice, int typeDice, char rollType, int drop, int plus)
        {
            RollResult results = new RollResult();
            results.results = new List<int>();
            results.drops = new List<int>();
            results.description = numDice.ToString() + rollType + typeDice.ToString();

            if(drop > 0)
            {
                results.description += " (Dropping lowest " + drop + ")";
            }

            results.rerolls = 0;
            results.total = 0;
            results.plus = plus;

            if((numDice > 0) && (typeDice > 1) && (numDice < 1001) && (drop <= numDice))
            {
                if ((rollType == 'd') || (rollType == 'e') || (rollType == 'f') || (rollType == 'g'))
                {
                    for(int i = 0; i < numDice; i++)
                    {
                        results.results.Add(r.Next(1, typeDice + 1));

                        if(rollType == 'e')
                        {
                            while(results.results.Last() == typeDice)
                            {
                                results.rerolls++;
                                results.results.Add(r.Next(1, typeDice + 1));
                            }
                        }
                        else if(rollType == 'f')
                        {
                            while (results.results.Last() == 1)
                            {
                                results.rerolls++;
                                results.results.Add(r.Next(1, typeDice + 1));
                            }
                        }
                        else if((rollType == 'g') && (typeDice > 2))
                        {
                            while ((results.results.Last() == typeDice) || (results.results.Last() == 1))
                            {
                                results.rerolls++;
                                results.results.Add(r.Next(1, typeDice + 1));
                            }
                        }
                    }

                    if(drop > 0)
                    {
                        results.results = results.results.OrderBy(x => x).ToList();

                        results.drops = results.results.Take(drop).ToList();

                        results.results = results.results.Skip(drop).ToList();
                    }

                    foreach(int roll in results.results)
                    {
                        results.total += roll + plus;
                    }

                    return (results);
                }
                else throw new FormatException();
            }
            else
            {
                throw new FormatException();
            }
        }
    }
}
