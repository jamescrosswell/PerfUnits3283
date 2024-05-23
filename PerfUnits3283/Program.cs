using PerfUnits3283;
using Sentry;
using Sentry.Infrastructure;

const int errorEveryXEvents = 10;

var transactionsToSend = 100;
string dsn = string.Empty;

// Loop through the arguments and check for the DSN argument
for (int i = 0; i < args.Length; i++)
{
    if (args[i].ToLower() == "--dsn" && args.Length > i + 1)
    {
        dsn = args[i + 1];
    }
    else if (args[i].ToLower() == "--events" && args.Length > i + 1)
    {
        transactionsToSend = int.Parse(args[i + 1]);
    }
}

if (string.IsNullOrWhiteSpace(dsn))
{
    Console.WriteLine("Please provide a Sentry DSN using the --dsn argument.");
    return;
}

SentrySdk.Init(options =>
{
    options.Dsn = dsn;
    options.Debug = true;
    options.DiagnosticLogger = new ConsoleDiagnosticLogger(SentryLevel.Debug);
    options.AutoSessionTracking = true;
    options.IsGlobalModeEnabled = true;
    options.EnableTracing = true;
    options.Transport = new LoggingHttpTransport(options);
    // options.Transport = new CurlTransport(options);
    // options.Transport = new InterceptingHttpTransport(options, new HttpClient());
});

Directory.CreateDirectory(Path.Combine("./envelopes/"));
for (var i = 1; i <= transactionsToSend; i++)
{
    Console.WriteLine($"Iteration {i}");
    var transaction = SentrySdk.StartTransaction("Background", "loop");
    try
    {
        if (i < errorEveryXEvents || i % errorEveryXEvents != 0)
        {
            Thread.Sleep(100);
            continue;
        }

        throw new Exception("Test exception");
    }
    catch (Exception e)
    {
        SentrySdk.CaptureException(e);
    }
    finally
    {
        transaction.Finish();
    }

    SentrySdk.Flush();
}
