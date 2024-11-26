﻿using System.Globalization;
using Fani_Assignment.Contracts;
using Fani_Assignment.Helpers;
using Fani_Assignment.Models;
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
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error) // Override for database logs
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

    private static InvoiceHeader? AddInvoiceHeader(IServiceScope scope, InvoiceRecord record)
    {
        var invoiceHeaderService = scope.ServiceProvider.GetRequiredService<IInvoiceHeaderService>();

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

        return createdHeader;
    }
    
    private static InvoiceLine? AddInvoiceLine(IServiceScope scope, InvoiceRecord record)
    {
        var invoiceLineService = scope.ServiceProvider.GetRequiredService<IInvoiceLineService>();
        double? quantity = null;
        if (double.TryParse(record.InvoiceQuantity, NumberStyles.Any, CultureInfo.InvariantCulture,
                out var parsedQuantity))
        {
            quantity = parsedQuantity;
        }

        double? unitSellingPriceExVat = null;
        if (double.TryParse(record.UnitsellingpriceexVAT?.Replace(";", ""), NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var parsedUnitSellingPriceExVAT))
        {
            unitSellingPriceExVat = parsedUnitSellingPriceExVAT;
        }

        var invoiceLine = new InvoiceLine
        {
            InvoiceNumber = record.InvoiceNumber,
            Description = record.Linedescription,
            Quantity = quantity,
            UnitSellingPriceExVAT = unitSellingPriceExVat
        };

        var createdLine = invoiceLineService.CreateAsync(invoiceLine).Result;

        // TODO: Log the sum of quantities for each invoiceNumber 
        
        return createdLine;
    }

    private static void LoadCsvData(IServiceScope scope)
    {
        Console.Clear();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning) // Override for database logs
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/loggerfile.txt", rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        
        Log.Information("Loading the csv info to the db...");


        var records =
            CsvReaderHelper.LoadCsvData(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv"));

        if (records != null)
            foreach (var record in records)
            {
                try
                {
                    var createdHeader = AddInvoiceHeader(scope, record);
                    var createdLine = AddInvoiceLine(scope, record);

                    if (createdHeader != null && createdLine != null)
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
            }).UseSerilog();
    
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