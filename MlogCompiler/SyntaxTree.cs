using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MlogCompiler
{
    public enum Instruction
    {
        Goto,
        // TODO
    }

    public class SyntaxTree
    {
        public SyntaxBranch root;


        public SyntaxTree(SyntaxBranch branch)
        {
            root = branch;
        }

        public SyntaxTree()
        {
            root = new SyntaxBranch();
        }
    }

    public class SyntaxBranch
    {
        public SyntaxBranch[] children;
        public Instruction instruction;

        public SyntaxBranch()
        {
            children = new SyntaxBranch[0];
        }

        public SyntaxBranch(SyntaxBranch[] children, Instruction instruction)
        {
            this.children = children;
            this.instruction = instruction;
        }
    }
}
