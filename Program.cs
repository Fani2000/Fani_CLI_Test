using Fani_Assignment.Contracts;
using Fani_Assignment.Helpers;
using Fani_Assignment.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Fani_Assignment;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/loggerfile.txt", rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
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

            ShowMenu(host);

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

    private static void ShowMenu(IHost host)
    {
        while (true)
        {
            Console.Clear();
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
                var csvImportService = new CsvImportService(scope.ServiceProvider);
                csvImportService.LoadCsvData(out double totalHeader, out double totalLine);

                if (Math.Abs(totalHeader - totalLine) < 0.01)
                {
                    Log.Information("Verification successful: The sum of InvoiceLines matches the sum of InvoiceHeader totals.");
                }
                else
                {
                    Log.Information("Verification failed: The sums do not match. InvoiceHeader Total: {0}, InvoiceLines Total: {1}",
                        totalHeader, Math.Round(totalLine, 2));
                }

                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
            }
            else if (input == "2")
            {
                Console.Clear();
                using var scope = host.Services.CreateScope();
                ShowInvoiceHeaders(scope);
            }
            else if (input == "3")
            {
                Console.Clear();
                using var scope = host.Services.CreateScope();
                ShowInvoiceLines(scope);
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
                System.Threading.Thread.Sleep(1000); // Brief pause before re-displaying the menu
            }
        }
    }

    private static void ShowInvoiceHeaders(IServiceScope scope)
    {
        Log.Information("Here is a list of all InvoiceHeaders");
        var invoiceHeaderService = scope.ServiceProvider.GetRequiredService<IInvoiceHeaderService>();
        var headers = invoiceHeaderService.GetAllAsync().Result.ToList();

        int currentPage = 0;
        const int pageSize = 10;

        while (true)
        {
            Console.Clear();
            Log.Information("Here is a list of InvoiceHeaders (Page {0})", currentPage + 1);
            var tableHelper = new TableHelper(pageSize);

            var paginatedHeaders = headers.Skip(currentPage * pageSize).Take(pageSize).ToList();
            tableHelper.DisplayInvoiceHeaders(paginatedHeaders);

            Console.WriteLine($"\nPage {currentPage + 1}/{(headers.Count - 1) / pageSize + 1}");
            Console.WriteLine("N: Next Page, P: Previous Page, Q: Quit");

            var input = Console.ReadKey(true).Key;
            if (input == ConsoleKey.N && (currentPage + 1) * pageSize < headers.Count)
            {
                currentPage++;
            }
            else if (input == ConsoleKey.P && currentPage > 0)
            {
                currentPage--;
            }
            else if (input == ConsoleKey.Q)
            {
                break;
            }
        }
    }

    private static void ShowInvoiceLines(IServiceScope scope)
    {
        Log.Information("Here is a list of all InvoiceLines");
        var invoiceLineService = scope.ServiceProvider.GetRequiredService<IInvoiceLineService>();
        var lines = invoiceLineService.GetAllAsync().Result.ToList();

        int currentPage = 0;
        const int pageSize = 10;

        while (true)
        {
            Console.Clear();
            Log.Information("Here is a list of InvoiceLines (Page {0})", currentPage + 1);
            var tableHelper = new TableHelper(pageSize);

            var paginatedLines = lines.Skip(currentPage * pageSize).Take(pageSize).ToList();
            tableHelper.DisplayInvoiceLines(paginatedLines);

            Console.WriteLine($"\nPage {currentPage + 1}/{(lines.Count - 1) / pageSize + 1}");
            Console.WriteLine("N: Next Page, P: Previous Page, Q: Quit");

            var input = Console.ReadKey(true).Key;
            if (input == ConsoleKey.N && (currentPage + 1) * pageSize < lines.Count)
            {
                currentPage++;
            }
            else if (input == ConsoleKey.P && currentPage > 0)
            {
                currentPage--;
            }
            else if (input == ConsoleKey.Q)
            {
                break;
            }
        }
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
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

                services.AddScoped<IInvoiceHeaderService, InvoiceHeaderService>();
                services.AddScoped<IInvoiceLineService, InvoiceLineService>();
            }).UseSerilog();
}