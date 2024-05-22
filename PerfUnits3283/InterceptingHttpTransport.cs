using System.ComponentModel;
using Sentry;
using Sentry.Extensibility;
using Sentry.Http;
using Sentry.Protocol.Envelopes;

namespace PerfUnits3283;

public sealed class InterceptingHttpTransport : HttpTransportBase, ITransport
{
    private readonly SentryOptions _options;
    private readonly HttpClient _httpClient;

    public InterceptingHttpTransport(SentryOptions options, HttpClient httpClient)
        : base(options)
    {
        _options = options;
        _httpClient = httpClient;
    }
    
    public async Task SendEnvelopeAsync(Envelope envelope, CancellationToken cancellationToken = default)
    {
        using var processedEnvelope = ProcessEnvelope(envelope);
        if (processedEnvelope.Items.Count == 0)
        {
            return;
        }
        
        // Capture envelopes for events, rather than sending these. We want to capture these and send them via cURL,
        // for debugging.
        if (ContainsEnvelopeItemOfType("event") && !ContainsEnvelopeItemOfType("transaction"))
        {
            _options.DiagnosticLogger?.LogDebug("Capturing error event envelope for debug. Envelope not sent.");
            StoreEnvelope(envelope, "event");
            return;
        }
        
        using var request = CreateRequest(processedEnvelope);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await HandleResponseAsync(response, processedEnvelope, cancellationToken).ConfigureAwait(false);
        return;
        
        bool ContainsEnvelopeItemOfType(string typeValue) => envelope.Items.Any(item => item.TryGetType() == typeValue);
    }

    void StoreEnvelope(ISerializable envelope, string fileName)
    {
        // Store the envelope
        var envelopeFilePath = Path.Combine(
            "./envelopes/",
            $"{fileName}{Guid.NewGuid().GetHashCode() % 10_000_000}.envelope");
        Console.WriteLine("Storing file {0}.", envelopeFilePath);
        // We need a memory stream since the envelope header won't include a time sent when
        // serialising to a file stream
        using var memoryStream = new MemoryStream();
        try
        {
            envelope.Serialize(memoryStream, null);
            memoryStream.Seek(0, SeekOrigin.Begin);
            using FileStream file = new FileStream(envelopeFilePath, FileMode.Create, FileAccess.Write);
            memoryStream.WriteTo(file);
        }
        finally
        {
            memoryStream.Close();
        }

        Console.WriteLine("File stored: {0}.", envelopeFilePath);
    }    
}