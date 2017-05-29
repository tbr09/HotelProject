using System;
using System.Collections.Generic;
using System.IO;
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

namespace TestVisualization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Item> attList = new List<Item>();
        List<Item> allAtt = new List<Item>();

        public MainWindow()
        {
            InitializeComponent();

            StreamReader sr1 = new StreamReader("output.txt");
            StreamReader sr2 = new StreamReader("F.txt");
            string s;
            string[] separators = { " ", "," };
            string[] words;
            Item newPoint;

            #region 
            do
            {
                s = sr1.ReadLine();
                if (s != null)
                {
                    words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    newPoint = new Item();
                    newPoint.lat = (double.Parse(words[0], System.Globalization.NumberStyles.Float) + 100) / 1.2;
                    newPoint.lng = (double.Parse(words[1], System.Globalization.NumberStyles.Float) + 100) / 1.6;
                    if (words.Count() == 3)
                    {
                        newPoint.rating = double.Parse(words[2]);
                    }
                    attList.Add(newPoint);
                }
            }
            while (s != null);
            #endregion


            s = sr2.ReadLine();
            #region 
            do
            {
                s = sr2.ReadLine();
                if (s != null)
                {
                    words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    newPoint = new Item();
                    newPoint.lat = (double.Parse(words[0], System.Globalization.NumberStyles.Float) + 100) / 1.2;
                    newPoint.lng = (double.Parse(words[1], System.Globalization.NumberStyles.Float) + 100) / 1.6;
                    if (words.Count() == 3)
                    {
                        newPoint.rating = double.Parse(words[2]);
                    }
                    allAtt.Add(newPoint);
                }
            }
            while (s != null);
            #endregion

            Draw(allAtt, false);
            Draw(attList, true);
        }

        public void Draw(List<Item> attList, bool lines)
        {
            int divider = 8;
            Ellipse r1;
            Line l;
            Point p1, p2;
            p1 = new Point();
            p1.X = attList[0].lat;
            p1.Y = attList[0].lng;
            r1 = new Ellipse();
            Canvas.SetLeft(r1, p1.X);
            Canvas.SetTop(r1, p1.Y);

            if (attList[0].rating == 0)
            {
                Canvas.SetLeft(r1, p1.X - 5);
                Canvas.SetTop(r1, p1.Y - 5);
                r1.Fill = Brushes.Yellow;
                r1.Height = 10;
                r1.Width = 10;
            }
            else
            {
                r1.Fill = Brushes.Red;
                r1.Height = attList[0].rating / divider;
                r1.Width = attList[0].rating / divider;
                Canvas.SetLeft(r1, p1.X - attList[0].rating / (2* divider));
                Canvas.SetTop(r1, p1.Y - attList[0].rating / (2 * divider));
            }
            mainCanvas.Children.Add(r1);
            for (int i = 1; i < attList.Count(); i++)
            {

                r1 = new Ellipse();
                p2 = new Point();
                p2.X = attList[i].lat;
                p2.Y = attList[i].lng;

                if (attList[i].rating == 0)
                {
                    Canvas.SetLeft(r1, p2.X - 5);
                    Canvas.SetTop(r1, p2.Y - 5);
                    r1.Fill = Brushes.Yellow;
                    r1.Height = 10;
                    r1.Width = 10;
                }
                else
                {
                    r1.Fill = Brushes.Red;
                    r1.Height = attList[i].rating / divider;
                    r1.Width = attList[i].rating / divider;
                    Canvas.SetLeft(r1, p1.X - attList[i].rating / (2 * divider));
                    Canvas.SetTop(r1, p1.Y - attList[i].rating / (2 * divider));
                }
                mainCanvas.Children.Add(r1);

                if (lines == true)
                {
                    l = new Line();
                    l.X1 = p1.X;
                    l.Y1 = p1.Y;
                    l.X2 = p2.X;
                    l.Y2 = p2.Y;
                    l.StrokeThickness = 2;
                    l.Stroke = Brushes.Black;
                    mainCanvas.Children.Add(l);
                }

                p1 = new Point();
                p1.X = attList[i].lat;
                p1.Y = attList[i].lng;
            }
        }
    }
}
