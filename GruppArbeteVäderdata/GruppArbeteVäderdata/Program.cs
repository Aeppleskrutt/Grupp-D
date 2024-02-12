namespace GruppArbeteVäderdata
{
    internal class Program
    {
        public static string path = "../../../Files/tempdata5-med fel.txt";
        
        static void Main(string[] args)
        {
            Helpers.AverageTempMonthUte(path);
            Helpers.AverageTempMonthInne(path);
            Helpers.AverageHumMonthUte(path);
            Helpers.AverageHumMonthInne(path);
            Helpers.AverageMoldMonthUte(path);
            Helpers.AverageMoldMonthInne(path);


           
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("1. Ute");
                Console.WriteLine("2. Inne");
                var key = (Console.ReadKey(true).KeyChar);
                switch (key)
                {
                    case '1':
                        Helpers.OutsideMenu();
                        break;

                    case '2':
                        Helpers.InsideMenu();
                        break;
                }
            }

            

        }
        
    }
}