﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MlogCompiler.Instruction;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
            Regex assignmentRx = new Regex(@"^(\w[\w\d]*?) = (\w.*?\b(?=\())"); // Method assignment
            Regex callRx = new Regex(@"^(\w[\w\d]*?)\.(\w.*?\b(?=\())"); // Match method call with .Method()

            Match match = firstWordRx.Match(line);
            Match assignmentMatch = assignmentRx.Match(line);
            Match callMatch = callRx.Match(line);

            // Check if the line is an assignment
            bool isAssignment = assignmentMatch.Success;
            bool isCall = callMatch.Success;

            if(!match.Success && !isAssignment && !isCall)
                throw new ArgumentException("Syntax error");

            bool hasParameters = true;
            bool isOp = false;

            string method = isAssignment ? assignmentMatch.Groups[2].Value : isCall ? callMatch.Groups[2].Value : match.Value;
            int mainIndex = 0; // Index where to place the main variable
            string[] ops = // Operation keywords
            {
                " + ", " - ", " * ", " / ", "//", " % ", " ^ ",
                " == ", " != ", " && ", " < ", " <= ", " > ", " >= ", " === ",
                " << ", " >> ", " | ", " & ", " xor ", " flip(",
                " max(", " min(", " angle(", " len(", " noise(", " abs(", " log(", " log10(", " floor(", " ceil(", " sqrt(", " rand(",
                " sin(", " cos(", " tan(", " asin(", " acos(", " atan(",
            };

            // Resolve main instruction
            switch(method)
            {
                // I/O
                case "Read":
                    instruction.instructionType = InstructionType.Read;
                    break;
                case "Write":
                    instruction.instructionType = InstructionType.Write;
                    mainIndex = 1;
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
                    mainIndex = 1;
                    break;
                case "Radar":
                    instruction.instructionType = InstructionType.Radar;
                    mainIndex = 6;
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
                case "jump":
                case "Jump":
                    instruction.instructionType = InstructionType.Jump;
                    break;
                case "label":
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

                // Interpret everything else as variable (set or op)
                default:
                    isOp = ops.Any(op => line.Contains(op));
                    instruction.instructionType = isOp ? InstructionType.Op : InstructionType.Set;
                    break;
            }

            if(hasParameters)
            {
                Regex parameterRx = new Regex(@"("".+""|(\b|\B)[^\s,\(\)]+(\b|\B))"); // Match all parameters

                // Special handling for Op
                if(instruction.instructionType == InstructionType.Op)
                {
                    string op = ops.First(o => line.Contains(o));
                    op = op.Trim(' ', '(');
                    string op2 = OpToMlog(op);
                    line = line
                        .Remove(line.IndexOf(op), op.Length)
                        .Remove(line.IndexOf('='), 1);

                    List<string> list = new List<string> { op2 };
                    list.AddRange(parameterRx.Matches(line).Select(m => m.Value));
                    instruction.parameters = list.ToArray();

                    return instruction;
                }
                // Special handling for Set
                else if(instruction.instructionType == InstructionType.Set)
                {
                    try
                    {
                        line = line.Remove(line.IndexOf('='), 1);
                        instruction.parameters = parameterRx.Matches(line).Select(m => m.Value).ToArray();

                        return instruction;
                    }
                    catch(ArgumentOutOfRangeException)
                    {
                        // There was no set operation
                        throw new CompilationException("Method not recognized");
                    }
                }
                // Special handling for Jump
                else if(instruction.instructionType == InstructionType.Jump)
                {
                    line = line.Substring(5); // Cut off "Jump"

                    bool conditional = line.Contains(" if ") || line.Contains(" if(");
                    if(conditional)
                    {
                        line = line.Replace(" if ", " ").Replace(" if(", " ");
                        instruction.parameters = parameterRx.Matches(line).Select(m => m.Value).ToArray();
                        string op = instruction.parameters[2];
                        List<string> list = instruction.parameters.ToList();
                        list.RemoveAt(2);
                        list.Insert(0, OpToMlog(op));
                    }
                    else
                    {
                        line += " always";
                        instruction.parameters = parameterRx.Matches(line).Select(m => m.Value).ToArray();
                    }

                    return instruction;
                }

                // Write all parameters to instruction
                if(isAssignment)
                {
                    Regex removeAssignmentRx = new Regex(@" = (\w.*?\b(?=\())"); // Match method name and " = " without bracket
                    Match match1 = removeAssignmentRx.Match(line);
                    line = line.Remove(line.IndexOf(match1.Value), match1.Value.Length);
                }
                else if(isCall)
                {
                    Regex removeCallRx = new Regex(@"\.(\w.*?\b(?=\())");
                    Match match1 = removeCallRx.Match(line);
                    line = line.Remove(line.IndexOf(match1.Value), match1.Value.Length);
                }
                else
                    line = line.Remove(line.IndexOf(match.Value), match.Value.Length);

                if(match.Value.StartsWith("//")) // Add entire line as parameter for comments
                    instruction.parameters = new string[] { line };
                else
                    instruction.parameters = parameterRx.Matches(line).Select(m => m.Value).ToArray();

                // Put main variable at correct index
                if(mainIndex != 0 && (isAssignment || isCall)) // Only if method is assigned or called on variable
                {
                    string mainVar = instruction.parameters[0];
                    List<string> list = instruction.parameters.Skip(1).ToList();
                    list.Insert(mainIndex, mainVar);
                    instruction.parameters = list.ToArray();
                }
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
            if(parameters is null) return lines;

            // Mlog code for each instruction type
            Dictionary<InstructionType, string> instructions = new Dictionary<InstructionType, string>()
            {
                {InstructionType.Read, "read"},
                {InstructionType.Write, "write"},
                {InstructionType.Draw, "draw"},
                {InstructionType.Print, "print"},
                {InstructionType.DrawFlush, "drawflush"},
                {InstructionType.PrintFlush, "printflush"},
                {InstructionType.GetLink, "getlink"},
                {InstructionType.Control, "control"},
                {InstructionType.Radar, "radar"},
                {InstructionType.Sensor, "sensor"},
                {InstructionType.Set, "set"},
                {InstructionType.Op, "op"},
                {InstructionType.Lookup, "lookup"},
                {InstructionType.PackColor, "packcolor"},
                {InstructionType.Wait, "wait"},
                {InstructionType.Stop, "stop"},
                {InstructionType.End, "end"},
                {InstructionType.Jump, "jump"},
                {InstructionType.Label, "label"},
                {InstructionType.UnitBind, "ubind"},
                {InstructionType.UnitControl, "ucontrol"},
                {InstructionType.UnitRadar, "uradar"},
                {InstructionType.UnitLocate, "ulocate"},
                {InstructionType.Comment, "#"},
            };

            try
            {
                switch(instruction.instructionType)
                {
                    case InstructionType.Null:
                        return lines;
                    case InstructionType.CompilerComment:
                        goto case InstructionType.Null;
                    case InstructionType.ForLoop:
                        lines.Add($"set {parameters[0]} {parameters[1]}\n");
                        label = $"__for{currentLabel}";
                        lines.Add("label " + label);
                        currentLabel++;
                        return lines;
                    case InstructionType.WhileLoop:
                        break;
                    case InstructionType.If:
                        break;
                    default:
                        lines.Add(instructions[instruction.instructionType] + " " + string.Join(' ', parameters));
                        return lines;
                }
            }
            catch(IndexOutOfRangeException)
            {
                throw new CompilationException("Incorrect number of parameters");
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
            string[]? parameters = instruction.parameters;
            if(parameters is null) return lines;

            switch(instruction.instructionType)
            {
                case InstructionType.ForLoop:
                    // bool ascent = int.Parse(parameters[1]) < int.Parse(parameters[2]);
                    lines.Add($"op add {parameters[0]} {parameters[0]} 1");
                    lines.Add($"jump {label} lessThan {parameters[0]} {parameters[2]}");
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
            int l = 0;
            while(i < code.Count)
            {
                if(code[i].StartsWith("label "))
                {
                    jumps.Add(code[i].Substring(6), l); // Cut off "label "
                    code.RemoveAt(i);
                    continue;
                }
                if(!code[i].StartsWith('#')) l++; // Don't count comments
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

        /// <summary>
        /// Converts an operator to an mlog operator
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        public static string OpToMlog(string op) => op switch
        {
            "+" => "add",
            "-" => "sub",
            "*" => "mul",
            "/" => "div",
            "//" => "idiv",
            "%" => "mod",
            "^" => "pow",
            "==" => "equal",
            "!=" => "notEqual",
            "&&" => "land",
            "<" => "lessThan",
            "<=" => "lessThanEq",
            ">" => "greaterThan",
            ">=" => "greaterThanEq",
            "===" => "strictEqual",
            "<<" => "shl",
            ">>" => "shr",
            "|" => "or",
            "&" => "and",
            "flip" => "not",
            _ => op
        };
    }
}
