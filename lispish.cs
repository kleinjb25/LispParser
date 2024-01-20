/*
Justin Klein 2023
This program tokenizes user-inputted lisp code and writes it into a parse tree,
barring that the code is syntatically correct.
Lisp Grammar:
<Program> ::= {<SExpr>} 
<SExpr> ::= <Atom> | <List> 
<List> ::= () | ( <Seq> ) 
<Seq> ::= <SExpr> <Seq> | <SExpr> 
<Atom> ::= ID | INT | REAL | STRING
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ExprParser
{
    public enum Symbols{
        REAL, ID, INT, STRING, LITERAL,
        Program, SExpr, List, Seq, Atom 
    }
    
    List<Node> Tokens;
    int TokenIndex;
    ExprParser(IEnumerable<Node> nodes) {
        Tokens = new List<Node>(nodes);
        TokenIndex = 0;
    }

    Node Token {
        get {
            return Tokens[TokenIndex];
        }
    }
    
    Node NextToken() {
        Node result = Token;
        TokenIndex++;
        return result;
    }

    // <Program> ::= {<SExpr>}
    //for each separate lisp statement, parse the SExpr
    public Node ParseProgram() {
        var program = new Node(Symbols.Program);
        while (TokenIndex < Tokens.Count)
        {
            program.children.Add(ParseSExpr());
        }
        return program;
    }

    // <SExpr> ::= <Atom> | <List> 
    public Node ParseSExpr() {
        var sexpr = new Node(Symbols.SExpr);
        if (Token.Symbol == Symbols.LITERAL && Token.Text == "(")
        {
            sexpr.children.Add(ParseList());
        } 
        else
        {
            sexpr.children.Add(ParseAtom());
        }
        return sexpr;
    }

    // <List> ::= () | ( <Seq> ) 
    public Node ParseList() {
        var list = new Node(Symbols.List);
        if (Token.Text == "(")
        {
            list.children.Add(NextToken()); // Add "("
            list.children.Add(ParseSeq());
            list.children.Add(NextToken()); // Add ")"
        }
        else
        {
            throw new Exception("ParseList error");
        }
        return list;
    }

    // <Seq> ::= <SExpr> <Seq> | <SExpr> 
    public Node ParseSeq() {
        
        var seq = new Node(Symbols.Seq);
        seq.children.Add(ParseSExpr());
        while (Token.Symbol != Symbols.LITERAL && Token.Text != ")")
        {
            seq.children.Add(ParseSeq());
            if (Token.Symbol == Symbols.LITERAL && Token.Text == "(")
            {
                seq.children.Add(ParseSeq());
            }
        }
        return seq;
    }

    // <Atom> ::= ID | INT | REAL | STRING
    public Node ParseAtom() {
        
        if (Token.Symbol == Symbols.ID || Token.Symbol == Symbols.INT || Token.Symbol == Symbols.REAL || Token.Symbol == Symbols.STRING)
        {
            return new Node(Symbols.Atom, NextToken());
        }
        else if (Token.Symbol == Symbols.LITERAL && Token.Text == ")")
        {
            // not in the definition of an atom, but rather just to handle ) tokens,
            // since they aren't handled anywhere else
            return new Node(Symbols.LITERAL, NextToken().Text);
        }
        else
        {
            throw new Exception("ParseAtom error");
        }
    }

    public class Node
    {
        public Symbols Symbol;
        public string Text = "";

        public List<Node> children = new List<Node>();
        public Node(Symbols symbol, string text){
            this.Symbol = symbol;
            this.Text = text;
        }
        public Node(Symbols symbol, params Node[] children){
            this.Symbol = symbol;
            this.Text = "";
            this.children = new List<Node>(children);
        }
        public void Print(string prefix = "")
        {
            Console.WriteLine($"{prefix+Symbol, -40} {Text}");  
            foreach(Node child in children) {
                child.Print(prefix + "  ");
            }
        }
    }

    public static List<Node> Tokenize(String src)
    {
        var result = new List<Node>();
        int pos = 0;
        Match m;
        var WS = new Regex(@"\G\s");
        var REAL = new Regex(@"\G[+-]?[0-9]*\.[0-9]+");
        var INT = new Regex(@"\G[+-]?[0-9]+");
        var STRING = new Regex(@"\G""(?>\\.|[^\\""])*""");
        var ID = new Regex(@"\G[^\s""()]+");
        var LITERAL = new Regex(@"\G[()+-]");
        while (pos < src.Length)
        {
            if ((m = WS.Match(src, pos)).Success)
            {
                pos += m.Length;
            }
            else if ((m = REAL.Match(src, pos)).Success)
            {
                result.Add(new Node(Symbols.REAL, m.Value));
                pos += m.Length;
            }
            else if ((m = INT.Match(src, pos)).Success)
            {
                result.Add(new Node(Symbols.INT, m.Value));
                pos += m.Length;
            }
            else if ((m = STRING.Match(src, pos)).Success)
            {
                result.Add(new Node(Symbols.STRING, m.Value));
                pos += m.Length;
            }
            else if ((m = ID.Match(src, pos)).Success)
            {
                result.Add(new Node(Symbols.ID, m.Value));
                pos += m.Length;
            }
            else if ((m = LITERAL.Match(src, pos)).Success)
            {
                result.Add(new Node(Symbols.LITERAL, m.Value));
                pos += m.Length;
            }
            else
            {
                throw new Exception("Lexer error");
            }
        }
        return result;
    }

    public static Node Parse(Node[] tokens)
    {
        var p = new ExprParser(tokens);
        var tree = p.ParseProgram();
        return tree;
    }
    
    private static void CheckString(string lispcode)
    {
        try
        {
            Console.WriteLine(new String('=', 50));
            Console.Write("Input: ");
            Console.WriteLine(lispcode);
            Console.WriteLine(new String('-', 50));

            Node[] tokens = Tokenize(lispcode).ToArray();

            Console.WriteLine("Tokens");
            Console.WriteLine(new String('-', 50));
            foreach (Node node in tokens)
            {
                Console.WriteLine($"{node.Symbol,-20}\t: {node.Text}");
            }
            Console.WriteLine(new String('-', 50));

            Node parseTree = Parse(tokens);

            Console.WriteLine("Parse Tree");
            Console.WriteLine(new String('-', 50));
            parseTree.Print();
            Console.WriteLine(new String('-', 50));
        }
        catch (Exception)
        {
            Console.WriteLine("Threw an exception on invalid input.");
        }
    }


    public static void Main(string[] args)
    {
         // Put the lisp code you want to parse in the quotes.
         // You can have multiple lines of lisp in one checkstring.
         CheckString(@"(+ 3.14 (* 4 7))");

         // This was for the original assignment in which a makefile was used to make sure the test cases the professor gave passed.
         // CheckString(Console.In.ReadToEnd());
    }
}

