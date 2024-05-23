namespace PerfUnits3283;

public class LoggingHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("Request:");
        Console.WriteLine(request.ToString());
        if (request.Content != null)
        {
            Console.WriteLine(await request.Content.ReadAsStringAsync(cancellationToken));
        }
        Console.WriteLine();

        var response = await base.SendAsync(request, cancellationToken);

        Console.WriteLine("Response:");
        Console.WriteLine(response.ToString());
        Console.WriteLine(await response.Content.ReadAsStringAsync(cancellationToken));
        Console.WriteLine();

        return response;
    }
}