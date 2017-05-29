using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checker
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader sr = new StreamReader("output.txt");

            string s;
            string[] separators = { " ", "," };
            List<Item> attList = new List<Item>();
            string[] words;
            Item newPoint;
            
            #region 
            do
            {
                s = sr.ReadLine();
                if (s != null)
                {
                    words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                    newPoint = new Item();
                    newPoint.lat = double.Parse(words[0], System.Globalization.NumberStyles.Float);
                    newPoint.lng = double.Parse(words[1], System.Globalization.NumberStyles.Float);
                    newPoint.rating = double.Parse(words[2]);
                    attList.Add(newPoint);
                }
            }
            while (s != null);
            #endregion

            double scoreCheck = 0;
            List<Item> visited = new List<Item>();

            foreach (Item x in attList)
            {
                if (!visited.Contains(x))
                {
                    visited.Add(x);
                }
                else
                {
                    Console.WriteLine("Repearting");
                }
                scoreCheck += x.rating;
            }
            Console.WriteLine("Score ->" + scoreCheck);

            Console.ReadKey();
        }
    }
}
