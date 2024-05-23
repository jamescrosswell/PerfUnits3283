using Sentry;
using Sentry.Extensibility;
using Sentry.Http;
using Sentry.Protocol.Envelopes;

namespace PerfUnits3283;

public class LoggingHttpTransport: HttpTransportBase, ITransport
{
    private readonly HttpClient _httpClient;

    public LoggingHttpTransport(SentryOptions options)
        : base(options)
    {
        _httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler()));

    }

    /// <summary>
    /// Sends an envelope over this transport.
    /// </summary>
    /// <param name="envelope">The envelope to send.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <remarks>
    /// This method implements the overarching workflow, but all features are implemented in the base class
    /// such that they can be shared with higher-level SDKs (such as Unity) that may implement their own method
    /// for performing HTTP transport.
    /// </remarks>
    public virtual async Task SendEnvelopeAsync(Envelope envelope, CancellationToken cancellationToken = default)
    {
        using var processedEnvelope = ProcessEnvelope(envelope);
        if (processedEnvelope.Items.Count > 0)
        {
            using var request = CreateRequest(processedEnvelope);
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            await HandleResponseAsync(response, processedEnvelope, cancellationToken).ConfigureAwait(false);
        }
    }
}
