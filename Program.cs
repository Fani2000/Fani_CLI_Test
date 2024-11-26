using System.Globalization;
using Fani_Assignment.Contracts;
using Fani_Assignment.Helpers;
using Fani_Assignment.Models;
using Fani_Assignment.Services;
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
        
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();
                
            }


            Log.Information("Application starting up...");

            /*
            var tableHelper = new TableHelper(10);

            var records = CsvReaderHelper.LoadCsvData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv"));

            if (records != null)
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
                */

            ShowMenu(host);

            // Log with color for shutting down
            Log.Information("Application shutting down...");
            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static void LoadCsvData(IServiceScope scope)
    {
        Console.Clear();
        Log.Information("Loading the csv info to the db...");

        var invoiceHeaderService = scope.ServiceProvider.GetRequiredService<IInvoiceHeaderService>();
        var invoiceLineService = scope.ServiceProvider.GetRequiredService<IInvoiceLineService>();

        var records =
            CsvReaderHelper.LoadCsvData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv"));

        foreach (var record in records)
        {
            try
            {
                DateTime? invoiceDate = null;
                if (DateTime.TryParse(record.InvoiceDate, out var parsedDate))
                {
                    invoiceDate = parsedDate;
                }

                double? invoiceTotal = null;
                if (double.TryParse(record.InvoiceTotalExVAT, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var parsedInvoiceTotal))
                {
                    invoiceTotal = parsedInvoiceTotal;
                }

                var invoiceHeader = new InvoiceHeader
                {
                    InvoiceNumber = record.InvoiceNumber,
                    InvoiceDate = invoiceDate,
                    Address = record.Address,
                    InvoiceTotal = invoiceTotal
                };

                var createdHeader = invoiceHeaderService.CreateAsync(invoiceHeader).Result;

                double? quantity = null;
                if (double.TryParse(record.InvoiceQuantity, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var parsedQuantity))
                {
                    quantity = parsedQuantity;
                }

                double? unitSellingPriceExVAT = null;
                if (double.TryParse(record.UnitsellingpriceexVAT.Replace(";", ""), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var parsedUnitSellingPriceExVAT))
                {
                    unitSellingPriceExVAT = parsedUnitSellingPriceExVAT;
                }

                var invoiceLine = new InvoiceLine
                {
                    InvoiceNumber = record.InvoiceNumber,
                    Description = record.Linedescription,
                    Quantity = quantity,
                    UnitSellingPriceExVAT = unitSellingPriceExVAT
                };

                var createdLine = invoiceLineService.CreateAsync(invoiceLine).Result;

                Log.Information(
                    $"Added InvoiceHeader ID: {createdHeader.InvoiceId}, InvoiceLine ID: {createdLine.LineId}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding invoice data to the database.");
            }
        }
    }

    private static void ShowInvoiceHeaders(IServiceScope scope)
    {
        Console.Clear();
        Log.Information("Here is a list of all InvoiceHeaders");
        var tableHelper = new TableHelper(10);
        var invoiceHeaderService = scope.ServiceProvider.GetRequiredService<IInvoiceHeaderService>();
        var headers = invoiceHeaderService.GetAllAsync().Result.ToList();
        tableHelper.DisplayInvoiceHeaders(headers);
    }
    
    private static void ShowInvoiceLines(IServiceScope scope)
    {
        Console.Clear();
        Log.Information("Here is a list of all InvoiceLines");
        var invoiceLineService = scope.ServiceProvider.GetRequiredService<IInvoiceLineService>();
        var tableHelper = new TableHelper(10);
        var lines = invoiceLineService.GetAllAsync().Result.ToList();
        tableHelper.DisplayInvoiceLines(lines);
                
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
                
                services.AddScoped<IInvoiceHeaderService, InvoiceHeaderService>();
                services.AddScoped<IInvoiceLineService, InvoiceLineService>();
                
            });
    
    private static void ShowMenu( IHost host)
    {
        while (true)
        {
            Console.WriteLine("Main Menu");
            Console.WriteLine("1. Load csv data");
            Console.WriteLine("2. View Invoice Headers");
            Console.WriteLine("3. View Invoice Lines");
            Console.WriteLine("4. Exit");
            Console.Write("Choose an option: ");
            
            var input = Console.ReadLine();
            
            if (input == "1")
            {
                Console.Clear();
                using var scope = host.Services.CreateScope();
                LoadCsvData(scope);
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
            }
            else if (input == "2")
            {
                Console.Clear();
                using var scope = host.Services.CreateScope();
                ShowInvoiceHeaders(scope);
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
                
            }
            else if (input == "3")
            {
                Console.Clear();
                using var scope = host.Services.CreateScope();
                ShowInvoiceLines(scope);
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
            }
            else if (input == "4")
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