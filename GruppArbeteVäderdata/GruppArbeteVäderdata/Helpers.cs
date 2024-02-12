using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GruppArbeteVäderdata.Addons;

namespace GruppArbeteVäderdata
{
    internal class Helpers
    {
        public static void ReadAll(string path)
        {

            using (StreamReader reader = new StreamReader(path))
            {

                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }


            }
        }
        public static void OutsideMenu()
        {
            Console.Clear();
            Console.WriteLine("1: Sök på datum ");
            Console.WriteLine("2: Sortera efter temperatur från varmast till kallast");
            Console.WriteLine("3: Sortera efter luftfuktighet från torrast till fuktigast");
            Console.WriteLine("4: Risk för mögel");
            Console.WriteLine("5: Visa när den meteorologiska hösten börjar");
            Console.WriteLine("6: Visa när den meteorologiska vintern börjar");


            var key = (Console.ReadKey(true).KeyChar);
            switch (key)
            {
                case '1':
                    SearchDayTempUte(Program.path);
                    break;

                case '2':
                    SortByTempUte(Program.path); 
                    break;
                case '3':
                    SortByHumidityUte(Program.path);
                    break;
                case '4':
                    RiskOfMold(Program.path);
                    break;
                case '5':
                    MeteorologicalAutumn(Program.path);
                    break;
                case '6':
                    MeteorologicalWinter(Program.path); 
                    break;
                default:
                    Console.Clear();
                    return;
            }
        }

        public static void InsideMenu()
        {
            Console.Clear();
            Console.WriteLine("1: Sök på datum ");
            Console.WriteLine("2: Sortera efter temperatur från varmast till kallast");
            Console.WriteLine("3: Sortera efter luftfuktighet från torrast till fuktigast");
            Console.WriteLine("4: Risk för mögel");

            var key = (Console.ReadKey(true).KeyChar);
            switch (key)
            {
                case '1':
                    SearchDayTempInne(Program.path);
                    break;

                case '2':
                    SortByTempInne(Program.path);
                    break;
                case '3':
                    SortByHumidityInne(Program.path);
                    break;
                case '4':
                    RiskOfMoldInne(Program.path);
                    break;
                default:
                    Console.Clear();
                    return;
                
            }
        }
        public static void Outside(string path)
        {

            using (StreamReader reader = new StreamReader(path))
            {
                string pattern = ".*Ute.*";
                Regex regex = new Regex(pattern);
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    if (regex.IsMatch(line))
                    {
                        Console.WriteLine(line);
                    }
                }

            }



        }

        public static void SearchDayTempUte(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Välj datum du vill söka temperaturfakta om Format: (YYYY-MM-DD)");

                string input = Console.ReadLine();
                string pattern = "\\b\\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\\d|3[01])\\b"; // [12]\\d 1-2 +0-9 = talen mellan 10-19 och 20-29
                var lines = File.ReadAllLines(path).ToList();

                double tempSum = 0.0;
                double humSum = 0.0;
                int tempCount = 0;
                int humCount = 0;

                string temperatureString;
                string humidityString;
                if (Regex.IsMatch(input, pattern))
                {
                    foreach (var line in lines)
                    {
                        if (line.Contains(input) && line.Contains("Ute"))
                        {
                            string[] parts = line.Split(',');
                            temperatureString = parts[2].Trim(); //trim ta bort whitespace
                            humidityString = parts[3].Trim();
                            
                            if (double.TryParse(temperatureString, CultureInfo.InvariantCulture, out double temperature)) //culture för att läsa datum med ,
                            {
                                tempSum += temperature;
                                tempCount++;
                            }
                            else
                            {
                                Console.WriteLine("Failed to parse temperature:" + temperatureString);
                            }
                            if (double.TryParse(humidityString, CultureInfo.InvariantCulture, out double humidity))
                            {
                                humSum += humidity;
                                humCount++;
                            }
                        }
                    }

                    if (tempCount > 0)
                    {
                        double tempAverage = tempSum / tempCount;
                        Console.WriteLine("Medeltemperatur: " + tempAverage.ToString("F2"));
                    }
                    else
                    {
                        Console.WriteLine("Temperatur data hittades ej.");
                    }

                    if (humCount > 0)
                    {
                        double humAverage = humSum / humCount;
                        Console.WriteLine("Medelluftfuktighet: " + humAverage.ToString("F2"));
                    }
                    else
                    {
                        Console.WriteLine("Luftfuktighets data hittades ej.");
                    }
                }
                else
                {
                    Console.WriteLine("Regex matchar inte");
                }

                Console.ReadKey();

            }


        }

        public static void SortByTempUte(string path) //EXTENSION & DELEGATE
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Här är dagarna sorterade varmast till kallast: ");
                var lines = File.ReadAllLines(path).ToList();

                var sortedDays = lines.Where(line => line.Contains("Ute")).Select(line =>
                {
                    string[] parts = line.Split(',');
                    string[] date = parts[0].Split(' ');
                    return new //anonym
                    {
                        Date = date[0],
                        Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture)
                    };
                }).GroupBy(day => day.Date) // Gruppera efter datum
                  .Select(group => new
                  {
                      Date = group.Key,
                      AverageTemperature = group.Select(day => day.Temperature).Average()

                  }).OrderByDescending(group => group.AverageTemperature).ToList();

                
                

                foreach (var day in sortedDays)
                {
                    MyDelegate del = (temp) => temp.ToString("F2"); //DELEGATE
                    string roundedTemperature = Addons.RoundTemp(del, day.AverageTemperature);

                    Console.WriteLine(day.Date + ": Ute, medeltemperatur = " + roundedTemperature + "°C");
                    WriteAll(day.Date + ": Ute, medeltemperatur = " + roundedTemperature + "°C"); //write to file

                }
                Console.WriteLine();
                int linecount = string.Join("\n", sortedDays).LineCount(); //EXTENSION
                Console.WriteLine("Totalt antal dagar: " +linecount);

                Console.ReadKey();

            }
        }

        public static void SortByHumidityUte(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Här är dagarna sorterade från torrast till fuktigast: ");
                var lines = File.ReadAllLines(path).ToList();

                var sortedDays = lines.Where(line => line.Contains("Ute")).Select(line =>
                {
                    string[] parts = line.Split(',');
                    string[] date = parts[0].Split(' ');
                    return new //anonym
                    {
                        Date = date[0],
                        Humidity = double.Parse(parts[3], CultureInfo.InvariantCulture)
                    };
                }).GroupBy(day => day.Date) // Gruppera efter datum
                  .Select(group => new
                  {
                      Date = group.Key,
                      AverageHumidity = group.Select(day => day.Humidity).Average()

                  }).OrderBy(group => group.AverageHumidity).ToList();


                foreach (var day in sortedDays)
                {
                    Console.WriteLine(day.Date + ": Ute, medelluftfuktighet = " + day.AverageHumidity.ToString("F2") +"%");
                }
                Console.WriteLine();
                int linecount = string.Join("\n", sortedDays).LineCount();
                Console.WriteLine("Totalt antal dagar: " + linecount);
                Console.ReadKey();
            }
        }

        public static void RiskOfMold(string path)
        {
            // ((luftfuktighet -78) * (Temp/15))/0,22

            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Här är dagarna sorterade minst till störst risk för mögel: ");
                var lines = File.ReadAllLines(path).ToList();

                var sortedDays = lines.Where(line => line.Contains("Ute")).Select(line =>
                {
                    string[] parts = line.Split(',');
                    string[] date = parts[0].Split(' ');
                    return new //anonym
                    {
                        Date = date[0],
                        Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        Humidity = double.Parse(parts[3], CultureInfo.InvariantCulture)
                    };
                }).GroupBy(day => day.Date) // Gruppera efter datum
                  .Select(group => new
                  {
                      Date = group.Key,
                      RiskOfMold = group.Average(day => (day.Humidity-78) * (day.Temperature/15)/0.22)

                  }).OrderByDescending(group => group.RiskOfMold).ToList();


                foreach (var day in sortedDays)
                {
                    Console.WriteLine(day.Date + ": Ute, risk för mögel = " + day.RiskOfMold.ToString("F2"));
                }
                Console.WriteLine();
                int linecount = string.Join("\n", sortedDays).LineCount();
                Console.WriteLine("Totalt antal dagar: " + linecount);
                Console.ReadKey();

            }
        }
        public static void MeteorologicalAutumn(string path) //uppdatera till vinter verision
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                WriteAll("Här börjar den meteorologiska hösten: ");
                Console.WriteLine("Här börjar den meteorologiska hösten: ");
                var lines = File.ReadAllLines(path).ToList();

                var sortedDays = lines.Where(line => line.Contains("Ute")).Select(line =>
                {
                    string[] parts = line.Split(',');
                    string[] date = parts[0].Split(' ');
                    return new //anonym
                    {
                        Date = date[0],
                        Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture)
                    };
                }).GroupBy(day => day.Date) // Gruppera efter datum
                  .Select(group => new
                  {
                      Date = group.Key,
                      AverageTemperature = group.Select(day => day.Temperature).Average()

                  }).OrderBy(group => group.Date).ToList();

                int daysInARow = 0;
                
                string pattern = "\\b\\d{4}-(0[8-9]|1[0-2])-(0[1-9]|[12]\\d|3[01])\\b"; //regex matchar bara efter månad 8
                Regex regex = new Regex(pattern);
                
                foreach (var day in sortedDays)
                {
                    Match match = regex.Match(day.Date);

                    if(day.AverageTemperature < 10 && daysInARow<5 && match.Success)
                    {
                        
                            Console.WriteLine(day.Date + ": Ute, medeltemperatur = " + day.AverageTemperature.ToString("F2") + "°C");
                            daysInARow++;
                        WriteAll(day.Date + ": Ute, medeltemperatur = " + day.AverageTemperature.ToString("F2") + "°C");


                        
                    }
                    
                }
                Console.ReadKey();
            }
        }

        public static void MeteorologicalWinter(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Här börjar den meteorologiska vintern: ");
                var lines = File.ReadAllLines(path).ToList();

                var sortedDays = lines.Where(line => line.Contains("Ute")).Select(line =>
                {
                    string[] parts = line.Split(',');
                    string[] date = parts[0].Split(' ');
                    return new //anonym
                    {
                        Date = date[0],
                        Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture)
                    };
                }).GroupBy(day => day.Date) // Gruppera efter datum
                  .Select(group => new
                  {
                      Date = group.Key,
                      AverageTemperature = group.Select(day => day.Temperature).Average()

                  }).OrderBy(group => group.Date).ToList();

                List<string> consecutiveDaysList = new List<string>(); 
                int consecutiveDays = 0;
                int previousDay = 0;

                foreach (var day in sortedDays)
                {
                    
                    string[] parts = day.Date.Split('-');
                    int currentDay = int.Parse(parts[2]);

                    
                    if (day.AverageTemperature < 0)
                    {
                        
                        if (currentDay - previousDay == 1 || previousDay == 0)
                        {
                            consecutiveDays++;
                            consecutiveDaysList.Add($"{day.Date}: Ute, Medeltemperatur = {day.AverageTemperature:F2}°C"); 

                            
                            if (consecutiveDays == 5)
                            {
                                
                                foreach (var consecutiveDay in consecutiveDaysList)
                                {
                                    Console.WriteLine(consecutiveDay);
                                    WriteAll(consecutiveDay);
                                }
                                break;
                            }
                        }
                        else
                        {
                            consecutiveDays = 1; 
                            consecutiveDaysList.Clear(); 
                            consecutiveDaysList.Add($"{day.Date} - temperatur: {day.AverageTemperature:F2}°C"); 
                        }

                        
                        previousDay = currentDay;
                    }
                    else
                    {
                        consecutiveDays = 0; 
                        consecutiveDaysList.Clear(); 
                                                     
                        previousDay = currentDay;
                    }
                }

                
                if (consecutiveDays < 5)
                {
                    Console.WriteLine("Det finns inte 5 dagar i följd med under 0°C.");
                    WriteAll("Det finns inte 5 dagar i följd med under 0°C.");
                }

                Console.ReadKey();
            }
        }

        public static void SearchDayTempInne(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Välj datum du vill söka temperaturfakta om Format: (YYYY-MM-DD)");

                string input = Console.ReadLine();
                string pattern = "\\b\\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\\d|3[01])\\b"; // [12]\\d 1-2 +0-9 = talen mellan 10-19 och 20-29
                var lines = File.ReadAllLines(path).ToList();

                double tempSum = 0.0;
                double humSum = 0.0;
                int tempCount = 0;
                int humCount = 0;

                string temperatureString;
                string humidityString;
                if (Regex.IsMatch(input, pattern))
                {
                    foreach (var line in lines)
                    {
                        if (line.Contains(input) && line.Contains("Inne"))
                        {
                            string[] parts = line.Split(',');
                            temperatureString = parts[2].Trim(); //trim ta bort whitespace
                            humidityString = parts[3].Trim();

                            if (double.TryParse(temperatureString, CultureInfo.InvariantCulture, out double temperature))
                            {
                                tempSum += temperature;
                                tempCount++;
                            }
                            else
                            {
                                Console.WriteLine("Failed to parse temperature:" + temperatureString);
                            }
                            if (double.TryParse(humidityString, CultureInfo.InvariantCulture, out double humidity))
                            {
                                humSum += humidity;
                                humCount++;
                            }
                        }
                    }

                    if (tempCount > 0)
                    {
                        double tempAverage = tempSum / tempCount;
                        Console.WriteLine("medeltemperatur: " + tempAverage.ToString("F2"));
                    }
                    else
                    {
                        Console.WriteLine("No valid temperature data found.");
                    }

                    if (humCount > 0)
                    {
                        double humAverage = humSum / humCount;
                        Console.WriteLine("medelluftfuktighet: " + humAverage.ToString("F2"));
                    }
                    else
                    {
                        Console.WriteLine("No valid humidity data found.");
                    }
                }
                else
                {
                    Console.WriteLine("Regex doesnt match");
                }

                Console.ReadKey();

            }


        }

        public static void SortByTempInne(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Här är dagarna sorterade varmast till kallast: ");
                var lines = File.ReadAllLines(path).ToList();

                var sortedDays = lines.Where(line => line.Contains("Inne")).Select(line =>
                {
                    string[] parts = line.Split(',');
                    string[] date = parts[0].Split(' ');
                    return new //anonym
                    {
                        Date = date[0],
                        Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture)
                    };
                }).GroupBy(day => day.Date) // Gruppera efter datum
                  .Select(group => new
                  {
                      Date = group.Key,
                      AverageTemperature = group.Select(day => day.Temperature).Average()

                  }).OrderByDescending(group => group.AverageTemperature).ToList();


                foreach (var day in sortedDays)
                {
                    Console.WriteLine(day.Date + ": Inne, medeltemperatur = " + day.AverageTemperature.ToString("F2") + "°C");
                }
                Console.WriteLine();
                int linecount = string.Join("\n", sortedDays).LineCount();
                Console.WriteLine("Totalt antal dagar: " + linecount);
                Console.ReadKey();
            }
        }

        public static void SortByHumidityInne(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Här är dagarna sorterade från torrast till fuktigast: ");
                var lines = File.ReadAllLines(path).ToList();

                var sortedDays = lines.Where(line => line.Contains("Inne")).Select(line =>
                {
                    string[] parts = line.Split(',');
                    string[] date = parts[0].Split(' ');
                    return new //anonym
                    {
                        Date = date[0],
                        Humidity = double.Parse(parts[3], CultureInfo.InvariantCulture)
                    };
                }).GroupBy(day => day.Date) // Gruppera efter datum
                  .Select(group => new
                  {
                      Date = group.Key,
                      AverageHumidity = group.Select(day => day.Humidity).Average()

                  }).OrderBy(group => group.AverageHumidity).ToList();


                foreach (var day in sortedDays)
                {
                    Console.WriteLine(day.Date + ": Inne, medelluftfuktighet = " + day.AverageHumidity.ToString("F2") + "%");
                }
                Console.WriteLine();
                int linecount = string.Join("\n", sortedDays).LineCount();
                Console.WriteLine("Totalt antal dagar: " + linecount);
                Console.ReadKey();
            }
        }

        public static void RiskOfMoldInne(string path)
        {
            // ((luftfuktighet -78) * (Temp/15))/0,22

            using (StreamReader reader = new StreamReader(path))
            {
                Console.Clear();
                Console.WriteLine("Här är dagarna sorterade minst till störst risk för mögel: ");
                var lines = File.ReadAllLines(path).ToList();

                var sortedDays = lines.Where(line => line.Contains("Inne")).Select(line =>
                {
                    string[] parts = line.Split(',');
                    string[] date = parts[0].Split(' ');
                    return new //anonym
                    {
                        Date = date[0],
                        Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        Humidity = double.Parse(parts[3], CultureInfo.InvariantCulture)
                    };
                }).GroupBy(day => day.Date) // Gruppera efter datum
                  .Select(group => new
                  {
                      Date = group.Key,
                      RiskOfMold = group.Average(day => (day.Humidity - 78) * (day.Temperature / 15) / 0.22)

                  }).OrderByDescending(group => group.RiskOfMold).ToList();


                foreach (var day in sortedDays)
                {
                    Console.WriteLine(day.Date + ": Inne, risk för mögel = " + day.RiskOfMold.ToString("F2"));
                }
                Console.WriteLine();
                int linecount = string.Join("\n", sortedDays).LineCount();
                Console.WriteLine("Totalt antal dagar: " + linecount);
                Console.ReadKey();
            }
        }

        public static void AverageTempMonthUte(string path)
        {
            
            string[] lines = File.ReadAllLines(path);

           
            var uteLines = lines.Where(line => line.Contains("Ute"));

            
            var temperatureByMonthAndYear = uteLines
                .Select(line => line.Split(','))
                .GroupBy(parts => new {
                    Year = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Year,
                    Month = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture).Month
                })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Temperatures = group.Select(parts => double.Parse(parts[2], CultureInfo.InvariantCulture))
                });

            
            foreach (var monthGroup in temperatureByMonthAndYear)
            {
                double averageTemperature = monthGroup.Temperatures.Average();
                
                WriteAll($"År: {monthGroup.Year}, Månad: {monthGroup.Month}, Medeltemperatur Ute: {averageTemperature:F2}");
                
            }
            
        }

        public static void AverageTempMonthInne(string path)
        {

            string[] lines = File.ReadAllLines(path);

            var inneLines = lines.Where(line => line.Contains("Inne"));

            var temperatureByMonthAndYear = inneLines
                .Select(line => line.Split(','))
                .Select(parts =>
                {
                    try
                    {
                        DateTime dateTime = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        return new
                        {
                            DateTime = dateTime,
                            Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture)
                        };
                    }
                    catch (FormatException) //vid fel format kastar vi raden
                    {
                        
                        return null; 
                    }
                })
                .Where(data => data != null)
                .GroupBy(data => new { data.DateTime.Year, data.DateTime.Month })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Temperatures = group.Select(data => data.Temperature)
                });

            foreach (var monthGroup in temperatureByMonthAndYear)
            {
                double averageTemperature = monthGroup.Temperatures.Average();
                WriteAll($"År: {monthGroup.Year}, Månad: {monthGroup.Month}, Medeltemperatur Inne: {averageTemperature:F2}");
            }

        }
        public static void AverageHumMonthUte(string path)
        {

            string[] lines = File.ReadAllLines(path);

            var inneLines = lines.Where(line => line.Contains("Ute"));

            var temperatureByMonthAndYear = inneLines
                .Select(line => line.Split(','))
                .Select(parts =>
                {
                    try
                    {
                        DateTime dateTime = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        return new
                        {
                            DateTime = dateTime,
                            Humidity = double.Parse(parts[3], CultureInfo.InvariantCulture)
                        };
                    }
                    catch (FormatException) //vid fel format kastar vi raden
                    {

                        return null;
                    }
                })
                .Where(data => data != null)
                .GroupBy(data => new { data.DateTime.Year, data.DateTime.Month })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Humidity = group.Select(data => data.Humidity)
                });

            foreach (var monthGroup in temperatureByMonthAndYear)
            {
                double averageHumidity = monthGroup.Humidity.Average();
                WriteAll($"År: {monthGroup.Year}, Månad: {monthGroup.Month}, Medelfuktighet Ute: {averageHumidity:F2}");
            }

        }
        public static void AverageHumMonthInne(string path)
        {

            string[] lines = File.ReadAllLines(path);

            var inneLines = lines.Where(line => line.Contains("Inne"));

            var temperatureByMonthAndYear = inneLines
                .Select(line => line.Split(','))
                .Select(parts =>
                {
                    try
                    {
                        DateTime dateTime = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        return new
                        {
                            DateTime = dateTime,
                            Humidity = double.Parse(parts[3], CultureInfo.InvariantCulture)
                        };
                    }
                    catch (FormatException) //vid fel format kastar vi raden
                    {

                        return null;
                    }
                })
                .Where(data => data != null)
                .GroupBy(data => new { data.DateTime.Year, data.DateTime.Month })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Humidity = group.Select(data => data.Humidity)
                });

            foreach (var monthGroup in temperatureByMonthAndYear)
            {
                double averageHumidity = monthGroup.Humidity.Average();
                WriteAll($"År: {monthGroup.Year}, Månad: {monthGroup.Month}, Medelfuktighet Inne: {averageHumidity:F2}");
            }

        }
        public static void AverageMoldMonthUte(string path)
        {

            string[] lines = File.ReadAllLines(path);

            var inneLines = lines.Where(line => line.Contains("Ute"));

            var temperatureByMonthAndYear = inneLines
                .Select(line => line.Split(','))
                .Select(parts =>
                {
                    try
                    {
                        DateTime dateTime = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        return new
                        {
                            DateTime = dateTime,
                            Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture),
                            Humidity = double.Parse(parts[3], CultureInfo.InvariantCulture)

                        };
                    }
                    catch (FormatException) //vid fel format kastar vi raden
                    {

                        return null;
                    }
                })
                .Where(data => data != null)
                .GroupBy(data => new { data.DateTime.Year, data.DateTime.Month })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Humidity = group.Select(data => data.Humidity),
                    Temperatures = group.Select(data => data.Temperature),

                    RiskOfMold = group.Average(day => (day.Humidity - 78) * (day.Temperature / 15) / 0.22)
                });

            foreach (var monthGroup in temperatureByMonthAndYear)
            {
                double averageHumidity = monthGroup.Humidity.Average();

                WriteAll("År: " + monthGroup.Year + ", Månad: " + monthGroup.Month + ", MedelMögelrisk Ute: " + monthGroup.RiskOfMold.ToString());
            }

        }
        public static void AverageMoldMonthInne(string path)
        {

            string[] lines = File.ReadAllLines(path);

            var inneLines = lines.Where(line => line.Contains("Inne"));

            var temperatureByMonthAndYear = inneLines
                .Select(line => line.Split(','))
                .Select(parts =>
                {
                    try
                    {
                        DateTime dateTime = DateTime.ParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        return new
                        {
                            DateTime = dateTime,
                            Temperature = double.Parse(parts[2], CultureInfo.InvariantCulture),
                            Humidity = double.Parse(parts[3], CultureInfo.InvariantCulture)

                        };
                    }
                    catch (FormatException) //vid fel format kastar vi raden
                    {

                        return null;
                    }
                })
                .Where(data => data != null)
                .GroupBy(data => new { data.DateTime.Year, data.DateTime.Month })
                .Select(group => new
                {
                    Year = group.Key.Year,
                    Month = group.Key.Month,
                    Humidity = group.Select(data => data.Humidity),
                    Temperatures = group.Select(data => data.Temperature),

                    RiskOfMold = group.Average(day => (day.Humidity - 78) * (day.Temperature / 15) / 0.22)
                });

            foreach (var monthGroup in temperatureByMonthAndYear)
            {
                double averageHumidity = monthGroup.Humidity.Average();

                WriteAll("År: " + monthGroup.Year + ", Månad: " + monthGroup.Month + ", MedelMögelrisk Inne: " + monthGroup.RiskOfMold.ToString());
                
            }
            WriteAll("Algoritmen för mögel: ((luftfuktighet -78) * (Temp/15))/0,22");
        }

        public static void WriteAll(string text)
        {
            string path = "../../../Files/";
            string fileName = "SparadLista";
            string filePath = Path.Combine(path, fileName);

           
            if (File.Exists(filePath)) // om rader redan finns skriv inte in
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (line == text)
                    {
                        return;
                    }
                }
            }

            using (StreamWriter streamWriter = new StreamWriter(filePath, true))
            {
                streamWriter.WriteLine(text);
                
            }
        }
    }
}


        


    