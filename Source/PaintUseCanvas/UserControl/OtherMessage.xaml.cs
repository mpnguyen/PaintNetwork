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
    /// Interaction logic for OtherMessage.xaml
    /// </summary>
    public partial class OtherMessage : System.Windows.Controls.UserControl
    {
        public OtherMessage()
        {
            InitializeComponent();
            TxtDateTime.Text = DateTime.Now.ToString();
        }

        public void SetMessage(string text)
        {
            TxtMessage.Text = text;
        }
        public void SetName(string name)
        {
            TxtName.Text = Name;
        }
    }
}
