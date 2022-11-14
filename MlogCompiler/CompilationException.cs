using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MlogCompiler
{
    public class CompilationException : Exception
    {
        public CompilationException(string message) : base(message) { }
        public CompilationException(string message, int line) : base($"Line {line}: {message}") { }
    }
}
