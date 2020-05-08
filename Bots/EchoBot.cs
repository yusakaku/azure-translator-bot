// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
// Adding libraries
using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.EchoBot
{
    public class EchoBot : ActivityHandler
    {
        // Add Translator Text API configuration
        private readonly string translatorEndpoint = "https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=ja";
        private readonly string translatorSubscriptionKey = "fc8fb73ee7954d6eb6ee5c22174d91b6";
        private readonly string translatorSubscriptionRegion = "eastasia";


        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);

            // Get user response
            var body = new object[] { new { Text = turnContext.Activity.Text } };
            var requestBody = JsonConvert.SerializeObject(body);

            // Make web request to Translator Text API
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(translatorEndpoint);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", translatorSubscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", translatorSubscriptionRegion);

                var response = await client.SendAsync(request);

                // Get API result and return answer
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<TranslatorResult>>(jsonResponse);

                var responseMessage = MessageFactory.Text(
                    $"「{result[0].translations[0].text}」" +
                    $"(検出言語: {result[0].detectedLanguage.language})"
                );

                await turnContext.SendActivityAsync(responseMessage, cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"外国語チョットワカル Bot だよ！"), cancellationToken);
                }
            }
        }

        // Class for Translator Text API result JSON
        public class TranslatorResult
        {
            public Detectedlanguage detectedLanguage { get; set; }
            public List<Translation> translations { get; set; }
        }

        public class Detectedlanguage
        {
            public string language { get; set; }
            public float score { get; set; }
        }

        public class Translation
        {
            public string text { get; set; }
            public string to { get; set; }
        }
    }
}


