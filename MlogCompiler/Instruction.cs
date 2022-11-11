namespace MlogCompiler
{
    public class Instruction
    {
        public enum InstructionType
        {
            Null,
            Read, Write, Draw, Print,
            DrawFlush, PrintFlush, GetLink, Control, Radar, Sensor,
            Set, Op,
            End, Jump,
            UnitBind, UnitControl, UnitRadar, UnitLocate,
            Comment,
            ForLoop, WhileLoop, If
        }

        public InstructionType instructionType;
        public string[]? parameters;

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
