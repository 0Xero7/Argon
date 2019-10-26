using System;
using System.Collections.Generic;
using System.Text;
using Models;

namespace TokenStream
{
    public class ArgonTokenStream
    {
        public List<Token> tokens { get; private set; }
        int ptr = 0;
        Stack<int> sliceStartIndices;

        public ArgonTokenStream()
        {
            tokens = new List<Token>();
            sliceStartIndices = new Stack<int>();
            ptr = 0;
        }

        public ArgonTokenStream(List<Token> arg)
        {
            tokens = arg;
            ptr = 0;
            sliceStartIndices = new Stack<int>();
        }

        public bool IsAtStart() => ptr == 0;
        public bool IsAtEnd() => ptr >= tokens.Count;

        public void AddToken(Token arg)
        { tokens.Add(arg); }

        public Token CurrentToken
        {
            get => Peek();
        }
        public Token NextToken
        {
            get => PeekAhead(1);
        }
        public Token PreviousToken
        {
            get => PeekBehind(1);
        }

        public Token BarfToken()
        {
            if (ptr <= 0)
                throw new IndexOutOfRangeException("Trying to barf before any token has been consumed.");

            return tokens[--ptr];
        }

        public Token ConsumeToken()
        {
            if (ptr >= tokens.Count)
                return null;

            return tokens[ptr++];
        }

        public Token ConsumeToNext()
        {
            if (ptr >= tokens.Count)
                throw new IndexOutOfRangeException("Trying to consume token after token stream has ended.");

            return tokens[++ptr];
        }

        public Token Peek()
        {
            if (ptr >= tokens.Count)
                throw new IndexOutOfRangeException("Trying to peek token after token stream has ended.");

            return tokens[ptr];
        }

        public Token PeekNext() => PeekAhead(1);
        public Token PeekBehind() => PeekBehind(1);

        public Token PeekAhead(int offset)
        {
            if ((ptr + offset) >= tokens.Count)
                throw new IndexOutOfRangeException("Trying to consume token after token stream has ended.");

            return tokens[(ptr + offset)];
        }

        public Token PeekBehind(int offset)
        {
            if ((ptr - offset) < 0)
                return null;// throw new IndexOutOfRangeException($"Cannot peek token before index 0. Your index was {ptr + offset}.");

            return tokens[(ptr - offset)];
        }

        public ArgonTokenStream Slice(int from, int length)
        {
            return new ArgonTokenStream(tokens.GetRange(from, length));
        }

        public void StartSlice(int offset = 0)
        {
            sliceStartIndices.Push(ptr + offset);
        }

        public ArgonTokenStream FinishSlice(int offset = 0)
        {
            return Slice(sliceStartIndices.Peek(), ptr - sliceStartIndices.Pop() - offset);
        }
    }
}
