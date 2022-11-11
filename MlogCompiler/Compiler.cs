using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MlogCompiler.Instruction;

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
        static char[] parameterSeparators = { ',', '\t', '\r', '\n', ' ' };

        static int currentLabel;
        static List<int> nextLabels = new List<int> ();

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

        /// <summary>
        /// Returns an instruction given a code line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        static Instruction LineToInstruction(string line)
        {
            Instruction instruction = new Instruction();
            Regex firstWordRx = new Regex(@"^(\/\/\/?|\w.*?\b)"); // Find command or comment
            Match match = firstWordRx.Match(line);
            if(!match.Success && !line.StartsWith("//")) throw new ArgumentException("Critical error");

            bool hasParameters = true;

            switch(match.Value)
            {
                // I/O
                case "Read":
                    instruction.instructionType = InstructionType.Read;
                    break;
                case "Write":
                    instruction.instructionType = InstructionType.Write;
                    break;
                case "Draw":
                    instruction.instructionType = InstructionType.Draw;
                    break;
                case "Print":
                    instruction.instructionType = InstructionType.Print;
                    break;

                // Building control
                case "DrawFlush":
                    hasParameters = false;
                    instruction.instructionType = InstructionType.DrawFlush;
                    break;
                case "PrintFlush":
                    hasParameters = false;
                    instruction.instructionType = InstructionType.PrintFlush;
                    break;
                case "GetLink":
                    instruction.instructionType = InstructionType.GetLink;
                    break;
                case "Control":
                    instruction.instructionType = InstructionType.Control;
                    break;
                case "Radar":
                    instruction.instructionType = InstructionType.Radar;
                    break;
                case "Sensor":
                    instruction.instructionType = InstructionType.Sensor;
                    break;

                // Flow control
                case "End":
                    hasParameters = false;
                    instruction.instructionType = InstructionType.End;
                    break;
                case "Jump":
                    instruction.instructionType = InstructionType.Jump;
                    break;
                case "Label":
                    instruction.instructionType = InstructionType.Label;
                    break;

                // Unit control
                case "UnitBind":
                    instruction.instructionType = InstructionType.UnitBind;
                    break;
                case "UnitControl":
                    instruction.instructionType = InstructionType.UnitControl;
                    break;
                case "UnitRadar":
                    instruction.instructionType = InstructionType.UnitRadar;
                    break;
                case "UnitLocate":
                    instruction.instructionType = InstructionType.UnitLocate;
                    break;

                // Comments
                case "//":
                    instruction.instructionType = InstructionType.Comment;
                    break;
                case "///":
                    instruction.instructionType = InstructionType.CompilerComment;
                    break;

                // High-level flow control
                case "for":
                    instruction.instructionType = InstructionType.ForLoop;
                    break;
                case "while":
                    instruction.instructionType = InstructionType.WhileLoop;
                    break;
                case "if":
                    instruction.instructionType = InstructionType.If;
                    break;

                // Variables
                default:
                    // Interpret everything else as variable (set or op)
                    // TODO
                    break;
            }

            if(hasParameters)
            {
                // Write all parameters to instruction
                line = line.Remove(line.IndexOf(match.Value), match.Value.Length);
                Regex parameterRx = new Regex(@"("".+""|\b[\w\d]+\b)");

                if(match.Value.StartsWith("//")) // Add entire line as parameter for comments
                    instruction.parameters = new string[] { line };
                else
                    instruction.parameters = parameterRx.Matches(line).Select(m => m.Value).ToArray();
            }

            return instruction;
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
    }
}
