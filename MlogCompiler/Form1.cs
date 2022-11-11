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
            List<string> lines = Compiler.GetCodeLines(textBox1.Text).Select(l => l.line).ToList();
            textBox2.Text = string.Join(Environment.NewLine, lines);
        }
    }
}