using Fani_Assignment.Helpers;
using Serilog;

namespace Fani_Assignment;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Application starting up...");

        var tableHelper = new TableHelper(10);

        var csvReaderHelper = new CsvReaderHelper(tableHelper);
        csvReaderHelper.LoadCsvData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv")); 

        ShowMenu(tableHelper);

        // Log with color for shutting down
        Log.Information("[red][bold]Application shutting down...[/]");
        Log.CloseAndFlush();
    }

    private static void ShowMenu(TableHelper tableHelper)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Main Menu");
            Console.WriteLine("1. Display Table");
            Console.WriteLine("2. Exit");
            Console.Write("Choose an option: ");
            
            var input = Console.ReadLine();
            
            if (input == "1")
            {
                Log.Information("Displaying the table.");
                Console.Clear();
                tableHelper.DisplayTable(1); // For simplicity, displaying the first page; we could implement further pagination logic if needed
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
            }
            else if (input == "2")
            {
                Log.Information("Exiting the application.");
                break;
            }
            else
            {
                Log.Warning("Invalid option selected.");
                Console.WriteLine("Invalid option, please try again.");
                System.Threading.Thread.Sleep(1000); // Pause briefly before re-displaying the menu
            }
        }
    }
}