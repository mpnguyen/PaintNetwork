using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PaintUseCanvas
{
    class MyTextBox
    {
        public FontFamily FontFamily { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public double FontSize { get; set; }
        public SolidColorBrush Color { get; set; }

        public MyTextBox()
        {
            FontFamily = new FontFamily("Arial");
            IsBold = false;
            IsItalic = false;
            IsUnderline = false;
            FontSize = 16;
            Color = new SolidColorBrush(Colors.Black);
        }

        //Add textbox to canvas
        //=============================================================
        public void AddTextBox(Point startPoint, Point endPoint, Canvas myCanvas, bool hasBorder = false)
        {
            //Create textbox
            var textBox = new TextBox
            {
                Text = "Insert text...!",
                Foreground = Brushes.Gray,
                FontFamily = FontFamily,
                BorderBrush = hasBorder ? new SolidColorBrush(Colors.Black) : null,
                FontStyle = IsItalic ? FontStyles.Italic : FontStyles.Normal,
                FontWeight = IsBold ? FontWeights.Bold : FontWeights.Normal,
                TextDecorations = IsUnderline ? TextDecorations.Underline : null,
                Width = Math.Abs(endPoint.X - startPoint.X),
                Height = Math.Abs(endPoint.Y - startPoint.Y),
                TextWrapping = TextWrapping.Wrap,
                Background = Brushes.Transparent,
                AcceptsReturn = true,
                FontSize = FontSize
            };

            //Even keyboard focus
            textBox.GotKeyboardFocus += textBox_GotKeyboardFocus;
            textBox.LostKeyboardFocus += textBox_LostKeyboardFocus;

            Canvas.SetTop(textBox, Math.Min(startPoint.Y, endPoint.Y));
            Canvas.SetLeft(textBox, Math.Min(startPoint.X, endPoint.X));

            myCanvas.Children.Add(textBox);
        }

        //Event keyboard lost focus
        void textBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (textBox.Text.Trim().Equals(""))
                {
                    //Add hint text
                    textBox.Foreground = Brushes.Gray;
                    textBox.Text = "Insert text...!";
                }
            }
        }

        //Event keyboard focus
        void textBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (Equals(textBox.Foreground, Brushes.Gray))
                {
                    //Remove hint text and set color
                    textBox.Text = "";
                    textBox.Foreground = Color;
                }
            }
        }

    }
}