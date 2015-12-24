using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PaintUseCanvas
{
    class MyShape
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public Color Color { get; set; }
        public int Size { get; set; }
        public DoubleCollection DashArray { get; set; }
        public Shape Shape { get; set; }
        public static bool IsShiftPress { get; set; }

        //Do em doc nhan xet cua thay hoi tre nen chua kip sua lai myshape ap dung huong doi tuong a!
        
        //Add Shape to canvas
        public void AddShape(EShape eshape, Canvas myCanvas)
        {
            Shape shape;
            switch (eshape)
            {
                case EShape.Line:
                    shape = CreateLine();
                    break;
                case EShape.DrawRectangle:
                    shape = CreateRectangle(false);
                    break;
                case EShape.FillRectangle:
                    shape = CreateRectangle(true);
                    break;
                case EShape.DrawEllipse:
                    shape = CreateEllipse(false);
                    break;
                case EShape.FillEllipse:
                    shape = CreateEllipse(true);
                    break;
                case EShape.DrawArrow:
                    shape = CreateArrow();
                    break;
                case EShape.DrawDiamond:
                    shape = CreateDiamond();
                    break;
                case EShape.DrawStar:
                    shape = CreateStar();
                    break;
                case EShape.DrawTriangle:
                    shape = CreateTriangle();
                    break;
                case EShape.DrawArrowDown:
                    shape = CreateArrowDown();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("eshape", eshape, null);
            }
            myCanvas.Children.Add(shape);
        }

        //Arrow
        public Shape CreateArrow()
        {
            var additionalShape = new Polygon();
            var height = EndPoint.Y - StartPoint.Y;
            var width = EndPoint.X - StartPoint.X;
            var arrow = new PointCollection
            {
                new Point(StartPoint.X, StartPoint.Y + height/4),
                new Point(StartPoint.X + width/2, StartPoint.Y + height/4),
                new Point(StartPoint.X + width/2, StartPoint.Y),
                new Point(EndPoint.X, StartPoint.Y + height/2),
                new Point(StartPoint.X + width/2, EndPoint.Y),
                new Point(StartPoint.X + width/2, StartPoint.Y + 3*height/4),
                new Point(StartPoint.X, StartPoint.Y + 3*height/4)
            };

            additionalShape.Points = arrow;
            additionalShape.StrokeThickness = Size;
            additionalShape.Stroke = new SolidColorBrush(Color);
            additionalShape.Stretch = Stretch.Fill;
            Canvas.SetLeft(additionalShape, Math.Min(StartPoint.X, EndPoint.X));
            Canvas.SetTop(additionalShape, Math.Min(StartPoint.Y,EndPoint.Y));
            return additionalShape;
        }
        //Arrow
        public Shape CreateArrowDown()
        {
            var additionalShape = new Polygon();
            var height = EndPoint.Y - StartPoint.Y;
            var width = EndPoint.X - StartPoint.X;
            var arrow = new PointCollection
            {
                new Point(StartPoint.X + width/4, StartPoint.Y),
                new Point(StartPoint.X + width*3/4, StartPoint.Y),
                new Point(StartPoint.X + width*3/4, StartPoint.Y + height/2),
                new Point(EndPoint.X, StartPoint.Y + height/2),
                new Point(StartPoint.X + width/2, EndPoint.Y),
                new Point(StartPoint.X, StartPoint.Y + height/2),
                new Point(StartPoint.X + width/4, StartPoint.Y + height/2)
            };

            additionalShape.Points = arrow;
            additionalShape.StrokeThickness = Size;
            additionalShape.Stroke = new SolidColorBrush(Color);
            additionalShape.Stretch = Stretch.Fill;
            Canvas.SetLeft(additionalShape, Math.Min(StartPoint.X, EndPoint.X));
            Canvas.SetTop(additionalShape, Math.Min(StartPoint.Y, EndPoint.Y));
            return additionalShape;
        }

        //Create diamond
        public Shape CreateDiamond()
        {
            var additionalShape = new Polygon();    
            var height = EndPoint.Y - StartPoint.Y;
            var width = EndPoint.X - StartPoint.X;
            var diamond = new PointCollection
            {
                new Point(StartPoint.X + width/2, StartPoint.Y),
                new Point(StartPoint.X, StartPoint.Y + height/2),
                new Point(StartPoint.X + width/2, EndPoint.Y),
                new Point(EndPoint.X, StartPoint.Y + height/2),
            };

            additionalShape.Points = diamond;
            additionalShape.StrokeThickness = Size;
            additionalShape.Stroke = new SolidColorBrush(Color);
            additionalShape.Stretch = Stretch.Fill;
            Canvas.SetLeft(additionalShape, Math.Min(StartPoint.X, EndPoint.X));
            Canvas.SetTop(additionalShape, Math.Min(StartPoint.Y, EndPoint.Y));
            return additionalShape;
        }

        //Create star
        public Shape CreateStar()
        {
            var additionalShape = new Polygon();
            var height = EndPoint.Y - StartPoint.Y;
            var width = EndPoint.X - StartPoint.X;
            var star = new PointCollection
            {
                new Point(StartPoint.X, StartPoint.Y + 2*height/5),
                new Point(EndPoint.X, StartPoint.Y + 2*height/5),
                new Point(StartPoint.X + width/5, EndPoint.Y),
                new Point(StartPoint.X + width/2, StartPoint.Y),
                new Point(StartPoint.X + 4*width/5, EndPoint.Y),
                new Point(StartPoint.X, StartPoint.Y + 2*height/5)
            };
            additionalShape.Points = star;
            additionalShape.StrokeThickness = Size;
            additionalShape.Stroke = new SolidColorBrush(Color);
            additionalShape.Stretch = Stretch.Fill;
            Canvas.SetLeft(additionalShape, Math.Min(StartPoint.X, EndPoint.X));
            Canvas.SetTop(additionalShape, Math.Min(StartPoint.Y, EndPoint.Y));
            return additionalShape;
        }
        //create triangle
        public Shape CreateTriangle()
        {
            var additionalShape = new Polygon();
            var width = EndPoint.X - StartPoint.X;
            var triangle = new PointCollection
            {
                new Point(StartPoint.X + width/2, StartPoint.Y),
                new Point(StartPoint.X, EndPoint.Y),
                new Point(EndPoint.X,EndPoint.Y)
            };
            additionalShape.Points = triangle;
            additionalShape.StrokeThickness = Size;
            additionalShape.Stroke = new SolidColorBrush(Color);
            additionalShape.Stretch = Stretch.Fill;
            Canvas.SetLeft(additionalShape, Math.Min(StartPoint.X, EndPoint.X));
            Canvas.SetTop(additionalShape, Math.Min(StartPoint.Y, EndPoint.Y));
            return additionalShape;
        }


        //Create Line
        public Shape CreateLine()
        {
            var line = new Line
            {
                StrokeThickness = Size, StrokeDashArray = DashArray, Stroke = new SolidColorBrush(Color), X1 = StartPoint.X, Y1 = StartPoint.Y, X2 = EndPoint.X, Y2 = EndPoint.Y
            };
            return line;
        }

        //Create rectangle
        public Shape CreateRectangle(bool isFill)
        {
            var rect = new Rectangle
            {
                Stroke = new SolidColorBrush(Color), StrokeDashArray = DashArray, StrokeThickness = Size, Width = Math.Abs(EndPoint.X - StartPoint.X), Height = Math.Abs(EndPoint.Y - StartPoint.Y)
            };
            if (IsShiftPress)
                rect.Height = rect.Width;
            Canvas.SetTop(rect, Math.Min(StartPoint.Y, EndPoint.Y));
            Canvas.SetLeft(rect, Math.Min(StartPoint.X, EndPoint.X));
            if (isFill)
                rect.Fill = new SolidColorBrush(Color);
            return rect;
        }

        //Create ellipse
        public Shape CreateEllipse(bool isFill)
        {
            var ellipse = new Ellipse
            {
                Stroke = new SolidColorBrush(Color), StrokeDashArray = DashArray, StrokeThickness = Size, Width = Math.Abs(EndPoint.X - StartPoint.X), Height = Math.Abs(EndPoint.Y - StartPoint.Y)
            };
            if (IsShiftPress)
                ellipse.Height = ellipse.Width;
            Canvas.SetTop(ellipse, Math.Min(StartPoint.Y, EndPoint.Y));
            Canvas.SetLeft(ellipse, Math.Min(StartPoint.X, EndPoint.X));
            if (isFill)
                ellipse.Fill = new SolidColorBrush(Color);
            return ellipse;
        }
    }
}