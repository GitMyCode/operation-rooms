using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using OfficeOpenXml;
using System.Data;
using System.Globalization;

namespace med_room
{
    public static class Headers{
        public static class TimeSlot {
            public const string Week = "Week";

            public const string NbPlages = "Nb_plages";

            public const string NbOperationLimit = "NbOperationLimit";
        }
    }
    public class Program
    {
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
#if DEBUG
            readFilePath = "OperationTimeSlot.ods";
#else
            Console.WriteLine(@"Path du fichier a lire (ex: C:\Users\blabla\Desktop\Test.xlsx)");
            readFilePath = Console.ReadLine();
            Console.WriteLine(@"Path du fichier ou ecrire les resultats (ex: C:\Users\blabla\Desktop\resultat.xlsx)");
            writeFilePath = Console.ReadLine();
#endif


            var fileInfo = new FileInfo(readFilePath);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var data = new ExcelReader().ReadOdsFile(fileInfo.FullName);
            var table1 = data.Tables[0];
            Console.WriteLine($"nb line {table1.Rows.Count}");
            var header = ExcelHelper.GetExcelHeader(table1, 0);
            table1.Rows.RemoveAt(0);
            var rowsEnumerator = table1.Rows.GetEnumerator();
            for(var i =1 ; i< table1.Rows.Count; i++){
                Console.WriteLine($"{table1.Rows[i][header[Headers.TimeSlot.Week]]}, {table1.Rows[i][header[Headers.TimeSlot.NbPlages]]}, {table1.Rows[i][header[Headers.TimeSlot.NbOperationLimit]]}");
            }
           

            //var operationList = GetOperations(fileInfo);
            //             var weekList = GetWeeks(fileInfo);

            //             var solver = new RoomSolver();
            //             var allFittedOperations = new List<OperationResult>();
            //             foreach (var week in weekList)
            //             {
            //                 var subset = operationList
            //                     .Where(x => x.SemaineDispo <= week.Number)
            //                     .Where(x => allFittedOperations.All(y => y.Id != x.Id))
            //                     .OrderByDescending(x => x.ScoreOp)
            //                     .ThenBy(x => x.Duree).ToList();

            //                 var weekSelectedOperations = solver.Solve(week, subset);
            //                 Console.WriteLine($"-------------------------WEEK {week.Number}-------------------------------");
            //                 foreach (var selectedOperation in weekSelectedOperations)
            //                 {
            //                     Console.WriteLine(
            //                         $"Id: {selectedOperation.Id} "
            //                         + $"\t Score_op: {selectedOperation.ScoreOp} "
            //                         + $"\t Durée: {selectedOperation.Duree} "
            //                         + $"\t Limite_VAR: {selectedOperation.LimitVar}");
            //                 }

            //                 Console.WriteLine(
            //                     $"Nb fitted operation: {weekSelectedOperations.Count} "
            //                     + $"\t Total hours: {(decimal)weekSelectedOperations.Sum(x => x.Duree) / 100}"
            //                     + $"\t Total ScoreOp: {weekSelectedOperations.Sum(x => x.ScoreOp)}");

            //                 allFittedOperations.AddRange(weekSelectedOperations);
            //             }

            //             Console.WriteLine($"--------------------------------------------------------");
            //             Console.WriteLine($"--------------------------------------------------------");
            //             Console.WriteLine($"------------------------END-----------------------------");
            //             Console.WriteLine($"--------------------------------------------------------");
            //             Console.WriteLine($"--------------------------------------------------------");
            //             Console.WriteLine(
            //                 $"Nb fitted operation: {allFittedOperations.Count} "
            //                 + $"\t Total hours: {(decimal)allFittedOperations.Sum(x => x.Duree) / 100}"
            //                 + $"\t Total ScoreOp: {allFittedOperations.Sum(x => x.ScoreOp)}");

            // #if !DEBUG
            //             SaveResults(new FileInfo(writeFilePath), allFittedOperations);
            // #endif
            //             Console.Read();
        }

        private static List<Operation> GetOperations(FileInfo fileInfo)
        {
            var workbook = ExcelFile.Load(fileInfo.FullName);

            using (var package = new ExcelPackage(fileInfo))
            {
                var operationWorksheet = package.Workbook.Worksheets["Operation"];
                // var header = ExcelHelper.GetExcelHeader(operationWorksheet, 1);

                var list = new List<Operation>();
                // for (var i = 2; i <= operationWorksheet.Dimension.Rows; i++)
                // {
                //     list.Add(
                //         new Operation
                //         {
                //             Id = operationWorksheet.Cells[i, header["ID_instance"]].GetValue<int>(),
                //             LimitVar = operationWorksheet.Cells[i, header["Limite_VAR"]].GetValue<string>(),
                //             ScoreOp = operationWorksheet.Cells[i, header["Score_op"]].GetValue<decimal>(),
                //             SemaineDispo = operationWorksheet.Cells[i, header["Sem_dispo_ajus"]].GetValue<int>(),
                //             Duree = (int)(100 * operationWorksheet.Cells[i, header["Duree_salle_aj_inter"]].GetValue<decimal>()),
                //         });
                // }

                return list;
            }
        }



        // private static List<Week> GetWeeks(FileInfo fileInfo)
        // {
        //     using (var package = new ExcelPackage(fileInfo))
        //     {
        //         var timeslotWorksheet = package.Workbook.Worksheets["TimeSlot"];
        //         var header = ExcelHelper.GetExcelHeader(timeslotWorksheet, 1);

        //         var list = new List<Week>();
        //         for (var i = 2; i <= timeslotWorksheet.Dimension.Rows; i++)
        //         {
        //             var nbSlot = timeslotWorksheet.Cells[i, header["Nb_plages"]].GetValue<int>();

        //             // if there is no header for timelimit for a week then provie de default timeslot value
        //             var availableTimeForWeek = header.ContainsKey("TimeLimit") ? timeslotWorksheet.Cells[i, header["TimeLimit"]].GetValue<int?>()?? TimeSlotDefault * nbSlot : TimeSlotDefault * nbSlot; 

        //             var nbOperationLimit = header.ContainsKey("NbOperationLimit") ? timeslotWorksheet.Cells[i, header["NbOperationLimit"]].GetValue<int?>() ?? NbOperationLimitDefault : NbOperationLimitDefault;


        //             var availableTimePerSlot = availableTimeForWeek/nbSlot;
        //             var timeSlot = Enumerable.Range(0, nbSlot).Select(
        //                 x => new TimeSlot
        //                 {
        //                     RemainingTime = availableTimePerSlot
        //                 }).ToList();

        //             list.Add(
        //                 new Week()
        //                 {
        //                     Number = timeslotWorksheet.Cells[i, header["Week"]].GetValue<int>(),
        //                     TimeSlots = timeSlot,
        //                     NbOperationLimit = nbOperationLimit
        //                 });
        //         }

        //         return list;
        //     }
        // }

        // private static void SaveResults(FileInfo fileInfo, IList<OperationResult> results)
        // {
        //     using (ExcelPackage package = new ExcelPackage(fileInfo))
        //     {
        //         GemBox.ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Result");

        //         worksheet.Cells[1, 1].Value = "ID_instance";
        //         worksheet.Cells[1, 2].Value = "WeekFitted";
        //         worksheet.Cells[1, 3].Value = "Limite_VAR";
        //         worksheet.Cells[1, 4].Value = "Score_op";
        //         worksheet.Cells[1, 5].Value = "Sem_dispo_ajus";
        //         worksheet.Cells[1, 6].Value = "Duree_salle_aj_inter";


        //         for (int i = 0; i < results.Count; i++)
        //         {
        //             var op = results[i];
        //             var row = i + 2;
        //             worksheet.Cells[row, 1].Value = op.Id;
        //             worksheet.Cells[row, 2].Value = op.WeekFitted;
        //             worksheet.Cells[row, 3].Value = op.LimitVar;
        //             worksheet.Cells[row, 4].Value = op.ScoreOp;
        //             worksheet.Cells[row, 5].Value = op.SemaineDispo;
        //             worksheet.Cells[row, 6].Value = op.Duree;
        //         }


        //         package.Save();
        //     }
        // }
    }

    public class Week
    {
        public int Number { get; set; }

        public IList<TimeSlot> TimeSlots { get; set; }

        public int NbOperationLimit { get; set; }
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