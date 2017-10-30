﻿using System;
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
            gViewer.LayoutAlgorithmSettingsButtonVisible = false;
            splitContainer1.Panel1.Controls.Add(gViewer);
            this.ResumeLayout();
        }

        private void buttonDraw_Click(object sender, EventArgs e)
        {
            String regex = textIn.Text;

            if (graphFA.NodeCount > 0)
                graphFA = new Graph("GraphFA");     //fix this
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

            BTreeNode root = new BTreeNode();
            regex = new string(regex.ToCharArray().Reverse().ToArray());
            root = makeTree(regex,root);

            drawTree(root);

            Node inv = new Node("init_node_inv");
            inv.Attr.Shape = Shape.Plaintext;
            inv.LabelText = "";
            graphFA.AddNode(inv);

            foreach(Node n in root.graph.nodes)
            {
                n.Attr.Shape = Shape.Circle;
                if (n == root.graph.anchorEnd)
                    n.Attr.Shape = Shape.DoubleCircle;
                graphFA.AddNode(n);
            }

            graphFA.AddEdge(inv.Id, root.graph.anchorStart.Id);

            foreach(Edge e in root.graph.edges)
            {
                
                graphFA.AddEdge(e.Source,e.LabelText,e.Target);
            }

            gViewer.Graph = null;
            graphFA.Attr.LayerDirection = LayerDirection.LR;
            gViewer.CalculateLayout(graphFA);
            gViewer.Graph = graphFA;
            gViewer.Refresh();
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
                        char t = rpnStack.Peek();
                        while (p < tokenPrecedence(t) && t != '(')
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

        private BTreeNode makeTree(string regex, BTreeNode root)
        {
            root.data = regex.First();
            regex = regex.Substring(1, regex.Length - 1);

            if (regex.Length == 0)
                return root;

            BTreeNode left = new BTreeNode();
            BTreeNode right = new BTreeNode();

            StringBuilder stbr = new StringBuilder();
            String strr, strl;

            int n = 1, i = -1;
            bool gotOpr = false;
            foreach (char c in regex)
            {
                i++;

                if(n == 0)
                {
                    break;
                }

                if(tokenPrecedence(c) == 0)
                {
                    gotOpr = true;
                    stbr.Append(c);
                    n--;
                }
                else if(!gotOpr)
                {
                    stbr.Append(c);
                    if(c != '*')
                        n++;
                }
                else
                {
                    showInvStr();
                    return null;
                }
            }

            if(root.data == '*')
            {
                strl = stbr.ToString();
                strr = null;
            }
            else
            {
                strr = stbr.ToString();
                strl = regex.Substring(i, regex.Length - i);
            }
            
            if(strl.Length > 1)
            {
                left = makeTree(strl, left);
            }
            else
            {
                left.data = strl.First();
                
            }
            root.left = left;

            if (strr == null)
            {
                right = null;
            }
            else if(strr.Length > 1)
            {
                right = makeTree(strr, right);
            }
            else
            {
                right.data = strr.First();
            }
            root.right = right;

            return root;
        }

        private void drawTree(BTreeNode root)
        {
            if (root == null)
                return;
            
            drawTree(root.left);
            drawTree(root.right);

            char c = root.data;
            if(c == '*')
            {
                root.graph = new GraphStar(root.left.graph);
            }
            else if(c == '+')
            {
                root.graph = new GraphPlus(root.left.graph, root.right.graph);
            }
            else if (c == '.')
            {
                root.graph = new GraphDot(root.left.graph, root.right.graph);
            }
            else
            {
                root.graph = new GraphSymbol(c);
            }
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

    public class BTreeNode
    {
        public char data;
        public BTreeNode left;
        public BTreeNode right;
        public GraphBase graph;

        public BTreeNode()
        {
            left = null;
            right = null;
        }

        public BTreeNode(char d)
        {
            data = d;
            left = null;
            right = null;
        }

        void addLeft(BTreeNode n)
        {
            left = n;
        }

        void addRight(BTreeNode n)
        {
            right = n;
        }

        BTreeNode getLeft()
        {
            return left;
        }

        BTreeNode getRight()
        {
            return right;
        }
    }

    public class GraphBase
    {
        public int posX, posY;
        public float scale;
        public String labelEdge;

        public List<Node> nodes;
        public List<Edge> edges;

        public Node anchorStart, anchorEnd;

        private static Random random = new Random();
        public static string RandomString(int length = 5)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class GraphSymbol : GraphBase
    {
        public GraphSymbol(char label, int x = 0, int y = 0, float s = 1.0f)
        {
            posX = x;
            posY = y;
            scale = s;
            labelEdge = "" + label;
            String id = label + RandomString();

            nodes = new List<Node>();
            edges = new List<Edge>();
            Node n = new Node("gsym_" + id + "_strt");
            n.LabelText = "";
            anchorStart = n;
            nodes.Add(n);
            n = new Node("gsym_" + id + "_end");
            n.LabelText = "";
            anchorEnd = n;
            nodes.Add(n);

            Edge e = new Edge("gsym_" + id + "_strt", labelEdge, "gsym_" + id + "_end");
            edges.Add(e);
        }
    }

    public class GraphPlus : GraphBase
    {
        public GraphPlus(GraphBase a, GraphBase b, int x = 0, int y = 0, float s = 1.0f)
        {
            nodes = new List<Node>();
            edges = new List<Edge>();

            Node n = new Node("gplus_" + a.nodes[0].Id + b.nodes[0].Id + "_strt");
            n.LabelText = "";
            anchorStart = n;
            nodes.Add(n);
            
            n = new Node("gplus_" + a.nodes[0].Id + b.nodes[0].Id + "_end");
            n.LabelText = "";
            anchorEnd = n;
            nodes.Add(n);
            nodes.AddRange(a.nodes);
            nodes.AddRange(b.nodes);
            
            String aStartA = a.anchorStart.Id,
                aEndA = a.anchorEnd.Id,
                aStartB = b.anchorStart.Id,
                aEndB = b.anchorEnd.Id;

            edges.Add(new Edge("gplus_" + a.nodes[0].Id + b.nodes[0].Id+"_strt","ε",aStartA));
            edges.Add(new Edge("gplus_" + a.nodes[0].Id + b.nodes[0].Id+"_strt","ε",aStartB));
            edges.Add(new Edge(aEndA,"ε","gplus_" + a.nodes[0].Id + b.nodes[0].Id + "_end"));
            edges.Add(new Edge(aEndB,"ε","gplus_" + a.nodes[0].Id + b.nodes[0].Id + "_end"));
            edges.AddRange(a.edges);
            edges.AddRange(b.edges);
        }
    }

    public class GraphDot : GraphBase
    {
        public GraphDot(GraphBase a, GraphBase b, int x = 0, int y = 0, float s = 1.0f)
        {
            nodes = new List<Node>();
            edges = new List<Edge>();
            
            bool symA = false, symB = false;                //fix this - unify merge
            if (a.nodes.Count == 2)
                symA = true;
            if (b.nodes.Count == 2)
                symB = true;

            if (symA)
            {
                nodes.Add(a.anchorStart);
            }
            else
                nodes.AddRange(a.nodes);
            if (symB && !symA)
            {
                nodes.Add(b.anchorEnd);
            }
            else
                nodes.AddRange(b.nodes);
            String aStartA = a.anchorStart.Id,
                aEndA = a.anchorEnd.Id,
                aStartB = b.anchorStart.Id,
                aEndB = b.anchorEnd.Id;
            if(symA)
            {
                edges.Add(new Edge(aStartA, a.labelEdge, aStartB));
            }
            else
            {
                edges.AddRange(a.edges);
                if (symB)
                    edges.Add(new Edge(aEndA, b.labelEdge, aEndB));
                else
                    edges.Add(new Edge(aEndA, "ε", aStartB));
            }
            if(!symB || symA)
                edges.AddRange(b.edges);

            anchorStart = a.anchorStart;
            anchorEnd = b.anchorEnd;
        }
    }

    public class GraphStar : GraphBase
    {
        public GraphStar(GraphBase a, int x = 0, int y = 0, float s = 1.0f)
        {
            nodes = new List<Node>();
            edges = new List<Edge>();

            Node n = new Node("gplus_" + a.nodes[0].Id + "_strt");
            n.LabelText = "";
            anchorStart = n;
            nodes.Add(n);
            nodes.AddRange(a.nodes);
            n = new Node("gplus_" + a.nodes[0].Id + "_end");
            n.LabelText = "";
            anchorEnd = n;
            nodes.Add(n);

            String aStartA = a.anchorStart.Id,
                aEndA = a.anchorEnd.Id;

            edges.Add(new Edge("gplus_" + a.nodes[0].Id + "_strt", "ε", aStartA));
            edges.Add(new Edge(aEndA, "ε", "gplus_" + a.nodes[0].Id + "_end"));
            edges.Add(new Edge(aEndA, "ε", aStartA));
            edges.Add(new Edge("gplus_" + a.nodes[0].Id + "_strt", "ε", "gplus_" + a.nodes[0].Id + "_end"));
            edges.AddRange(a.edges);
        }
    }
}
