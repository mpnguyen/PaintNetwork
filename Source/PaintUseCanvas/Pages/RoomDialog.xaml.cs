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
using System.Windows.Shapes;
using PaintUseCanvas.UserControl;

namespace PaintUseCanvas.Pages
{
    /// <summary>
    /// Interaction logic for RoomDialog.xaml
    /// </summary>
    public partial class RoomDialog : Window
    {
        private List<RoomData> _listRoom;

        public delegate void MainWindowHandler(string txtName);

        public MainWindowHandler CreateRoom;
        public MainWindowHandler JoinRoom;

        public RoomDialog(List<RoomData> listRoom)
        {
            _listRoom = listRoom;
            InitializeComponent();
        }

        private void BtnCreate_OnClick(object sender, RoutedEventArgs e)
        {
            CreateRoom(TxtNameRoom.Text);
            DialogResult = true;
            this.Close();
        }

        private void RoomDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var roomData in _listRoom)
            {
                var room = new UCRoom();
                room.SetName(roomData.Name);
                room.SetHostname(roomData.Host);
                room.SetNumber(roomData.Member.Count.ToString());
                room.MouseDoubleClick += room_MouseDoubleClick;
                ListBoxRoom.Items.Add(room);
            }
        }

        void room_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var room = sender as UCRoom;
            JoinRoom(room.TxtName.Text);
            this.Close();
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            if (TxtNameRoom.Text != "")
            {
                foreach (UCRoom room in ListBoxRoom.Items)
                {
                    if (room.TxtName.Text != TxtNameRoom.Text)
                        room.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                foreach (UCRoom room in ListBoxRoom.Items)
                {
                    room.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
