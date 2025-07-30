using System.Net.Http.Headers;
using Azure.Core;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace McDContactManager.Service;

public class GraphService
{
    private readonly GraphServiceClient _graphClient;

    public GraphService(TokenCredential  credential)
    {
        _graphClient = new GraphServiceClient(credential);
    }

    public async Task<List<Message>> GetEmailsAsync(int top = 10)
    {
        try
        {
            var response = await _graphClient.Me.Messages.GetAsync(options =>
            {
                options.QueryParameters.Top = top;
                options.QueryParameters.Select = ["subject", "receivedDateTime", "bodyPreview"];
            });

            return response?.Value?.ToList() ?? new List<Message>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Graph API error: {ex.Message}");
            return new List<Message>();
        }
    }
}