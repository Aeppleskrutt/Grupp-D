using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GruppArbeteVäderdata
{
    internal static class Addons
    {
        public delegate string MyDelegate(double temp);

        public static string RoundTemp(MyDelegate del, double temp)
        {
            double roundedTemp = Math.Round(temp,2);
            return del(roundedTemp);
        }


        public static int LineCount(this string str) //EXTENSION
        {
            return str.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        
    }
}
