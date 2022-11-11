using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MlogCompiler
{
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
        public SyntaxBranch? parent;
        public SyntaxBranch[]? children;
        public Instruction instruction;

        public SyntaxBranch()
        {
            parent = null;
            children = null;
            instruction = new Instruction();
        }

        public SyntaxBranch(SyntaxBranch? parent)
        {
            this.parent = parent;
            children = null;
            instruction = new Instruction();
        }

        public SyntaxBranch(SyntaxBranch? parent, SyntaxBranch[] children, Instruction instruction)
        {
            this.parent = parent;
            this.children = children;
            this.instruction = instruction;
        }
    }
}
