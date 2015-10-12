using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FrontListNames
{
    public partial class ListName : Form
    {
        public BackListaNomes.ListNames lista ;
        public ListName()
        {
            InitializeComponent();
        }

        private void ListNames_Load(object sender, EventArgs e)
        {
           lista = new BackListaNomes.ListNames();
        }

        private void addbutton_Click(object sender, EventArgs e)
        {
           string addname = addtextbox.Text;
           if(addname.Length == 0)
             System.Windows.Forms. MessageBox.Show("Error No Name");
           else
             lista.add(addname);
           addtextbox.Text = "";
        }

        private void addtextbox_TextChanged(object sender, EventArgs e)
        {
        
        }

        private void listextbox_TextChanged(object sender, EventArgs e)
        {
          
        }

        private void listbutton_Click(object sender, EventArgs e)
        {
            listextbox.Text = lista.list();
        }

        private void clearbutton_Click(object sender, EventArgs e)
        {
            lista.clear();
        }
    }
}
