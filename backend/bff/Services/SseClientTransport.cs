using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace SkyLab.Backend.Services;

/// <summary>
/// Adapts Server-Sent Events (SSE) to the IClientTransport interface required by the MCP SDK.
/// Uses a custom robust SSE parser to handle the event stream.
/// </summary>
public class SseClientTransport : IClientTransport
{
    private readonly Uri _url;
    private readonly HttpClient _httpClient;
    
    public string Name => "SSE";

    public SseClientTransport(Uri url, HttpClient httpClient)
    {
        _url = url;
        _httpClient = httpClient;
    }

    public async Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var transport = new SseTransport(_url, _httpClient);
        await transport.ConnectAsync(cancellationToken);
        return transport;
    }

    private class SseTransport : ITransport
    {
        private readonly Uri _sseUrl;
        private readonly HttpClient _httpClient;
        private readonly Channel<JsonRpcMessage> _channel;
        private Uri? _postUrl;
        private CancellationTokenSource? _cts;
        private Task? _readLoop;

        public SseTransport(Uri url, HttpClient httpClient)
        {
            _sseUrl = url;
            _httpClient = httpClient;
            _channel = Channel.CreateUnbounded<JsonRpcMessage>();
        }

        // SessionId is not strictly required for this client implementation
        public string SessionId => string.Empty;

        public ChannelReader<JsonRpcMessage> MessageReader => _channel.Reader;

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _readLoop = ReadSseLoop(_cts.Token);
            
            // Allow brief time for connection establishment
            await Task.Delay(50, cancellationToken);
        }

        private async Task ReadSseLoop(CancellationToken token)
        {
            try
            {
                using var stream = await _httpClient.GetStreamAsync(_sseUrl, token);
                using var reader = new StreamReader(stream);

                while (!token.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(token);
                    if (line == null) break;

                    if (line.StartsWith("event: endpoint"))
                    {
                        var dataLine = await reader.ReadLineAsync(token);
                        if (dataLine?.StartsWith("data: ") == true)
                        {
                            var endpoint = dataLine.Substring(6).Trim();
                            _postUrl = Uri.TryCreate(endpoint, UriKind.Absolute, out var abs) 
                                ? abs 
                                : new Uri(_sseUrl, endpoint);
                        }
                    }
                    else if (line.StartsWith("event: message"))
                    {
                        var dataLine = await reader.ReadLineAsync(token);
                        if (dataLine?.StartsWith("data: ") == true)
                        {
                            var json = dataLine.Substring(6);
                            try
                            {
                                var message = JsonSerializer.Deserialize<JsonRpcMessage>(json);
                                if (message != null)
                                {
                                    await _channel.Writer.WriteAsync(message, token);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[SSE] Parse Error: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.WriteLine($"[SSE] Stream Error: {ex.Message}");
                _channel.Writer.TryComplete(ex);
            }
            finally
            {
                _channel.Writer.TryComplete();
            }
        }

        public async Task SendMessageAsync(JsonRpcMessage message, CancellationToken cancellationToken)
        {
            // Default to /message if no endpoint event received yet
            var targetUrl = _postUrl ?? new Uri(_sseUrl, "/message");
            
            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(targetUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            if (_readLoop != null)
            {
                try { await _readLoop; } catch {}
            }
            _cts?.Dispose();
        }
    }
}