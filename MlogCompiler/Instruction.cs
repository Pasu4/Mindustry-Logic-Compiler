namespace MlogCompiler
{
    public class Instruction
    {
        public enum InstructionType
        {
            Null,
            Read, Write, Draw, Print,
            DrawFlush, PrintFlush, GetLink, Control, Radar, Sensor,
            Set, Op, Lookup, PackColor,
            Wait, Stop, End, Jump, Label,
            UnitBind, UnitControl, UnitRadar, UnitLocate,
            Comment, CompilerComment,
            ForLoop, WhileLoop,DoForLoop, DoWhileLoop, If
        }

        public InstructionType instructionType;
        public string[]? parameters;
        public List<int> labels = new List<int>();
        // public int line;

        public Instruction(InstructionType instructionType, string[]? parameters)
        {
            this.instructionType = instructionType;
            this.parameters = parameters;
        }

        public Instruction()
        {
            instructionType = InstructionType.Null;
            parameters = null;
        }
    }
}
