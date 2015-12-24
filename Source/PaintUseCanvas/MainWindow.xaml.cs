using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using PaintUseCanvas.UserControl;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using TextBox = System.Windows.Controls.TextBox;


namespace PaintUseCanvas
{
    enum EShape
    {
        Line,
        DrawRectangle,
        FillRectangle,
        DrawEllipse,
        FillEllipse,
        DrawArrow,
        DrawDiamond,
        DrawStar,
        DrawTriangle,
        DrawArrowDown
    }

    public partial class MainWindow
    {
        private enum ModeDraw
        {
            Select,
            Draw,
            AddText
        }

        private Network network = new Network();
        private ModeDraw _modeDraw = ModeDraw.Draw;
        private readonly MyShape _myShape = new MyShape();
        private bool _isDown = false;
        private bool _isSizeChange = false;
        private bool _isMouseMoved = false;
        private bool _justAddedShape = false;
        private bool _isShapeMoved = false;
        private Point _startPoint;
        private EShape _eShape = EShape.Line;
        private bool _isSelected = false;
        private UIElement _selectedElement = null;
        private UIElement _clipboardElement = null;
        private AdornerLayer _aLayer;
        private readonly MyTextBox _myTextBox;
        private double _dx1;
        private double _dy1;
        private double _dx2;
        private double _dy2;
        public static bool IsDragging;
        public static List<Canvas> UndoList = new List<Canvas>();
        private readonly List<Canvas> _redoList = new List<Canvas>();

        public MainWindow()
        {
            UndoList.Add(new Canvas());
            _myTextBox = new MyTextBox();
            MyShape.IsShiftPress = false;
            InitializeComponent();


        }

        //========================================
        //Key press event to draw square or circle
        private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                    MyShape.IsShiftPress = false;
                    break;
            }
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                    MyShape.IsShiftPress = true;
                    break;
                case Key.Delete:
                    BtnDeleteShape_OnClick(sender, e);
                    break;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
                BtnCut_OnClick(sender, e);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
                BtnCopy_OnClick(sender, e);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
                BtnPaste_OnClick(sender, e);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
                Undo_OnClick(sender, e);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
                Redo_OnClick(sender, e);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
                BtnNew_OnClick(sender, e);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
                BtnOpen_OnClick(sender, e);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
                BtnSave_OnClick(sender, e);
        }


        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _aLayer = AdornerLayer.GetAdornerLayer(MyCanvas);

            _aLayer.Add(new AnchorPoint(MyCanvas));
        }

        //======================================================================
        //========================Mouse event===================================
        //Mouse down
        private void MyCanvas_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            switch (_modeDraw)
            {
                case ModeDraw.Draw:
                    _isDown = true;
                    _justAddedShape = false;
                    _myShape.StartPoint = e.GetPosition(sender as IInputElement);
                    break;

                case ModeDraw.Select:
                    _isDown = true;
                    if (!Equals(e.Source as TextBox, _selectedElement) || _selectedElement == null)
                        SelectShape(e);
                    break;
                case ModeDraw.AddText:
                    _startPoint = e.GetPosition(sender as IInputElement);
                    _isDown = true;
                    _justAddedShape = false;
                    break;

            }
        }

        //Mouse move
        private void MyCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            switch (_modeDraw)
            {
                case ModeDraw.Draw:
                    MyCanvas.Cursor = Cursors.Pen;
                    if (!_isDown) return;
                    if (_justAddedShape)
                    {
                        if (MyCanvas.Children.Count > 0)
                            MyCanvas.Children.RemoveAt(MyCanvas.Children.Count - 1);
                    }
                    _isMouseMoved = true;
                    _myShape.EndPoint = e.GetPosition(sender as IInputElement);
                    _myShape.AddShape(_eShape, MyCanvas);
                    _justAddedShape = true;
                    MyCanvas.CaptureMouse();
                    break;
                case ModeDraw.Select:
                    MyCanvas.Cursor = Cursors.Hand;
                    MoveShape(e);

                    break;
                case ModeDraw.AddText:
                    MyCanvas.Cursor = Cursors.ScrollAll;
                    if (!_isDown) return;
                    if (_justAddedShape)
                    {
                        if (MyCanvas.Children.Count > 0)
                            MyCanvas.Children.RemoveAt(MyCanvas.Children.Count - 1);
                    }
                    _isMouseMoved = true;
                    var endPoint = e.GetPosition(sender as IInputElement);
                    _myTextBox.AddTextBox(_startPoint, endPoint, MyCanvas, true);
                    _justAddedShape = true;
                    MyCanvas.CaptureMouse();
                    break;
            }

        }

        //Mouse up
        private void MyCanvas_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            switch (_modeDraw)
            {
                case ModeDraw.Draw:
                    if (_isDown && _isMouseMoved)
                    {

                        if (MyCanvas.Children.Count > 0)
                            MyCanvas.Children.RemoveAt(MyCanvas.Children.Count - 1);

                        _myShape.EndPoint = e.GetPosition(sender as IInputElement);
                        _myShape.AddShape(_eShape, MyCanvas);
                        _justAddedShape = true;
                        var shape = MyCanvas.Children[MyCanvas.Children.Count - 1] as Shape;
                        if (shape != null)
                            shape.SizeChanged += ShapeSizeChange;
                        if (shape is Polygon)
                        {
                            _modeDraw = ModeDraw.Select;
                            BtnSelect.IsChecked = true;
                        }
                        MyCanvas.ReleaseMouseCapture();
                        UndoList.Add(CloneElement(MyCanvas) as Canvas);
                    }
                    _isDown = false;
                    _isMouseMoved = false;

                    break;
                case ModeDraw.Select:
                    if (_isShapeMoved)
                        UndoList.Add(CloneElement(MyCanvas) as Canvas);
                    if (_isSizeChange)
                    {
                        UndoList.Add(CloneElement(MyCanvas) as Canvas);
                        _isSizeChange = false;
                    }
                    _isShapeMoved = false;
                    _isDown = false;
                    IsDragging = false;
                    e.Handled = false;
                    break;
                case ModeDraw.AddText:
                    if (_isDown && _isMouseMoved)
                    {

                        if (MyCanvas.Children.Count > 0)
                            MyCanvas.Children.RemoveAt(MyCanvas.Children.Count - 1);

                        var endPoint = e.GetPosition(sender as IInputElement);
                        _myTextBox.AddTextBox(_startPoint, endPoint, MyCanvas);
                        _justAddedShape = true;
                    }
                    _isDown = false;
                    _isMouseMoved = false;
                    _modeDraw = ModeDraw.Select;
                    BtnSelect.IsChecked = true;
                    MyCanvas.ReleaseMouseCapture();
                    break;
            }
        }

        //Event size change to add undo list
        private void ShapeSizeChange(object sender, SizeChangedEventArgs e)
        {
            _isSizeChange = true;
        }

        //======================================================================




        //Button shape type check
        //================================================================
        private void BtnDrawLine_OnChecked(object sender, RoutedEventArgs e)
        {
            if (TipLabel != null)
                TipLabel.Content = "Tooltip: Draw a line";
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.Line;
        }

        private void BtnDrawRectangle_OnChecked(object sender, RoutedEventArgs e)
        {
            TipLabel.Content = "Tooltip: Press left shift to draw a square";
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.DrawRectangle;
        }

        private void BtnFillRectangle_OnChecked(object sender, RoutedEventArgs e)
        {
            TipLabel.Content = "Tooltip: Press left shift to draw a filled square";
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.FillRectangle;
        }

        private void BtnDrawEllipse_OnChecked(object sender, RoutedEventArgs e)
        {
            TipLabel.Content = "Tooltip: Press left shift to draw a circle";
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.DrawEllipse;
        }

        private void BtnFillEllipse_OnChecked(object sender, RoutedEventArgs e)
        {
            TipLabel.Content = "Tooltip: Press left shift to draw a filled circle";
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.FillEllipse;
        }

        //==================================================================


        //Size select
        //=================================================================
        private void SmallSize_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.Size = 3;
            if (_isSelected && _modeDraw == ModeDraw.Select)
            {
                var shape = _selectedElement as Shape;

                if (shape != null) shape.StrokeThickness = 3;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        private void MediumSize_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.Size = 6;
            if (_isSelected && _modeDraw == ModeDraw.Select)
            {
                var shape = _selectedElement as Shape;
                if (shape != null) shape.StrokeThickness = 6;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }


        private void LargeSize_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.Size = 9;
            if (_isSelected && _modeDraw == ModeDraw.Select)
            {
                var shape = _selectedElement as Shape;
                if (shape != null) shape.StrokeThickness = 9;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        private void VeryLargeSize_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.Size = 12;
            if (_isSelected && _modeDraw == ModeDraw.Select)
            {
                var shape = _selectedElement as Shape;
                if (shape != null) shape.StrokeThickness = 12;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        //==================================================================







        //=================================================================
        //========================Select Style Brush=======================
        private void Solid_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.DashArray = null;
            if (_isSelected && _modeDraw == ModeDraw.Select)
            {
                var shape = _selectedElement as Shape;
                if (shape == null) return;
                shape.StrokeDashArray = null;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        private void Dash_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.DashArray = new DoubleCollection {4, 1};
            if (_isSelected && _modeDraw == ModeDraw.Select)
            {
                var shape = _selectedElement as Shape;
                if (shape == null) return;
                shape.StrokeDashArray = new DoubleCollection {4, 1};
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        private void Dot_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.DashArray = new DoubleCollection {1};
            if (!_isSelected || _modeDraw != ModeDraw.Select) return;
            var shape = _selectedElement as Shape;
            if (shape == null) return;
            shape.StrokeDashArray = new DoubleCollection {1};
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        private void DashDot_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.DashArray = new DoubleCollection {4, 1, 1, 1};
            if (!_isSelected || _modeDraw != ModeDraw.Select) return;
            var shape = _selectedElement as Shape;
            if (shape == null) return;
            shape.StrokeDashArray = new DoubleCollection {4, 1, 1, 1};
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        private void DashDotDot_OnSelected(object sender, RoutedEventArgs e)
        {
            _myShape.DashArray = new DoubleCollection {4, 1, 1, 1, 1, 1};
            if (!_isSelected || _modeDraw != ModeDraw.Select) return;
            var shape = _selectedElement as Shape;
            if (shape == null) return;
            shape.StrokeDashArray = new DoubleCollection {4, 1, 1, 1, 1, 1};
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        //============================================================



        //Button select click
        //========================================================
        private void BtnSelect_Checked(object sender, RoutedEventArgs e)
        {
            TipLabel.Content = "Select your shape";
            _modeDraw = ModeDraw.Select;
        }


        //Select shape
        private void SelectShape(MouseEventArgs e)
        {

            var textBox = _selectedElement as TextBox;
            if (textBox != null)
            {
                var parent = (FrameworkElement) textBox.Parent;
                while (parent != null && parent is IInputElement && !((IInputElement) parent).Focusable)
                {
                    parent = (FrameworkElement) parent.Parent;
                }

                var scope = FocusManager.GetFocusScope(textBox);
                FocusManager.SetFocusedElement(scope, parent);
                if (!textBox.IsKeyboardFocused)
                {
                    UndoList.Add(CloneElement(MyCanvas) as Canvas);
                }
            }
            if (_isSelected)
            {
                _isSelected = false;
                if (_selectedElement != null)
                {
                    var adorners = _aLayer.GetAdorners(_selectedElement);
                    if (adorners != null)
                        _aLayer.Remove(adorners[0]);
                    _selectedElement = null;
                }
            }

            if (!Equals(e.Source, MyCanvas))
            {

                _startPoint = e.GetPosition(MyCanvas);

                _selectedElement = e.Source as UIElement;
                if (_selectedElement != null)
                {
                    _selectedElement.Focus();

                    if (_selectedElement is Line)
                    {
                        var line = _selectedElement as Line;
                        _dx1 = -_startPoint.X + line.X1;
                        _dy1 = -_startPoint.Y + line.Y1;
                        _dx2 = -_startPoint.X + line.X2;
                        _dy2 = -_startPoint.Y + line.Y2;
                    }
                    else
                    {
                        _dx1 = Canvas.GetLeft(_selectedElement);
                        _dy1 = Canvas.GetTop(_selectedElement);
                    }
                    _aLayer = AdornerLayer.GetAdornerLayer(_selectedElement);

                    _aLayer.Add(new AnchorPoint(_selectedElement));
                }

                _isSelected = true;
                e.Handled = true;
            }
        }


        //Move shape 
        private void MoveShape(MouseEventArgs e)
        {
            if (_selectedElement == null) return;
            if (_selectedElement is TextBox)
                return;
            if (!_isDown) return;
            if ((IsDragging == false) &&
                ((Math.Abs(e.GetPosition(MyCanvas).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                 (Math.Abs(e.GetPosition(MyCanvas).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                IsDragging = true;

            if (!IsDragging) return;
            var position = Mouse.GetPosition(MyCanvas);
            var line = _selectedElement as Line;
            if (line != null)
            {

                line.X1 = position.X + _dx1;
                line.X2 = position.X + _dx2;
                line.Y1 = position.Y + _dy1;
                line.Y2 = position.Y + _dy2;
            }
            else
            {
                Canvas.SetTop(_selectedElement, position.Y - (_startPoint.Y - _dy1));
                Canvas.SetLeft(_selectedElement, position.X - (_startPoint.X - _dx1));
            }
            _isShapeMoved = true;

        }


        //========================Open file==========================
        private void BtnOpen_OnClick(object sender, RoutedEventArgs e)
        {

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp) | *.jpg;*.jpeg;*.png;*.bmp",
                Title = "Open files"
            };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;
            MyCanvas.Children.Clear();
            _selectedElement = null;
            _clipboardElement = null;
            var image = new Image {Source = new BitmapImage(new Uri(openFileDialog.FileName))};
            var rectImage = new Rectangle
            {
                Fill = new ImageBrush(image.Source),
                Width = image.Source.Width,
                Height = image.Source.Height
            };
            Canvas.SetTop(rectImage, 0);
            Canvas.SetLeft(rectImage, 0);
            MyCanvas.Children.Add(rectImage);
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        //========================Exit App==========================
        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //========================New file==========================
        private void BtnNew_OnClick(object sender, RoutedEventArgs e)
        {
            MyCanvas.Children.Clear();
            _selectedElement = null;
            _clipboardElement = null;
            UndoList.Clear();
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        //========================Save file==========================
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save file",
                Filter = "JPG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp|JPEG (*.jpeg)|*.jpeg|PNG (*.png)|*.png"
            };
            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {

                SaveImage(new Uri(saveFileDialog.FileName), MyCanvas);
            }
        }

        //=============================================================


        //Save image
        public void SaveImage(Uri path, Canvas canvas)
        {
            if (path == null) return;


            var transform = canvas.LayoutTransform;
            var oldMargin = canvas.Margin;

            canvas.LayoutTransform = null;

            var size = new Size(canvas.ActualWidth, canvas.ActualHeight);

            canvas.Measure(size);
            var relativePoint = canvas.TransformToAncestor(Application.Current.MainWindow)
                .Transform(new Point(0, 0));
            canvas.Arrange(new Rect(size));

            var renderBitmap =
                new RenderTargetBitmap(
                    (int) size.Width,
                    (int) size.Height,
                    96d,
                    96d,
                    PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);

            canvas.Arrange(new Rect(relativePoint, size));

            using (var outStream = new FileStream(path.LocalPath, FileMode.Create))
            {

                var encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                encoder.Save(outStream);
            }

            canvas.Margin = oldMargin;
            canvas.LayoutTransform = transform;
            canvas.UpdateLayout();
        }


        //Event button clipboard click
        //Cut
        private void BtnCut_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedElement == null)
            {
                MessageBox.Show("Please select your shape!");
                return;
            }
            _clipboardElement = _selectedElement;
            MyCanvas.Children.Remove(_selectedElement);
            _selectedElement = null;
        }

        //Copy
        private void BtnCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedElement == null)
            {
                MessageBox.Show("Please select your shape!");
                return;
            }
            _clipboardElement = _selectedElement;
        }

        //Paste
        private void BtnPaste_OnClick(object sender, RoutedEventArgs e)
        {
            if (_clipboardElement == null)
            {
                MessageBox.Show("Clipboard is empty!");
                return;
            }
            var shape = _clipboardElement;
            var saved = XamlWriter.Save(shape);
            var newShape = (UIElement) XamlReader.Load(XmlReader.Create(new StringReader(saved)));
            Canvas.SetTop(newShape, Canvas.GetTop(shape) + 30);
            Canvas.SetLeft(newShape, Canvas.GetLeft(shape) + 30);
            MyCanvas.Children.Add(newShape);
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        //Delete Shape
        private void BtnDeleteShape_OnClick(object sender, RoutedEventArgs e)
        {
            if (_selectedElement == null)
            {
                MessageBox.Show("Please select your shape!");
                return;
            }
            MyCanvas.Children.Remove(_selectedElement);
            _selectedElement = null;
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        //=====================AddTextEvent============================
        private void BtnAddText_OnClick(object sender, RoutedEventArgs e)
        {
            _modeDraw = ModeDraw.AddText;

        }

        //===============================Edit text style=====================================
        //=====================================================================================
        //Choose font family
        private void FontChooser_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var textBox = _selectedElement as TextBox;
            _myTextBox.FontFamily = FontChooser.SelectedValue as FontFamily;
            if (textBox != null)
            {
                textBox.FontFamily = FontChooser.SelectedValue as FontFamily;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        //Is bold text
        private void BtnBold_OnClick(object sender, RoutedEventArgs e)
        {
            var textBox = _selectedElement as TextBox;
            _myTextBox.IsBold = BtnBold.IsChecked == true;
            if (textBox != null)
            {
                textBox.FontWeight = BtnBold.IsChecked == true ? FontWeights.Bold : FontWeights.Normal;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        //Italic text
        private void BtnItalic_OnClick(object sender, RoutedEventArgs e)
        {
            var textBox = _selectedElement as TextBox;
            _myTextBox.IsItalic = BtnBold.IsChecked == true;
            if (textBox != null)
            {
                textBox.FontStyle = BtnItalic.IsChecked == true ? FontStyles.Italic : FontStyles.Normal;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        //Underline text
        private void BtnUnderline_OnClick(object sender, RoutedEventArgs e)
        {
            var textBox = _selectedElement as TextBox;
            _myTextBox.IsUnderline = BtnUnderline.IsChecked == true;
            if (textBox != null)
            {
                textBox.TextDecorations = BtnUnderline.IsChecked == true ? TextDecorations.Underline : null;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        //Font size
        private void BtnFontSize_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var textBox = _selectedElement as TextBox;

            double size;
            if (double.TryParse(BtnFontSize.Value.ToString(), out size))
            {
                if (textBox != null)
                {
                    textBox.FontSize = size;
                    UndoList.Add(CloneElement(MyCanvas) as Canvas);
                }
                _myTextBox.FontSize = size;
            }
            else
            {
                if (textBox != null)
                {
                    textBox.FontSize = 16;
                    UndoList.Add(CloneElement(MyCanvas) as Canvas);
                }
                _myTextBox.FontSize = 16;
            }
        }

        //Choose color text
        private void BtnColorText_OnClick(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            var result = colorDialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            var color = colorDialog.Color;
            var newcolor = Color.FromArgb(color.A, color.R, color.G, color.B);
            BtnColorText.Foreground = new SolidColorBrush(newcolor);
            var textBox = _selectedElement as TextBox;
            if (textBox != null)
            {
                textBox.Foreground = BtnColorText.Foreground;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
            _myTextBox.Color = (SolidColorBrush) BtnColorText.Foreground;
        }

        //Choose background textbox
        private void BtnBackgroudText_OnClick(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            var result = colorDialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            var color = colorDialog.Color;
            var newcolor = Color.FromArgb(color.A, color.R, color.G, color.B);
            BtnBackgroudText.Foreground = new SolidColorBrush(newcolor);
            var textBox = _selectedElement as TextBox;
            if (textBox != null)
            {
                textBox.Background = BtnBackgroudText.Foreground;
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        //========================================================================


        //==========================Rotate event===================================
        //=========================================================================
        private void BtnRotateRight_OnClick(object sender, RoutedEventArgs e)
        {
            var shape = _selectedElement as Shape;
            if (shape != null)
            {
                if (shape is Polygon)
                    return;
                if (shape.Fill != null)
                {
                    //Get angle
                    var transform = shape.Fill.RelativeTransform as RotateTransform ?? new RotateTransform(0);
                    //Rotate fill shape
                    shape.Fill.RelativeTransform = new RotateTransform(transform.Angle + 90, 0.5, 0.5);
                }

                var newtop = Canvas.GetTop(shape) + (shape.Height - shape.Width)/2;
                var newleft = Canvas.GetLeft(shape) + (shape.Width - shape.Height)/2;
                var temp = shape.Height;
                shape.Height = shape.Width;
                shape.Width = temp;
                Canvas.SetTop(shape, newtop);
                Canvas.SetLeft(shape, newleft);
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        private void BtnRotateLeft_OnClick(object sender, RoutedEventArgs e)
        {
            var shape = _selectedElement as Shape;
            if (shape != null)
            {
                if (shape is Polygon)
                    return;
                if (shape.Fill != null)
                {
                    //Get angle
                    var transform = shape.Fill.RelativeTransform as RotateTransform ?? new RotateTransform(0);

                    //Rotate fill shape
                    shape.Fill.RelativeTransform = new RotateTransform(transform.Angle - 90, 0.5, 0.5);
                }

                var newtop = Canvas.GetTop(shape) + (shape.Height - shape.Width)/2;
                var newleft = Canvas.GetLeft(shape) + (shape.Width - shape.Height)/2;
                var temp = shape.Height;
                shape.Height = shape.Width;
                shape.Width = temp;
                Canvas.SetTop(shape, newtop);
                Canvas.SetLeft(shape, newleft);
                UndoList.Add(CloneElement(MyCanvas) as Canvas);
            }
        }

        //====================================================================================

        //==============================Undo - Redo ==========================================
        //====================================================================================
        public static UIElement CloneElement(UIElement orig)
        {
            if (orig == null)
                return (null);

            var s = XamlWriter.Save(orig);

            var stringReader = new StringReader(s);

            var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings());

            return (UIElement) XamlReader.Load(xmlReader);
        }

        //Undo
        private void Undo_OnClick(object sender, RoutedEventArgs e)
        {
            MyCanvas.Children.Clear();
            if (UndoList.Count > 1)
            {
                _redoList.Add(CloneElement(UndoList[UndoList.Count - 1]) as Canvas);
                UndoList.RemoveAt(UndoList.Count - 1);
                foreach (var child in UndoList[UndoList.Count - 1].Children)
                {
                    MyCanvas.Children.Add(CloneElement(child as UIElement));
                }
            }
            _selectedElement = null;
            _clipboardElement = null;



        }

        //Redo
        private void Redo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_redoList.Count == 0)
                return;

            MyCanvas.Children.Clear();

            UndoList.Add(CloneElement(_redoList[_redoList.Count - 1]) as Canvas);

            foreach (var child in _redoList[_redoList.Count - 1].Children)
            {
                MyCanvas.Children.Add(CloneElement(child as UIElement));
            }
            _redoList.RemoveAt(_redoList.Count - 1);
            _selectedElement = null;
            _clipboardElement = null;
        }


        //Change color shape
        private void BtnChooseColor1_OnClick(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            var result = colorDialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            var color = colorDialog.Color;
            var newcolor = Color.FromArgb(color.A, color.R, color.G, color.B);

            BtnColor1.Background = new SolidColorBrush(newcolor);
            BtnColor1.PressedBackground = BtnColor1.Background;
            BtnColor1.MouseOverBackground = BtnColor1.Background;
            BtnColor1.CheckedBackground = BtnColor1.Background;
        }

        private void BtnChooseColor2_OnClick(object sender, RoutedEventArgs e)
        {
            var colorDialog = new ColorDialog();
            var result = colorDialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            var color = colorDialog.Color;
            var newcolor = Color.FromArgb(color.A, color.R, color.G, color.B);

            BtnColor2.Background = new SolidColorBrush(newcolor);

            BtnColor2.PressedBackground = BtnColor2.Background;
            BtnColor2.MouseOverBackground = BtnColor2.Background;
            BtnColor2.CheckedBackground = BtnColor2.Background;
        }

        //Gradient brush
        private void BtnGradient_OnClick(object sender, RoutedEventArgs e)
        {
            var shape = _selectedElement as Shape;
            if (shape is Line)
                return;
            if (shape == null)
            {
                MessageBox.Show("Please select your shape!");
                return;
            }

            shape.Stroke = null;
            var solidColorBrush = BtnColor1.Background as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var colorBrush = BtnColor2.Background as SolidColorBrush;
                if (colorBrush != null)
                    shape.Fill = new LinearGradientBrush(solidColorBrush.Color,
                        colorBrush.Color, new Point(0, 0), new Point(1, 1));
            }
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        //Fill shape color brush
        private void BtnFill_OnClick(object sender, RoutedEventArgs e)
        {
            var shape = _selectedElement as Shape;
            if (shape == null)
            {
                MessageBox.Show("Please select your shape!");
                return;
            }

            shape.Fill = BtnColor1.IsChecked == true ? BtnColor1.Background : BtnColor2.Background;
            shape.Stroke = shape.Fill;
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        //Fill shape by image
        private void BtnImage_OnClick(object sender, RoutedEventArgs e)
        {
            var shape = _selectedElement as Shape;
            if (shape is Line)
                return;
            if (shape == null)
            {
                MessageBox.Show("Please select your shape!");
                return;
            }
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp) | *.jpg;*.jpeg;*.png;*.bmp",
                Title = "Choose image..."
            };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;
            shape.Stroke = null;
            var image = new Image {Source = new BitmapImage(new Uri(openFileDialog.FileName))};
            shape.Fill = new ImageBrush(image.Source);

            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }

        //Change color 1 or color 2
        private void BtnColor_OnChecked(object sender, RoutedEventArgs e)
        {
            var ribbonRadioButton = sender as RibbonRadioButton;
            if (ribbonRadioButton != null)
            {
                var solidColorBrush = ribbonRadioButton.Background as SolidColorBrush;
                if (solidColorBrush != null)
                    _myShape.Color = solidColorBrush.Color;
                if (TipLabel != null)
                    TipLabel.Content = ribbonRadioButton.Name + " is choose";
            }
        }

        //Border Color shape
        private void BtnBorderColor_OnClick(object sender, RoutedEventArgs e)
        {
            var shape = _selectedElement as Shape;
            if (shape == null)
            {
                MessageBox.Show("Please select your shape!");
                return;
            }
            shape.Stroke = BtnColor1.IsChecked == true ? BtnColor1.Background : BtnColor2.Background;
        }


        //Zoom by mouse
        //private void MainWindow_OnMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    double scale = 1;
        //    var scaleTransform = MyCanvas.LayoutTransform as ScaleTransform;
        //    if (scaleTransform != null)
        //    {
        //        scale = scaleTransform.ScaleX;
        //    }

        //    if (e.Delta > 0)
        //    {
        //        scale += 0.2;
        //    }
        //    else
        //    {
        //        scale -= 0.2;
        //        if (scale < 0.2)
        //            return;
        //    }
        //    MyCanvas.LayoutTransform = new ScaleTransform(scale, scale);
        //}

        //Insert custom shape ================================================================
        private void BtnDrawArrow_OnChecked(object sender, RoutedEventArgs e)
        {
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.DrawArrow;
        }

        private void BtnDrawDiamond_OnChecked(object sender, RoutedEventArgs e)
        {
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.DrawDiamond;
        }

        private void BtnStar_OnClick(object sender, RoutedEventArgs e)
        {
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.DrawStar;
        }

        private void BtnTriangle_OnClick(object sender, RoutedEventArgs e)
        {
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.DrawTriangle;
        }

        private void BtnArrowDown_OnClick(object sender, RoutedEventArgs e)
        {
            _modeDraw = ModeDraw.Draw;
            _eShape = EShape.DrawArrowDown;
        }

        private void BtnInsertIamge_OnClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp) | *.jpg;*.jpeg;*.png;*.bmp",
                Title = "Insert image: "
            };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;
            _selectedElement = null;
            _clipboardElement = null;
            var image = new Image {Source = new BitmapImage(new Uri(openFileDialog.FileName))};
            var rectImage = new Rectangle
            {
                Fill = new ImageBrush(image.Source),
                Width = image.Source.Width,
                Height = image.Source.Height
            };
            Canvas.SetTop(rectImage, 0);
            Canvas.SetLeft(rectImage, 0);
            MyCanvas.Children.Add(rectImage);
            UndoList.Add(CloneElement(MyCanvas) as Canvas);
        }
        //========================================================================================================
        private void BtnConnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (BtnConnect.IsChecked == true)
            {
                if (TxtUsername.Text == "")
                {
                    MessageBox.Show("Please type your name before connect!");
                    BtnConnect.IsChecked = false;
                    return;
                }
                TxtUsername.IsReadOnly = true;
                BtnConnect.Label = "Disconnect";
                if (network.Connect())
                {
                    MessageBox.Show("Connected!");
                    network.ClientSend(TxtUsername.Text);
                    new Thread(Recevice).Start();
                }
                else
                {
                    MessageBox.Show("Connect fail!");
                    TxtUsername.IsReadOnly = false;
                    BtnConnect.Label = "Connect";
                    BtnConnect.IsChecked = false;
                }
            }
            else
            {
                network.Disconnect();
                TxtUsername.IsReadOnly = false;
                BtnConnect.Label = "Connect";
            }
        }
        private void Recevice()
        {
            while (true)
            {
                string nameClient = network.ClientRecieve();
                string data = network.ClientRecieve();

                this.Dispatcher.BeginInvoke((ThreadStart)delegate()
                {
                    var second = DateTime.Now.Second;
                    if (second%2 == 0)
                    {
                        var message = new UserMessage();
                        message.SetMessage(data);
                        message.Focus();
                        ListMessage.Items.Add(message);
                        
                    }
                    else
                    {
                        var message = new OtherMessage();
                        message.SetMessage(data);
                        message.Focus();
                        ListMessage.Items.Add(message);
                    }
                    ListMessage.ScrollIntoView(ListMessage.Items[ListMessage.Items.Count - 1]);
                });
            }
        }
        private void Send_OnClick(object sender, RoutedEventArgs e)
        {
            network.ClientSend(TxtMessage.Text);
            TxtMessage.Text = "";
        }

        private void TxtMessage_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (Equals(textBox.Foreground, Brushes.Gray))
                {
                    //Remove hint text and set color
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            }
        }
        private void TxtMessage_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (textBox.Text.Trim().Equals(""))
                {
                    //Add hint text
                    textBox.Foreground = Brushes.Gray;
                    textBox.Text = "Type for message ...!";
                }
            }
        }
    }
}
