using System.Diagnostics;
using System.Text.Json.Serialization;
using StarFederation.Datastar.DependencyInjection;

namespace DataStarTester.Views.Sample
{

    public record Signals
    {
        [JsonPropertyName("delay")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? Delay { get; set; } = null;
    }
    
    public static class Sample 
    {
        [RegisterEndpoint]
        public static void RegisterSampleEndpoints(WebApplication app)
        {
            app.MapGet("/stream-element-patches", async (IDatastarService datastarService) =>
            {
                const string message = "Hello, Elements!";
                Signals? mySignals = await datastarService.ReadSignalsAsync<Signals>();
                Debug.Assert(mySignals != null, nameof(mySignals) + " != null");

                await datastarService.PatchSignalsAsync(new { show_patch_element_message = true });

                for (var index = 1; index < message.Length; ++index)
                {
                    await datastarService.PatchElementsAsync($"""<div id="message">{message[..index]}</div>""");
                    await Task.Delay(TimeSpan.FromMilliseconds(mySignals.Delay.GetValueOrDefault(0)));
                }

                await datastarService.PatchElementsAsync($"""<div id="message">{message}</div>""");
            });

            app.MapGet("/stream-signal-patches", async (IDatastarService datastarService) =>
            {
                const string message = "Hello, Signals!";
                Signals? mySignals = await datastarService.ReadSignalsAsync<Signals>();
                Debug.Assert(mySignals != null, nameof(mySignals) + " != null");

                await datastarService.PatchSignalsAsync(new { show_patch_element_message = false });

                for (var index = 1; index < message.Length; ++index)
                {
                    await datastarService.PatchSignalsAsync(new { signals_message = message[..index] });
                    await Task.Delay(TimeSpan.FromMilliseconds(mySignals.Delay.GetValueOrDefault(0)));
                }

                await datastarService.PatchSignalsAsync(new { signals_message = message });
            });

            app.MapGet("/execute-script", (IDatastarService datastarService) =>
                datastarService.ExecuteScriptAsync("alert('Hello! from the server 🚀')"));
        }
    }
}