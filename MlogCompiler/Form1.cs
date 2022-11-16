using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MlogCompiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void compileButton_Click(object sender, EventArgs e)
        {
            bool success = false;
            CompilationException? ex = null;

            statusLabel.ForeColor = SystemColors.ControlText;
            statusLabel.Text = "Compiling...";
            string str = "";

            await Task.Run(() => str = Compile(out success, out ex));

            if(success)
            {
                statusLabel.Text = "Done";
                textBox2.Text = str;
            }
            else
            {
                statusLabel.ForeColor = Color.Red;
                statusLabel.Text = ex.Message;
            }
        }

        private string Compile(out bool success, out CompilationException? exception)
        {
            exception = null;
            success = false;
            try
            {
                CompilerOptions options = Compiler.GetCompilerOptions(textBox1.Text);
                SyntaxTree tree = Compiler.CompileTree(textBox1.Text, options);
                List<string> lines = Compiler.ConvertTree(tree, options);
                success = true;
                return string.Join(Environment.NewLine, lines);
            }
            catch(CompilationException e)
            {
                exception = e;
                return "";
            }
        }
    }
}