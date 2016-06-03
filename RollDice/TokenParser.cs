using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RollDice
{
    public class TokenParser
    {
        public char[] Operands = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };
        public char[] Operators = { '^', '+', '-', '*', '/', '(', ')', 'd', 'e', 'f', 'g', 'x', 'r', ',', 'z', 'p' };

        string Input;
        int Position = 0;
        string[] cleanTokens;
        public TokenParser(string input)
        {
            Input = input;
        }
        
        public string GetNextToken()
        {
            if(Input.Trim().Contains(' '))
            {
                return (GetNextTokenClean());
            }
            else
            {
                return (GetNextTokenDirty());
            }
        }

        private string GetNextTokenClean()
        {
            if(cleanTokens == null)
            {
                cleanTokens = Input.Trim().Split();
            }

            if(cleanTokens.Length > Position)
            {
                Position++;
                return (cleanTokens[Position-1]);
            }
            else
            {
                return (null);
            }
        }

        private string GetNextTokenDirty()
        {
            if(Input.Length > Position)
            {
                if (Operands.Contains(Input[Position]) || Operators.Contains(Input[Position]))
                {
                    string returnString = Input[Position].ToString();
                    Position++;

                    if (!Operators.Contains(returnString[0]))
                    {
                        for (int i = Position; i < Input.Length; i++)
                        {
                            if (Operators.Contains(Input[i]))
                            {
                                break;
                            }
                            else if (Operands.Contains(Input[i]))
                            {
                                returnString += Input[i];
                                Position++;
                            }
                        }
                    }
                    else if (returnString[0] == 'z')
                    {
                        while(Input[Position] != '(')
                        {
                            returnString += Input[Position];
                            Position++;
                        }
                    }

                    return (returnString);
                }
                else
                {
                    return (null);
                }
            }
            else
            {
                return (null);
            }
        }
    }
}
