using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace Slack.Webhooks
{
    public interface ISlackClient
    {
        bool Post(SlackMessage slackMessage);
        Task<IRestResponse> PostAsync(SlackMessage slackMessage);
        bool PostToChannels(SlackMessage message, IEnumerable<string> channels);
        IEnumerable<Task<IRestResponse>> PostToChannelsAsync(SlackMessage message, IEnumerable<string> channels);
    }
}