using static System.Windows.Forms.VisualStyles.VisualStyleElement;

#pragma warning disable IDE1006 // Auto-named methods
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
                statusLabel.Text = ex!.Message;
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

        private void importButton_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // string path = saveFileDialog1.FileName;
                Stream stream = openFileDialog1.OpenFile();
                StreamReader sr = new StreamReader(stream);
                textBox1.Text = sr.ReadToEnd();
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = openFileDialog1.FileName;
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream stream = saveFileDialog1.OpenFile();
                if(stream != null)
                {
                    StreamWriter sr = new StreamWriter(stream);
                    sr.Write(textBox2.Text);
                    stream.Close();
                }
            }
        }
    }
}
#pragma warning restore IDE1006