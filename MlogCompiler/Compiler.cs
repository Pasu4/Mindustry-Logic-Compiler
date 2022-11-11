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
        static string[] mathOps = { "+", "-", "*", "/", "%" , ""};

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
                branch.children = _children.ToArray();
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
            return ResolveJumps(ConvertBranch(tree.root));
        }

        public static List<string> ConvertBranch(SyntaxBranch branch)
        {
            List<string> lines = new List<string>();

            // Add opening code
            lines.AddRange(OpeningCode(branch.instruction, out string label));

            // Add code for all children
            if(branch.children != null)
                foreach(SyntaxBranch child in branch.children)
                    lines.AddRange(ConvertBranch(child));

            // Add closing code
            lines.AddRange(ClosingCode(branch.instruction, label));

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
            if(string.IsNullOrEmpty(line)) return new Instruction();

            Instruction instruction = new Instruction();
            Regex firstWordRx = new Regex(@"^(\/\/\/?|\w.*?\b)"); // Find command or comment
            Match match = firstWordRx.Match(line);
            if(!match.Success) throw new ArgumentException("Critical error");

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
                    instruction.instructionType = InstructionType.DrawFlush;
                    break;
                case "PrintFlush":
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
                case "Wait":
                    instruction.instructionType = InstructionType.Wait;
                    break;
                case "Stop":
                    hasParameters = false;
                    instruction.instructionType = InstructionType.Stop;
                    break;
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
                    if(line.Any(c => "+-*/".Contains(c))) // TODO Add all operations
                        instruction.instructionType = InstructionType.Op;
                    else
                        instruction.instructionType = InstructionType.Set;
                    break;
            }

            if(hasParameters)
            {
                // Write all parameters to instruction
                line = line.Remove(line.IndexOf(match.Value), match.Value.Length);
                Regex parameterRx = new Regex(@"("".+""|(\b|\B)[^\s,\(\)]+(\b|\B))");

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
        static List<string> OpeningCode(Instruction instruction, out string label)
        {
            List<string> lines = new List<string>();
            string[]? parameters = instruction.parameters;
            label = "";

            switch(instruction.instructionType)
            {
                case InstructionType.Null:
                    goto default;
                case Instruction.InstructionType.Read:
                    lines.Add($"read {parameters[0]} {parameters[1]} {parameters[2]}");
                    return lines;
                case InstructionType.Write:
                    lines.Add($"write {parameters[0]} {parameters[1]} {parameters[2]}");
                    return lines;
                case InstructionType.Draw:
                    break;
                case InstructionType.Print:
                    lines.Add($"print {parameters[0]}");
                    return lines;
                case InstructionType.DrawFlush:
                    break;
                case InstructionType.PrintFlush:
                    lines.Add($"printflush {parameters[0]}");
                    return lines;
                case InstructionType.GetLink:
                    break;
                case InstructionType.Control:
                    break;
                case InstructionType.Radar:
                    break;
                case InstructionType.Sensor:
                    break;
                case InstructionType.Set:
                    break;
                case InstructionType.Op:
                    break;
                case InstructionType.Lookup:
                    break;
                case InstructionType.PackColor:
                    break;
                case InstructionType.Wait:
                    break;
                case InstructionType.Stop:
                    break;
                case InstructionType.End:
                    break;
                case InstructionType.Jump:
                    break;
                case InstructionType.Label:
                    break;
                case InstructionType.UnitBind:
                    break;
                case InstructionType.UnitControl:
                    break;
                case InstructionType.UnitRadar:
                    break;
                case InstructionType.UnitLocate:
                    break;
                case InstructionType.Comment:
                    break;
                case InstructionType.CompilerComment:
                    break;
                case InstructionType.ForLoop:
                    lines.Add($"set {parameters[0]} {parameters[1]}\n");
                    lines.Add($"label __for{currentLabel}");

                    label = $"__for{currentLabel}";
                    currentLabel++;
                    return lines;
                case InstructionType.WhileLoop:
                    break;
                case InstructionType.If:
                    break;
                default:
                    return lines;
            }

            throw new CompilationException("Instruction is not implemented: " + instruction.instructionType);
        }

        /// <summary>
        /// Returns the last part of mlog code for an instruction. Only used if the instruction has a scope.
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        static List<string> ClosingCode(Instruction instruction, string label)
        {
            List<string> lines = new List<string>();
            string[] parameters = instruction.parameters;

            switch(instruction.instructionType)
            {
                case InstructionType.ForLoop:
                    bool ascent = int.Parse(parameters[1]) < int.Parse(parameters[2]);
                    lines.Add($"op {(ascent ? "add" : "sub")} {parameters[0]} {parameters[0]} 1");
                    lines.Add($"jump {label} {(ascent ? "lessThan" : "greaterThan")} {parameters[0]} {parameters[2]}");
                    return lines;
                case InstructionType.WhileLoop:
                    break;
                case InstructionType.If:
                    break;
                default:
                    return lines;
            }

            throw new CompilationException("Instruction is not implemented: " + instruction.instructionType);
        }

        static List<string> ResolveJumps(List<string> _code)
        {
            List<string> code = new List<string>(_code); // Shallow copy
            Dictionary<string, int> jumps = new Dictionary<string, int>();
            int i = 0;
            while(i < code.Count)
            {
                if(code[i].StartsWith("label "))
                {
                    jumps.Add(code[i].Substring(6), i); // Cut off "label "
                    code.RemoveAt(i);
                    continue;
                }
                i++;
            }

            for(i = 0; i < code.Count; i++)
            {
                if(code[i].StartsWith("jump "))
                {
                    string last = code[i].Substring(5); // Cut off "goto "
                    string label = last;
                    if(label.Contains(' '))
                    {
                        label = label.Remove(label.IndexOf(' ')); // Remove parameters
                        last = last.Substring(last.IndexOf(' '));
                    }
                    else
                        last = "";

                    if(!jumps.ContainsKey(label))
                        throw new CompilationException($"There is no label called \"{label}\"");
                    code[i] = code[i].Remove(5) + jumps[label] + last;
                }
            }

            return code;
        }
    }
}
