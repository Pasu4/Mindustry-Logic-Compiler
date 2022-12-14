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
        UseStack = 1
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
            // Remove compiler options
            code = string
                .Join(Environment.NewLine, code
                    .Split(Environment.NewLine)
                    .Where(l => !l.StartsWith('#'))
                    .ToArray());

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
        public static List<string> ConvertTree(SyntaxTree tree, CompilerOptions options)
        {
            List<string> result = ConvertBranch(tree.root, options);
            return ResolveJumps(result);
        }

        public static List<string> ConvertBranch(SyntaxBranch branch, CompilerOptions options)
        {
            List<string> lines = new List<string>();

            // Add opening code
            lines.AddRange(OpeningCode(branch.instruction, out string[]? labels, options));

            // Add code for all children
            if(branch.children != null)
                foreach(SyntaxBranch child in branch.children)
                    lines.AddRange(ConvertBranch(child, options));

            // Add closing code
            lines.AddRange(ClosingCode(branch.instruction, labels, options));

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
            int depth = 0; // Count of scopes
            bool quoted = false;
            List<CodeLine> lines = new List<CodeLine>();
            for(; index < code.Length; index++)
            {
                // Also enters empty strings into the list, omits the semicolon, only in the current scope
                if(code[index] == ';' && depth == 0 && !quoted)
                {
                    lines.Add(new CodeLine(code.Substring(0, index)));
                    code = code.Substring(index + 1);
                    index = 0;
                    continue;
                }
                // Start tracking the scope
                if(code[index] == '{' && !quoted)
                {
                    /*int lastBracket = code.LastIndexOf('}');
                    lines.Add(new CodeLine(code.Substring(0, lastBracket + 1), true)); // Include last bracket
                    code = code.Substring(lastBracket + 1);
                    index = 0;
                    continue;*/
                    depth++;
                }
                // Scope closing
                if(code[index] == '}' && !quoted)
                {
                    // Only close scope if the brackets match
                    depth--;
                    if(depth == 0) // Scope was closed
                    {
                        lines.Add(new CodeLine(code.Substring(0, index + 1), true));
                        code = code.Substring(index + 1);
                        index = 0;
                        continue;
                    }
                }
                // Quote / Unquote (Finally a use for XOR)
                quoted ^= code[index] == '"';
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
            Regex firstWordRx = new Regex(@"^(\/\/\/?|@?\w.*?\b)"); // Find command or comment
            Regex assignmentRx = new Regex(@"^(\w[\w\d-]*?) = (\w.*?\b(?=\())"); // Method assignment
            Regex callRx = new Regex(@"^(\w[\w\d-]*?)\.(\w.*?\b(?=\())"); // Match method call with .Method()

            Match match = firstWordRx.Match(line);
            Match assignmentMatch = assignmentRx.Match(line);
            Match callMatch = callRx.Match(line);

            // Check if the line is an assignment
            bool isAssignment = assignmentMatch.Success;
            bool isCall = callMatch.Success;

            if(!match.Success && !isAssignment && !isCall)
                throw new ArgumentException("Syntax error");

            bool hasParameters = true;
            bool special = false; // Wether to handle the operation seperately

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
            switch(method.ToLower())
            {
                // I/O
                case "read":
                    instruction.instructionType = InstructionType.Read;
                    break;
                case "write":
                    instruction.instructionType = InstructionType.Write;
                    mainIndex = 1;
                    break;
                case "draw":
                    instruction.instructionType = InstructionType.Draw;
                    break;
                case "print":
                    instruction.instructionType = InstructionType.Print;
                    break;

                // Building control
                case "drawflush":
                    instruction.instructionType = InstructionType.DrawFlush;
                    break;
                case "printflush":
                    instruction.instructionType = InstructionType.PrintFlush;
                    break;
                case "getlink":
                    instruction.instructionType = InstructionType.GetLink;
                    break;
                case "control":
                    instruction.instructionType = InstructionType.Control;
                    mainIndex = 1;
                    break;
                case "radar":
                    instruction.instructionType = InstructionType.Radar;
                    mainIndex = 6;
                    break;
                case "sensor":
                    instruction.instructionType = InstructionType.Sensor;
                    break;

                // Flow control
                case "wait":
                    instruction.instructionType = InstructionType.Wait;
                    break;
                case "stop":
                    hasParameters = false;
                    instruction.instructionType = InstructionType.Stop;
                    break;
                case "end":
                    hasParameters = false;
                    instruction.instructionType = InstructionType.End;
                    break;
                case "jump":
                    instruction.instructionType= InstructionType.Jump;
                    special = true;
                    break;
                case "label":
                    instruction.instructionType = InstructionType.Label;
                    break;
                case "sub":
                    instruction.instructionType = InstructionType.Sub;
                    break;
                case "return":
                    instruction.instructionType = InstructionType.Return;
                    break;

                // Unit control
                case "unitbind":
                case "ubind":
                    instruction.instructionType = InstructionType.UnitBind;
                    break;
                case "unitcontrol":
                case "ucontrol":
                    instruction.instructionType = InstructionType.UnitControl;
                    break;
                case "unitradar":
                case "uradar":
                    instruction.instructionType = InstructionType.UnitRadar;
                    break;
                case "unitlocate":
                case "ulocate":
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
                case "dofor":
                    instruction.instructionType = InstructionType.DoForLoop;
                    break;
                case "dowhile":
                    instruction.instructionType = InstructionType.DoWhileLoop;
                    break;


                // Explicit Set / Op (for mlog import)
                case "set":
                    instruction.instructionType = InstructionType.Set;
                    break;
                case "op":
                    instruction.instructionType = InstructionType.Op;
                    break;

                // Interpret everything else as variable (set or op)
                default:
                    special = true;
                    instruction.instructionType = ops.Any(op => line.Contains(op)) ? InstructionType.Op : InstructionType.Set;
                    break;
            }

            if(hasParameters)
            {
                Regex parameterRx = new Regex(@"("".+""|(\b|\B)[^\s,\(\)]+(\b|\B))"); // Match all parameters

                // Special handling for Op
                if(special && instruction.instructionType == InstructionType.Op)
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
                else if(special && instruction.instructionType == InstructionType.Set)
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
                else if(special && instruction.instructionType == InstructionType.Jump)
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
                        list.Insert(1, OpToMlog(op));
                        instruction.parameters = list.ToArray();
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
        static List<string> OpeningCode(Instruction instruction, out string[]? labels, CompilerOptions options)
        {
            List<string> lines = new List<string>();
            string[]? parameters = instruction.parameters;
            labels = null;
            // if(parameters is null) return lines;

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
                {InstructionType.Sub, "jump"},
                {InstructionType.UnitBind, "ubind"},
                {InstructionType.UnitControl, "ucontrol"},
                {InstructionType.UnitRadar, "uradar"},
                {InstructionType.UnitLocate, "ulocate"}
            };

            try
            {
                switch(instruction.instructionType)
                {
#nullable disable
                    case InstructionType.Null:
                        return lines;
                    case InstructionType.CompilerComment:
                        goto case InstructionType.Null;
                    case InstructionType.DoForLoop:
                        lines.Add($"set {parameters[0]} {parameters[1]}\n");
                        labels = new string[] { $"__for{currentLabel}" };
                        lines.Add("label " + labels[0]);
                        currentLabel++;
                        return lines;
                    case InstructionType.DoWhileLoop:
                        labels = new string[] { $"__while{currentLabel}" };
                        lines.Add("label " + labels[0]);
                        currentLabel++;
                        return lines;
                    case InstructionType.ForLoop:
                        if(parameters[0] != parameters[1]) // Optimization
                            lines.Add($"set {parameters[0]} {parameters[1]}\n");
                        labels = new string[] { $"__for{currentLabel}", $"__break{currentLabel}" };
                        lines.Add($"jump {labels[1]} greaterThanEq {parameters[0]} {parameters[2]}"); // Pre-test
                        lines.Add("label " + labels[0]);
                        currentLabel++;
                        return lines;
                    case InstructionType.WhileLoop:
                        labels = new string[] { $"__while{currentLabel}", $"__break{currentLabel}" };
                        lines.Add("label " + labels[0]);
                        lines.Add($"jump {labels[1]} {OpToMlog(InverseOp(parameters[1]))} {parameters[0]} {parameters[2]}"); // Pre-test
                        currentLabel++;
                        return lines;
                    case InstructionType.If:
                        if(parameters[1] == "===")
                        {
                            lines.Add($"op strictEqual __if {parameters[0]} {parameters[2]}");
                            labels = new string[] { $"__if{currentLabel}" };
                            lines.Add($"jump {labels[0]} notEqual __if true");
                        }
                        else
                        {
                            labels = new string[] { $"__if{currentLabel}" };
                            lines.Add($"jump {labels[0]} {OpToMlog(InverseOp(parameters[1]))} {parameters[0]} {parameters[2]}");
                        }
                        currentLabel++;
                        return lines;
                    case InstructionType.Sub:
                        if((options & CompilerOptions.UseStack) != 0)
                        {
                            lines.Add("op add __retAddr @counter 3");
                            lines.Add("write __retAddr cell1 __stack");
                            lines.Add("op add __stack __stack 1");
                        }
                        else
                            lines.Add("op add __retAddr @counter 1");
                        lines.Add($"jump {parameters[0]} always");
                        return lines;

                    case InstructionType.Return:
                        if((options & CompilerOptions.UseStack) != 0)
                        {
                            lines.Add("op sub __stack __stack 1");
                            lines.Add("read @counter cell1 __stack");
                        }
                        else
                            lines.Add("set @counter __retAddr");
                        return lines;

                    case InstructionType.Comment:
                        lines.AddRange(parameters[0].Split(Environment.NewLine).Select(l => "# " + l.Trim()));
                        return lines;

                    default:
                        string str = instructions[instruction.instructionType];
                        if(parameters is not null) str += " " + string.Join(' ', parameters);
                        lines.Add(str);
                        return lines;
#nullable restore
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
        static List<string> ClosingCode(Instruction instruction, string[]? labels, CompilerOptions options)
        {
            List<string> lines = new List<string>();
            string[]? parameters = instruction.parameters;
            // if(parameters is null) return lines;

            try
            {
                switch(instruction.instructionType)
                {
#nullable disable
                    case InstructionType.DoForLoop:
                        lines.Add($"op add {parameters[0]} {parameters[0]} 1");
                        lines.Add($"jump {labels[0]} lessThan {parameters[0]} {parameters[2]}");
                        return lines;
                    case InstructionType.DoWhileLoop:
                        lines.Add($"jump {labels[0]} {OpToMlog(parameters[1])} {parameters[0]} {parameters[2]}");
                        return lines;
                    case InstructionType.ForLoop:
                        // bool ascent = int.Parse(parameters[1]) < int.Parse(parameters[2]);
                        lines.Add($"op add {parameters[0]} {parameters[0]} 1");
                        lines.Add($"jump {labels[0]} lessThan {parameters[0]} {parameters[2]}");
                        lines.Add($"label {labels[1]}"); // To skip the for loop
                        return lines;
                    case InstructionType.WhileLoop:
                        lines.Add($"jump {labels[0]} {OpToMlog(parameters[1])} {parameters[0]} {parameters[2]}");
                        lines.Add($"label {labels[1]}"); // To skip the while loop
                        return lines;
                    case InstructionType.If:
                        lines.Add("label " + labels[0]);
                        return lines;
                    default:
                        return lines;
#nullable restore
                }
            }
            catch(IndexOutOfRangeException)
            {
                throw new CompilationException("Incorrect number of parameters");
            }

            throw new CompilationException("Instruction is not implemented: " + instruction.instructionType);
        }

        static List<string> ResolveJumps(List<string> _code)
        {
            List<string> code = new List<string>(_code); // Shallow copy

            if(code.Any(l => !l.StartsWith("#")) && code.Last(l => !l.StartsWith("#")).StartsWith("label")) // Resolve labels at the end
                code.Add("end");

            Dictionary<string, int> jumps = new Dictionary<string, int>();
            int i = 0;
            int l = 0;

            while(i < code.Count)
            {
                if(code[i].StartsWith("label "))
                {
                    try
                    {
                        jumps.Add(code[i].Substring(6), l); // Cut off "label "
                    }
                    catch(ArgumentException)
                    {
                        throw new CompilationException($"Label {code[i].Substring(6)} is declared more than once");
                    }
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
                    string last = code[i].Substring(5); // Cut off "jump "
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

        /// <summary>
        /// Returns the inverse operation
        /// </summary>
        /// <param name="op">The operation</param>
        /// <returns>The inverse of the operation</returns>
        /// <exception cref="CompilationException">The operation does not exist or is invalid for jumps</exception>
        public static string InverseOp(string op) => op switch
        {
            "==" => "!=",
            "!=" => "==",
            "<" => ">=",
            "<=" => ">",
            ">" => "<=",
            ">=" => "<",
            _ => throw new CompilationException("This operator is not available for jump statements")
        };

        public static CompilerOptions GetCompilerOptions(string code)
        {
            try
            {
                return code
                    .Split(Environment.NewLine)
                    .Where(s => s.StartsWith('#'))
                    .Select(s => (CompilerOptions) Enum.Parse(typeof(CompilerOptions), s.AsSpan(1))) // Performance
                    .ToArray()
                    .Aggregate(CompilerOptions.None, (combined, next) => combined |= next, o => o);
            }
            catch(ArgumentException)
            {
                throw new CompilationException("Could not resolve compiler options");
            }
        }
    }
}
