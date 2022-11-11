namespace MlogCompiler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void compileButton_Click(object sender, EventArgs e)
        {
            Compiler.CompileTree(textBox1.Text, CompilerOptions.None);
            List<string> lines = Compiler.ConvertTree(Compiler.CompileTree(textBox1.Text));
            textBox2.Text = string.Join(Environment.NewLine, lines);
        }
    }
}