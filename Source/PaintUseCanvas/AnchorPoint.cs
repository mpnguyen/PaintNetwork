using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintUseCanvas
{
    public class AnchorPoint : Adorner  
    {

        private Thumb _topLeft, _topRight, _bottomLeft, _bottomRight;
        private Line _selectedLine;
        private readonly VisualCollection _visualChildren;


        public AnchorPoint(UIElement element)
            : base(element)
        {
            _visualChildren = new VisualCollection(this);
            var line = element as Line;
            //If element is line, just have 2 anchor
            if (line != null)
            {
                BuildAdornerCorner(ref _topLeft, Cursors.SizeWE);
                BuildAdornerCorner(ref _topRight, Cursors.SizeWE);
                _topLeft.DragDelta += _topLeft_DragDelta;
                _topRight.DragDelta += _topRight_DragDelta;

                _selectedLine = line;
                return;
            }

            //IF element is Canvas
            var canvas = element as Canvas;
            if (canvas != null)
            {
                BuildAdornerCorner(ref _bottomRight, Cursors.SizeNWSE);
                _bottomRight.DragDelta += _bottomRight_DragDelta;
                return;
            }
            //Build anchor
            BuildAdornerCorner(ref _topLeft, Cursors.SizeNWSE);
            BuildAdornerCorner(ref _topRight, Cursors.SizeNESW);
            _topLeft.DragDelta += _topLeft_DragDelta;
            _topRight.DragDelta += _topRight_DragDelta;

            BuildAdornerCorner(ref _bottomLeft, Cursors.SizeNESW);
            BuildAdornerCorner(ref _bottomRight, Cursors.SizeNWSE);

            _bottomLeft.DragDelta += _bottomLeft_DragDelta;
            _bottomRight.DragDelta += _bottomRight_DragDelta;

        }

        //Resize botton - right --------------------------------------
        void _bottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MainWindow.IsDragging = false;
            var adornedElement = AdornedElement as FrameworkElement;
            var hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            EnforceSize(adornedElement);

            adornedElement.Height = Math.Max(e.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);
            // If shift press, draw circle or square
            adornedElement.Width = MyShape.IsShiftPress
                ? adornedElement.Height
                : Math.Max(adornedElement.Width + e.HorizontalChange, hitThumb.DesiredSize.Width);
        }

        // Resize bottom - left -----------------------------------
        void _bottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MainWindow.IsDragging = false;

            var adornedElement = AdornedElement as FrameworkElement;
            var hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            EnforceSize(adornedElement);

            adornedElement.Height = Math.Max(e.VerticalChange + adornedElement.Height, hitThumb.DesiredSize.Height);

            var widthOld = adornedElement.Width;
            var widthNew = Math.Max(adornedElement.Width - e.HorizontalChange, hitThumb.DesiredSize.Width);
            var leftOld = Canvas.GetLeft(adornedElement);
            // If shift press, draw circle or square
            if (MyShape.IsShiftPress)
                widthNew = adornedElement.Height;
            adornedElement.Width = widthNew;
            Canvas.SetLeft(adornedElement, leftOld - (widthNew - widthOld));
        }
        
        //Resize top - right -----------------------------------------
        void _topRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MainWindow.IsDragging = false;
            //Process for line
            if (_selectedLine != null)
            {
                var position = Mouse.GetPosition(this);

                _selectedLine.X2 = position.X;
                _selectedLine.Y2 = position.Y;
                return;
            }
            var adornedElement = AdornedElement as FrameworkElement;
            var hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            EnforceSize(adornedElement);

            adornedElement.Width = Math.Max(adornedElement.Width + e.HorizontalChange, hitThumb.DesiredSize.Width);

            var heightOld = adornedElement.Height;
            var heightNew = Math.Max(adornedElement.Height - e.VerticalChange, hitThumb.DesiredSize.Height);
            var topOld = Canvas.GetTop(adornedElement);
            // If shift press, draw circle or square
            if (MyShape.IsShiftPress)
                heightNew = adornedElement.Width;
            adornedElement.Height = heightNew;
            Canvas.SetTop(adornedElement, topOld - (heightNew - heightOld));
        }

        // Resize top - left-----------------------------------------------
        void _topLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MainWindow.IsDragging = false;
            //Process for line
            if (_selectedLine != null)
            {
                var position = Mouse.GetPosition(this);

                _selectedLine.X1 = position.X;
                _selectedLine.Y1 = position.Y;
                return;
            }
            var adornedElement = AdornedElement as FrameworkElement;
            var hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;

            EnforceSize(adornedElement);

            var widthOld = adornedElement.Width;
            var widthNew = Math.Max(adornedElement.Width - e.HorizontalChange, hitThumb.DesiredSize.Width);
            var leftOld = Canvas.GetLeft(adornedElement);
            adornedElement.Width = widthNew;
            Canvas.SetLeft(adornedElement, leftOld - (widthNew - widthOld));

            var heightOld = adornedElement.Height;
            var heightNew = Math.Max(adornedElement.Height - e.VerticalChange, hitThumb.DesiredSize.Height);
            var topOld = Canvas.GetTop(adornedElement);
            // If shift press, draw circle or square
            if (MyShape.IsShiftPress)
                heightNew = widthNew;
            adornedElement.Height = heightNew;
            Canvas.SetTop(adornedElement, topOld - (heightNew - heightOld));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var selectedElement = AdornedElement as FrameworkElement;
            //Process for line
            if (selectedElement is Line)
            {
                _selectedLine = selectedElement as Line;
                var startRect = new Rect(_selectedLine.X1 - (_topLeft.Width/2), _selectedLine.Y1 - (_topLeft.Width/2),
                    _topLeft.Width, _topLeft.Height);
                _topLeft.Arrange(startRect);

                var endRect = new Rect(_selectedLine.X2 - (_topRight.Width/2), _selectedLine.Y2 - (_topRight.Height/2),
                    _topRight.Width, _topRight.Height);
                _topRight.Arrange(endRect);
            }
            else
            {
                //For other shape
                var desiredWidth = AdornedElement.DesiredSize.Width;
                var desiredHeight = AdornedElement.DesiredSize.Height;

                var adornerWidth = DesiredSize.Width;
                var adornerHeight = DesiredSize.Height;

                if (selectedElement is Canvas)
                {
                    _bottomRight.Arrange(new Rect(desiredWidth - adornerWidth/2 + 5, desiredHeight - adornerHeight/2 +5,
                        adornerWidth, adornerHeight));
                    return finalSize;
                }

                _topLeft.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
                _topRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
                _bottomLeft.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
                _bottomRight.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            }

            return finalSize;
        }


        void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor)
        {
            //Build anchorPoint
            if (cornerThumb != null) return;
            if (AdornedElement is Canvas)
            {
                cornerThumb = new Thumb
                {
                    Cursor = customizedCursor,
                    //Opacity = 0.60,
                    Background = new SolidColorBrush(Colors.White),
                    BorderBrush = new SolidColorBrush(Colors.Black),
                    BorderThickness = new Thickness(1),
                    Height = 8,
                    Width = 8
                };
            }
            else
            {
                cornerThumb = new Thumb
                {
                    Cursor = customizedCursor,
                    Opacity = 0.60,
                    Background = new SolidColorBrush(Colors.Blue),
                    Height = 10,
                    Width = 10
                };
            }
           
            _visualChildren.Add(cornerThumb);
        }


        void EnforceSize(FrameworkElement adornedElement)
        {
            if (adornedElement.Width.Equals(double.NaN))
                adornedElement.Width = adornedElement.DesiredSize.Width;
            if (adornedElement.Height.Equals(double.NaN))
                adornedElement.Height = adornedElement.DesiredSize.Height;

            var parent = adornedElement.Parent as FrameworkElement;
            if (parent != null)
            {
                adornedElement.MaxHeight = parent.ActualHeight;
                adornedElement.MaxWidth = parent.ActualWidth;
            }
        }

        protected override int VisualChildrenCount { get { return _visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return _visualChildren[index]; }

    }
}