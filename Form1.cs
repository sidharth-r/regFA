using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Drawing;

namespace regexFA
{
    public partial class rfaForm : Form
    {

        GViewer gViewer;
        Graph graphFA;

        public rfaForm()
        {
            InitializeComponent();
        }

        private void rfaForm_Load(object sender, EventArgs e)
        {
            gViewer = new GViewer();
            graphFA = new Graph("GraphFA");

            gViewer.Graph = graphFA;

            this.SuspendLayout();
            gViewer.Dock = DockStyle.Fill;
            gViewer.EdgeInsertButtonVisible = false;
            gViewer.LayoutEditingEnabled = false;
            splitContainer1.Panel1.Controls.Add(gViewer);
            //this.Controls.Add(gViewer);
            this.ResumeLayout();
        }

        private void buttonDraw_Click(object sender, EventArgs e)
        {
            String regex = textIn.Text;

            procRegex(regex);
        }

        private void procRegex(String regex)
        {
            regex = toRPN(regex);
            if (regex == "")
            {
                showInvStr();
                return;
            }                

            //int nPara = 0;
            StringBuilder sub = new StringBuilder();
            bool gotOpr = false, gotOp = false;
            char c;

            for (int i = regex.Length - 1; i > 0; i--)
            {
                c = regex[i];

               
            }
        }

        private String toRPN(String regex)
        {
            Stack<char> rpnStack = new Stack<char>();

            int nPara = 0;
            StringBuilder sb = new StringBuilder();

            foreach(char c in regex)
            {
                int p = tokenPrecedence(c);

                if (c == '(')
                {
                    nPara++;
                    rpnStack.Push(c);
                }
                else if (c == ')')
                {
                    nPara--;
                    char t;
                    do
                    {
                        t = rpnStack.Pop();
                        if (t != '(')
                            sb.Append(t);

                    } while (t != '(');
                }
                else if (p > 0)
                {
                    if(rpnStack.Count > 0)
                    {
                        while (p < tokenPrecedence(rpnStack.Peek()))
                        {
                            sb.Append(rpnStack.Pop());
                            if (rpnStack.Count == 0)
                                break;
                        }
                        
                    }                    
                    rpnStack.Push(c);
                }
                else
                    sb.Append(c);
            }

            while (rpnStack.Count > 0)
                sb.Append(rpnStack.Pop());

            if(nPara > 0)
            {
                return "";
            }

            return sb.ToString();
        }

        private void showInvStr()
        {
            MessageBox.Show("Invalid regex", "Error", MessageBoxButtons.OK);
        }

        private int tokenPrecedence(char c)
        {
            if (c == '(' || c == ')')
                return 4;
            else if (c == '*')
                return 3;
            else if (c == '.')
                return 2;
            else if (c == '+')
                return 1;
            else
                return 0;
        }
    }
}
