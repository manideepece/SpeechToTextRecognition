using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.SpeechRecognition;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;

namespace SpeechToTextRecognition
{
    public class Program
    {
        public static MicrophoneRecognitionClient _microRecogClient;
       
        public static void Main(string[] args)
        {
            //Console.WriteLine("Press enter to start your speech");
            //Console.ReadLine();
            //ConvertSpeechToText(SpeechRecognitionMode.LongDictation, "en-US", "19ee855d47dc419e827a085193ea1b1a");
            RecognizeIntentAsync().Wait();
            Console.WriteLine("Please press Enter to continue.");
            Console.ReadLine();
        }

        static async Task RecognizeIntentAsync()
        {
            // Creates an instance of a speech config with specified subscription key
            // and service region. Note that in contrast to other services supported by
            // the Cognitive Services Speech SDK, the Language Understanding service
            // requires a specific subscription key from https://www.luis.ai/.
            // The Language Understanding service calls the required key 'endpoint key'.
            // Once you've obtained it, replace with below with your own Language Understanding subscription key
            // and service region (e.g., "westus").
            // The default language is "en-us".
            var config = SpeechConfig.FromSubscription("1bf850c2800044619a986cade3bf81e6", "westus");

            // Creates an intent recognizer using microphone as audio input.
            using (var recognizer = new IntentRecognizer(config))
            {
                // Creates a Language Understanding model using the app id, and adds specific intents from your model
                var model = LanguageUnderstandingModel.FromAppId("1071c51b-d4c2-43e0-9dc4-3c9b364290b8");
                recognizer.AddIntent(model, "YourLanguageUnderstandingIntentName1", "id1");
                recognizer.AddIntent(model, "YourLanguageUnderstandingIntentName2", "id2");
                recognizer.AddIntent(model, "YourLanguageUnderstandingIntentName3", "any-IntentId-here");

                // Starts recognizing.
                Console.WriteLine("Say something...");

                // Starts intent recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of 15
                // seconds of audio is processed.  The task returns the recognition text as result. 
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query. 
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                if (result.Reason == ResultReason.RecognizedIntent)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Console.WriteLine($"    Intent Id: {result.IntentId}.");
                    Console.WriteLine($"    Language Understanding JSON: {result.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult)}.");
                }
                else if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Console.WriteLine($"    Intent not recognized.");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }

        public static void ConvertSpeechToText(SpeechRecognitionMode mode, string language, string subscriptionKey)
        {
            _microRecogClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(mode, language, subscriptionKey);
            _microRecogClient.OnResponseReceived += OnResponseReceivedHandler;
            _microRecogClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            _microRecogClient.OnConversationError += OnConversationError;
            _microRecogClient.StartMicAndRecognition();
        }

        public static void OnConversationError(object sender, SpeechErrorEventArgs e)
        {
            Console.WriteLine("Error Code: {0}", e.SpeechErrorCode.ToString());
            Console.WriteLine("Error Text: {0}", e.SpeechErrorText);
            Console.WriteLine();
        }

        public static void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Console.WriteLine("{0} ", e.PartialResult);
            Console.WriteLine();
        }

        public static void OnResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
            {
                Console.Write("{0} ", e.PhraseResponse.Results[i].DisplayText);
            }
            Console.WriteLine();
        }
    }
}
