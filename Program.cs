using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using CsvHelper;
using System.Reflection;
using System.Diagnostics;

namespace med_room
{

    public static class Headers
    {

        public static class TimeSlot
        {
            public const string Week = "Week";

            public const string NbPlages = "Nb_plages";

            public const string NbOperationLimitPerSlot = "NbOperationLimitPerSlot";

            public const string WeekTimeLimit = "WeekTimeLimit";
        }

        public static class Opp
        {
            public const string ID_instance = "ID_instance";
            public const string Limite_VAR = "Limite_VAR";
            public const string Score_op = "Score_op";
            public const string Sem_dispo_ajus = "Sem_dispo_ajus";
            public const string Duree_salle_aj_inter = "Duree_salle_aj_inter";
        }
    }
    public class Program
    {
        public static string Version
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                return fileVersionInfo.ProductVersion;
            }
        }
        public const int TimeSlotDefault = 800;

        public const int NbOperationLimitDefault = 10;

        public static decimal[] TimeSlot = new decimal[]
        {
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            8.0m,
            8.0m,
            8.0m,
            8.0m,
            8.0m,
            8.0m,
            8.0m,
            8.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            16.0m,
            8.0m,
            8.0m,
            8.0m,
            8.0m,
            8.0m
        };

        static void Main(string[] args)
        {
            var readFilePath = string.Empty;
            var writeFilePath = string.Empty;
            Console.WriteLine($"ProgramVersion: {Version}");
#if DEBUG
            readFilePath = "OperationTimeSlot.ods";
#else
            Console.WriteLine("---------------------Format---------------------");
            Console.WriteLine("First Worksheet Name: TimeSlot");
            Console.WriteLine($"\tHEADER: {Headers.TimeSlot.Week}, {Headers.TimeSlot.NbPlages}, {Headers.TimeSlot.NbOperationLimitPerSlot}, {Headers.TimeSlot.WeekTimeLimit}");
             Console.WriteLine("Second Worksheet Name: Operation");
            Console.WriteLine($"\tHEADER:{Headers.Opp.ID_instance}, {Headers.Opp.Limite_VAR}, {Headers.Opp.Score_op}, {Headers.Opp.Sem_dispo_ajus}, {Headers.Opp.Duree_salle_aj_inter}");
            Console.WriteLine("------------------------------------------------");
            
            Console.WriteLine(@"Path du fichier a lire .ODS (ex: C:\Users\blabla\Desktop\Test.ods)");
            readFilePath = Console.ReadLine();
            Console.WriteLine(@"Path du fichier ou ecrire les resultats .CSV (ex: C:\Users\blabla\Desktop\resultat.csv)");
            writeFilePath = Console.ReadLine();
#endif


            var fileInfo = new FileInfo(readFilePath);



            var operationList = GetOperations(fileInfo);
            var weekList = GetWeeks(fileInfo);

            // foreach (var week in weekList)
            // {
            //     Console.WriteLine(week.ToString());
            // }

            // Console.WriteLine("------Operation------");
            // foreach(var opp in operationList){
            //     Console.WriteLine(opp.ToString());
            // }

            var solver = new RoomSolver();
            var allFittedOperations = new List<OperationResult>();
            foreach (var week in weekList)
            {
                var subset = operationList
                    .Where(x => x.SemaineDispo <= week.Number)
                    .Where(x => allFittedOperations.All(y => y.Id != x.Id))
                    .OrderByDescending(x => x.ScoreOp)
                    .ThenBy(x => x.Duree).ToList();

                var weekSelectedOperations = solver.Solve(week, subset);
                Console.WriteLine($"-------------------------WEEK {week.Number}-------------------------------");
                foreach (var selectedOperation in weekSelectedOperations)
                {
                    Console.WriteLine(
                        $"Id: {selectedOperation.Id} "
                        + $"\t Score_op: {selectedOperation.ScoreOp} "
                        + $"\t Durée: {selectedOperation.Duree} "
                        + $"\t Limite_VAR: {selectedOperation.LimitVar}");
                }

                Console.WriteLine(
                    $"Nb fitted operation: {weekSelectedOperations.Count} "
                    + $"\t Total hours: {(decimal)weekSelectedOperations.Sum(x => x.Duree) / 100}"
                    + $"\t Total ScoreOp: {weekSelectedOperations.Sum(x => x.ScoreOp)}");

                allFittedOperations.AddRange(weekSelectedOperations);
            }

            Console.WriteLine($"--------------------------------------------------------");
            Console.WriteLine($"--------------------------------------------------------");
            Console.WriteLine($"------------------------END-----------------------------");
            Console.WriteLine($"--------------------------------------------------------");
            Console.WriteLine($"--------------------------------------------------------");
            Console.WriteLine(
                $"Nb fitted operation: {allFittedOperations.Count} "
                + $"\t Total hours: {(decimal)allFittedOperations.Sum(x => x.Duree) / 100}"
                + $"\t Total ScoreOp: {allFittedOperations.Sum(x => x.ScoreOp)}");

#if !DEBUG
            SaveResults(new FileInfo(writeFilePath), allFittedOperations);
#endif
            Console.Read();
        }

        private static List<Operation> GetOperations(FileInfo fileInfo)
        {
            var operations = new List<Operation>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var data = new ExcelReader().ReadOdsFile(fileInfo.FullName);
            var operationTable = data.Tables[1];
            var header = ExcelHelper.GetExcelHeader(operationTable, 0);
            operationTable.Rows.RemoveAt(0);

            foreach (DataRow operationDataRow in operationTable.Rows)
            {
                var id = operationDataRow.GetCell<int>(() => header[Headers.Opp.ID_instance]);
                var limitVar = operationDataRow.GetCell<string>(() => header[Headers.Opp.Limite_VAR]);
                var scoreOp = operationDataRow.GetCell<decimal>(() => header[Headers.Opp.Score_op]);
                var semaineDispo = operationDataRow.GetCell<int>(() => header[Headers.Opp.Sem_dispo_ajus]);
                var duree = (int)(operationDataRow.GetCell<decimal>(() => header[Headers.Opp.Duree_salle_aj_inter]) * 100);

                operations.Add(new Operation()
                {
                    Id = id,
                    LimitVar = limitVar,
                    ScoreOp = scoreOp,
                    SemaineDispo = semaineDispo,
                    Duree = duree
                });
            }

            return operations;
        }

        private static List<Week> GetWeeks(FileInfo fileInfo)
        {
            var weeks = new List<Week>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var data = new ExcelReader().ReadOdsFile(fileInfo.FullName);
            var weekTable = data.Tables[0];

            Console.WriteLine($"nb line {weekTable.Rows.Count}");
            var header = ExcelHelper.GetExcelHeader(weekTable, 0);
            weekTable.Rows.RemoveAt(0);
            var rowsEnumerator = weekTable.Rows.GetEnumerator();
            for (var i = 0; i < weekTable.Rows.Count; i++)
            {
                var cell = weekTable.Rows[i];
                var weekNumber = cell.GetCell<int>(() => header[Headers.TimeSlot.Week]);
                var nbOperationLimit = cell.GetCell<int>(() => header[Headers.TimeSlot.NbOperationLimitPerSlot], NbOperationLimitDefault);
                var nbPlages = cell.GetCell<int>(() => header[Headers.TimeSlot.NbPlages]);
                var availableTimeForWeek = cell.GetCell<int>(() => header[Headers.TimeSlot.WeekTimeLimit], TimeSlotDefault * nbPlages);

                var availableTimePerSlot = availableTimeForWeek / nbPlages;

                var timeSlot = Enumerable.Range(0, nbPlages).Select(x => new TimeSlot()
                {
                    RemainingTime = availableTimePerSlot
                }).ToList();

                weeks.Add(new Week()
                {
                    Number = weekNumber,
                    NbOperationLimit = nbOperationLimit,
                    TimeSlots = timeSlot
                });
            }

            return weeks;
        }

        private static void SaveResults(FileInfo fileInfo, IList<OperationResult> results)
        {
            using (var mem = new MemoryStream())
            using (var writer = new StreamWriter(fileInfo.FullName))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.Configuration.Delimiter = ";";

                csvWriter.WriteField("ID_instance");
                csvWriter.WriteField("WeekFitted");
                csvWriter.WriteField("Limite_VAR");
                csvWriter.WriteField("Score_op");
                csvWriter.WriteField("Sem_dispo_ajus");
                csvWriter.WriteField("Duree_salle_aj_inter");
                csvWriter.NextRecord();

                foreach (var op in results)
                {
                    csvWriter.WriteField(op.Id);
                    csvWriter.WriteField(op.WeekFitted);
                    csvWriter.WriteField(op.LimitVar);
                    csvWriter.WriteField(op.ScoreOp);
                    csvWriter.WriteField(op.SemaineDispo);
                    csvWriter.WriteField(op.Duree);
                    csvWriter.NextRecord();
                }

                writer.Flush();
                var result = Encoding.UTF8.GetString(mem.ToArray());
                Console.WriteLine(result);
            }
        }
    }

    public class Week
    {
        public int Number { get; set; }

        public IList<TimeSlot> TimeSlots { get; set; }

        public int NbOperationLimit { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Number: {this.Number}\tNbOperationLimit:{this.NbOperationLimit}");
            sb.AppendLine($"\tTimeSlot:{string.Join(',', this.TimeSlots.Select(x => x.RemainingTime))}");

            return sb.ToString();
        }
    }

    public class TimeSlot
    {
        public string Type { get; set; }

        public int RemainingTime { get; set; }
    }

    public class Operation
    {
        public int Id { get; set; }

        public string LimitVar { get; set; }

        public decimal ScoreOp { get; set; }

        public int SemaineDispo { get; set; }

        public int Duree { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}:{this.Id}, {nameof(LimitVar)}:{this.LimitVar}, {nameof(ScoreOp)}:{this.ScoreOp}, {nameof(SemaineDispo)}:{this.SemaineDispo}, {nameof(Duree)}:{this.Duree}";
        }
    }

    public class OperationResult
    {
        public int Id { get; set; }

        public string LimitVar { get; set; }

        public decimal ScoreOp { get; set; }

        public int SemaineDispo { get; set; }

        public int Duree { get; set; }

        public int WeekFitted { get; set; }
    }
}

// public static class ExcelHelper
// {
//     public static Dictionary<string, int> GetExcelHeader(GemBox.Spreadsheet.ExcelWorksheet workSheet, int rowIndex)
//     {
//         var header = new Dictionary<string, int>();
//         if (workSheet != null)
//         {
//             for (var columnIndex = workSheet.Dimension.Start.Column; columnIndex <= workSheet.Dimension.End.Column; columnIndex++)
//             {
//                 if (workSheet.Cells[rowIndex, columnIndex].Value != null)
//                 {
//                     var columnName = workSheet.Cells[rowIndex, columnIndex].Value.ToString();

//                     if (!header.ContainsKey(columnName) && !string.IsNullOrEmpty(columnName))
//                     {
//                         header.Add(columnName, columnIndex);
//                     }
//                 }
//             }
//         }

//         return header;
//     }

public static class DataHelper
{

    public static T GetCell<T>(this DataRow row, Func<int> getHeaderIndex, T defaultValue = default(T))
    {
        try
        {
            var index = getHeaderIndex();
            return row[index].GetOrDefault<T>(defaultValue);

        }
        catch (KeyNotFoundException)
        {
            return defaultValue;
        }
    }
    public static T GetOrDefault<T>(this object value, T defaultValue = default(T))
    {
        if (value == System.DBNull.Value)
        {
            return defaultValue;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }
}

public static class ExcelHelper
{
    public static Dictionary<string, int> GetExcelHeader(DataTable workSheet, int rowIndex)
    {
        var header = new Dictionary<string, int>();
        if (workSheet != null)
        {
            var headerRow = workSheet.Rows[rowIndex];
            for (var columnIndex = 0; columnIndex < headerRow.ItemArray.Count(); columnIndex++)
            {
                var columnName = headerRow[columnIndex].ToString();
                if (!string.IsNullOrEmpty(columnName))
                {
                    header.Add(columnName, columnIndex);
                }
            }
        }

        return header;
    }
}