using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MlogCompiler
{
    public enum CompilerOptions
    {
        None = 0,
        UseStack = 1, // Required to use methods without WriteOutMethods
        UseLocals = 2,
        WriteOutMethods = 4
    }

    public static class Compiler
    {
        static char[] ignoredCharacters = { '\t', '\r', '\n' }; // All characters that are ignored by the compiler

        public static SyntaxTree CompileTree(string code, CompilerOptions options = CompilerOptions.None)
        {
            SyntaxTree tree = new SyntaxTree(CompileBranch(code, null, options));

            return tree;
        }

        public static SyntaxBranch CompileBranch(string code, SyntaxBranch? parent, CompilerOptions options = CompilerOptions.None)
        {
            SyntaxBranch branch = new SyntaxBranch();

            // Iterate all commands

            return branch;
        }

        /// <summary>
        /// Splits code into single code lines and scopes
        /// </summary>
        /// <param name="code">The code</param>
        /// <returns>A list of all scopes and single instructions</returns>
        public static List<CodeLine> GetCodeLines(string code)
        {
            int index = 0; // So i can be used outside the loop
            List<CodeLine> lines = new List<CodeLine>();
            for(; index < code.Length; index++)
            {
                // Also enters empty strings into the list, omits the semicolon
                if(code[index] == ';')
                {
                    lines.Add(new CodeLine(code.Substring(0, index)));
                    code = code.Substring(index + 1);
                    index = 0;
                    continue;
                }
                // Adds the entire scope to the list
                if(code[index] == '{')
                {
                    int lastBracket = code.LastIndexOf('}');
                    lines.Add(new CodeLine(code.Substring(0, lastBracket + 1), true)); // Include last bracket
                    code = code.Substring(lastBracket + 1);
                    index = 0;
                    continue;
                }
            }

            // Remove leading and trailing spaces
            lines = lines.Select(l => 
            {
                l.line = l.line.Trim().Trim(ignoredCharacters);
                return l;
            }).ToList();

            return lines;
        }
    }
}
