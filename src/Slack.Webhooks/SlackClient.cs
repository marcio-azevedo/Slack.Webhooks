using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using RestSharp;

#if NET40
using System.Threading.Tasks;
using RestSharp.Extensions;
#endif

namespace Slack.Webhooks
{
    public class SlackClient
    {
        private readonly RestClient _restClient;
        private readonly Uri _webhookUri;

        private const string VALID_HOST = "hooks.slack.com";
        private const string POST_SUCCESS = "ok";

        /// <summary>
        /// Returns the RestClient's current Timeout value.
        /// </summary>
        internal int TimeoutMs { get { return _restClient.Timeout; } }

        public SlackClient(string webhookUrl, int timeoutSeconds = 100)
        {
            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _webhookUri))
                throw new ArgumentException("Please enter a valid Slack webhook url");

            if (_webhookUri.Host != VALID_HOST)
                throw new ArgumentException("Please enter a valid Slack webhook url");

            var baseUrl = _webhookUri.GetLeftPart(UriPartial.Authority);
            _restClient = new RestClient(baseUrl);
            _restClient.Timeout = timeoutSeconds * 1000;
        }

        public virtual bool Post(SlackMessage slackMessage)
        {
            
            var request = new RestRequest(_webhookUri.PathAndQuery, Method.POST);

            var payload = SimpleJson.SerializeObject(slackMessage, new SnakeJsonSerializerStrategy());
            request.AddParameter("payload", payload);

            try
            {
                var response = _restClient.Execute(request);
                return response.StatusCode == HttpStatusCode.OK && response.Content == POST_SUCCESS;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public bool PostToChannels(SlackMessage message, IEnumerable<string> channels)
        {
            return channels.DefaultIfEmpty(message.Channel)
                    .Select(message.Clone)
                    .Select(Post).All(r => r);
        }

#if NET40
        public IEnumerable<Task<IRestResponse>> PostToChannelsAsync(SlackMessage message, IEnumerable<string> channels)
        {
            return channels.DefaultIfEmpty(message.Channel)
                                .Select(message.Clone)
                                .Select(PostAsync);
        }

        public Task<IRestResponse> PostAsync(SlackMessage slackMessage)
        {
            SimpleJson.CurrentJsonSerializerStrategy = new SnakeJsonSerializerStrategy();
            var request = new RestRequest(_webhookUri.PathAndQuery, Method.POST);

            var payload = SimpleJson.SerializeObject(slackMessage, new SnakeJsonSerializerStrategy());
            request.AddParameter("payload", payload);

            return ExecuteTaskAsync(request);
        }

        private Task<IRestResponse> ExecuteTaskAsync(IRestRequest request)
        {
            var taskCompletionSource = new TaskCompletionSource<IRestResponse>();
            try
            {
                _restClient.ExecuteAsync(request, (response, _) =>
                {
                    if (response.ErrorException != null)
                        taskCompletionSource.TrySetException(response.ErrorException);
                    else if (response.ResponseStatus != ResponseStatus.Completed)
                        taskCompletionSource.TrySetException(response.ResponseStatus.ToWebException());
                    else
                        taskCompletionSource.TrySetResult(response);
                });
            }
            catch (Exception ex)
            {
                taskCompletionSource.TrySetException(ex);
            }
            return taskCompletionSource.Task;
        }
#endif
    }

    class SnakeJsonSerializerStrategy : PocoJsonSerializerStrategy
    {
        protected override string MapClrMemberNameToJsonFieldName(string clrPropertyName)
        {
            return string.Concat(clrPropertyName.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString().ToLower() : x.ToString().ToLower()));
        }
    }
}
