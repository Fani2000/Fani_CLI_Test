using Fani_Assignment.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Fani_Assignment;

public class Program
{
    public static void Main(string[] args)
    {
        
        var host = CreateHostBuilder(args).Build();
        
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
        
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Application starting up...");

        var tableHelper = new TableHelper(10);

        var records = CsvReaderHelper.LoadCsvData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv")); ;
        
        //  TODO: Create InvoiceHeader + InvoiceLines models

        // TODO: InvoiceHeader => Add to the db + Log and HandleException
        
        // TODO: InvoiceLine => Add to the db + Log and HandleException
        
        foreach (var record in records)
        {
            tableHelper.AddRow(
                record.InvoiceNumber, 
                record.InvoiceDate, 
                record.Address, 
                record.InvoiceTotalExVAT, 
                record.Linedescription, 
                record.InvoiceQuantity,
                record.UnitsellingpriceexVAT
            );
        }

        ShowMenu(tableHelper);

        // Log with color for shutting down
        Log.Information("Application shutting down...");
        Log.CloseAndFlush();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
            });
    
    private static void ShowMenu(TableHelper tableHelper)
    {
        while (true)
        {
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