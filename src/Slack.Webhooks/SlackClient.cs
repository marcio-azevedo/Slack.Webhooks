using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using RestSharp;
using ServiceStack.Text;

#if NET40
using RestSharp.Extensions;
using System.Threading.Tasks;
#endif

namespace Slack.Webhooks
{
    public class SlackClient
    {
        private readonly RestClient _restClient;
        private readonly Uri _webhookUri;

        private const string VALID_HOST = "hooks.slack.com";
        private const string POST_SUCCESS = "ok";

        public SlackClient(string webhookUrl)
        {
            JsConfig.EmitLowercaseUnderscoreNames = true;
            JsConfig.IncludeNullValues = false;
            SetPropertyConvention();

            if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out _webhookUri))
                throw new ArgumentException("Please enter a valid Slack webhook url");
           
            if (_webhookUri.Host != VALID_HOST)
                throw new ArgumentException("Please enter a valid Slack webhook url");

            var baseUrl = _webhookUri.GetLeftPart(UriPartial.Authority);
            _restClient = new RestClient(baseUrl);
        }

        private void SetPropertyConvention()
        {
            var assembly = Assembly.GetAssembly(typeof(JsConfig));
            var jsConfig = assembly.GetType("ServiceStack.Text.JsConfig");
            var conventionEnum = assembly.GetType("ServiceStack.Text.JsonPropertyConvention") ??
                           assembly.GetType("ServiceStack.Text.PropertyConvention");
            var propertyConvention = jsConfig.GetProperty("PropertyConvention");

            var lenient = conventionEnum.GetField("Lenient");

            var enumValue = Enum.ToObject(conventionEnum, (int) lenient.GetValue(conventionEnum));
            propertyConvention.SetValue(jsConfig, enumValue, null);
        }

        public bool Post(SlackMessage slackMessage)
        {
            var request = new RestRequest(_webhookUri.PathAndQuery, Method.POST);

            request.AddParameter("payload", JsonSerializer.SerializeToString(slackMessage));
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
            var request = new RestRequest(_webhookUri.PathAndQuery, Method.POST);

            request.AddParameter("payload", JsonSerializer.SerializeToString(slackMessage));

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
}