using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Sentry;
using Sentry.Extensibility;
using Sentry.Http;
using Sentry.Protocol.Envelopes;

namespace PerfUnits3283;

public sealed class CurlTransport(SentryOptions options) : HttpTransportBase(options), ITransport
{
    public async Task SendEnvelopeAsync(Envelope envelope, CancellationToken cancellationToken = default)
    {
        using var processedEnvelope = ProcessEnvelope(envelope);
        if (processedEnvelope.Items.Count == 0)
        {
            return;
        }

        var dsn = Dsn.Parse(options.Dsn!);
        var envelopeContents = await GetJsonEnvelope(envelope, cancellationToken);
        var arguments = $"""
                            -v -X "POST" "{dsn.GetEnvelopeEndpointUri()}" \
                            -H 'Content-Type: application/x-sentry-envelope' \
                            -H 'X-Sentry-Auth: Sentry sentry_version=7, sentry_key={dsn.PublicKey}' \
                            --trace-ascii /dev/stdout \
                            -d $'{envelopeContents}'
                         """;
        var startInfo = new ProcessStartInfo
        {
            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "curl.exe" : "curl",
            Arguments = arguments,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var result = new StringBuilder();
        result.Append($"CurlTransport Response {envelope.TryGetEventId()}");
        
        var process = new Process { StartInfo = startInfo };
        process.Start();
        // while (!process.StandardOutput.EndOfStream)
        // {
        //     if (await process.StandardOutput.ReadLineAsync(cancellationToken) is {} line)
        //     {
        //         result.Append(line);
        //     }
        // }
        // options.DiagnosticLogger?.LogDebug(result.ToString());
    }
    
    async Task<string> GetJsonEnvelope(Envelope envelope, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await envelope.SerializeAsync(memoryStream, null, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(memoryStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }
    
    async Task<string> EscapeDoubleQuotesAsync(string input, CancellationToken cancellationToken = default)
    {
        var escaped = new StringBuilder();
        foreach (var c in input)
        {
            if (c == '"')
            {
                // Double the quotes for bash " escaping
                escaped.Append(c);
            }
            escaped.Append(c);
        }
        return await Task.FromResult(escaped.ToString());
    }
}