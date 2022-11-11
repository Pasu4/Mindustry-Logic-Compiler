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

        static int currentLabel

        /// <summary>
        /// Compiles a tree from code
        /// </summary>
        /// <param name="code">The code to compile</param>
        /// <param name="options">Compiler options</param>
        /// <returns>The new syntax tree</returns>
        public static SyntaxTree CompileTree(string code, CompilerOptions options = CompilerOptions.None)
        {
            SyntaxTree tree = new SyntaxTree(CompileBranch(new CodeLine("{" + code + "}", true), null, options)); // Treat all code as being in a scope

            return tree;
        }

        /// <summary>
        /// Compiles a branch of the syntax tree
        /// </summary>
        /// <param name="codeLine">Line of code or scope</param>
        /// <param name="parent">Parent branch</param>
        /// <param name="options">Compiler options</param>
        /// <returns>The new branch</returns>
        public static SyntaxBranch CompileBranch(CodeLine codeLine, SyntaxBranch? parent, CompilerOptions options = CompilerOptions.None)
        {
            SyntaxBranch branch = new SyntaxBranch(parent);

            if(codeLine.isScope)
            {
                // Compile child branches
                int fromIndex = codeLine.line.IndexOf('{');
                string control = codeLine.line.Substring(0, fromIndex);
                string scope = codeLine.line.Substring(fromIndex + 1, codeLine.line.Length - fromIndex - 2); // Exclude control and brackets

                branch.instruction = LineToInstruction(control);

                List<SyntaxBranch> _children = new List<SyntaxBranch>();
                foreach(CodeLine cl in GetCodeLines(scope))
                {
                    _children.Add(CompileBranch(cl, branch, options));
                }
            }
            else
            {
                branch.instruction = LineToInstruction(codeLine.line);
            }

            return branch;
        }

        /// <summary>
        /// Converts a syntax tree to Mindustry Logic code
        /// </summary>
        /// <param name="tree">The tree</param>
        /// <returns>Mindustry Logic code</returns>
        public static List<string> ConvertTree(SyntaxTree tree)
        {
            return ConvertBranch(tree.root);
        }

        public static List<string> ConvertBranch(SyntaxBranch branch)
        {
            List<string> lines = new List<string>();

            // Add opening code
            lines.AddRange(OpeningCode(branch.instruction));

            // Add code for all children
            if(branch.children != null)
                foreach(SyntaxBranch child in branch.children)
                    lines.AddRange(ConvertBranch(child));

            // Add closing code
            lines.AddRange(ClosingCode(branch.instruction));

            return lines;
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

        static Instruction LineToInstruction(string line)
        {
            return new Instruction(); // TODO
        }

        /// <summary>
        /// Returns the first part of mlog code for an instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        static List<string> OpeningCode(Instruction instruction)
        {
            List<string> lines = new List<string>();

            switch(instruction.instructionType)
            {
                case Instruction.InstructionType.Read:
                    break;
                case Instruction.InstructionType.Write:
                    break;
                case Instruction.InstructionType.Draw:
                    break;
                case Instruction.InstructionType.Print:
                    break;
                case Instruction.InstructionType.DrawFlush:
                    break;
                case Instruction.InstructionType.PrintFlush:
                    break;
                case Instruction.InstructionType.GetLink:
                    break;
                case Instruction.InstructionType.Control:
                    break;
                case Instruction.InstructionType.Radar:
                    break;
                case Instruction.InstructionType.Sensor:
                    break;
                case Instruction.InstructionType.Set:
                    break;
                case Instruction.InstructionType.Op:
                    break;
                case Instruction.InstructionType.End:
                    break;
                case Instruction.InstructionType.Jump:
                    break;
                case Instruction.InstructionType.UnitBind:
                    break;
                case Instruction.InstructionType.UnitControl:
                    break;
                case Instruction.InstructionType.UnitRadar:
                    break;
                case Instruction.InstructionType.UnitLocate:
                    break;
                case Instruction.InstructionType.Comment:
                    break;
                case Instruction.InstructionType.ForLoop:
                    break;
                case Instruction.InstructionType.WhileLoop:
                    break;
                case Instruction.InstructionType.If:
                    break;
                default:
                    return lines;
            }

            throw new NotImplementedException("Instruction is not implemented yet");
        }

        /// <summary>
        /// Returns the last part of mlog code for an instruction
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        static List<string> ClosingCode(Instruction instruction)
        {

        }
    }
}
