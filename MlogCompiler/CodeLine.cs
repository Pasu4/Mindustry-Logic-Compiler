using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MlogCompiler
{
    public class CodeLine
    {
        public string line;
        public bool isScope;

        public CodeLine(string line = "", bool isScope = false)
        {
            this.line = line;
            this.isScope = isScope;
        }
    }
}
