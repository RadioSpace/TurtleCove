using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Shell
{
    public partial class ShellForm : Form
    {
        Point centerScreen, oldMousePos;

        public ShellForm()
        {
            InitializeComponent();

            ClientSize = new System.Drawing.Size(800, 600);
            centerScreen = PointToClient(new Point(400, 300));
            oldMousePos = centerScreen;

            Cursor.Position = centerScreen;
            
            Cursor.Hide();

            
            
        }
    }
}
