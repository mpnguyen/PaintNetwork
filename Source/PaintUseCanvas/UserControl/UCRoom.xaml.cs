using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PaintUseCanvas.UserControl
{
    /// <summary>
    /// Interaction logic for UCRoom.xaml
    /// </summary>
    public partial class UCRoom : System.Windows.Controls.UserControl
    {
        public UCRoom()
        {
            InitializeComponent();
        }

        public void SetName(string name)
        {
            TxtName.Text = name;
        }
        public void SetHostname(string name)
        {
            TxtHost.Text = name;
        }
        public void SetNumber(string number)
        {
            TxtNumber.Text = number;
        }
    }
}
