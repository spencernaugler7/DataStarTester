using StarFederation.Datastar.DependencyInjection;
using System.Text.Json.Serialization;

namespace DataStarTester.Views.Home;

public record IndexSignals
{
    [JsonPropertyName("formInput")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FormInput { get; init; }

    [JsonPropertyName("output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Output { get; init; }
}

public static class IndexEndpoints
{
    private static IndexSignals DefaultSignals { get; } = new() { FormInput = "", Output = "empty" };

    [RegisterEndpoint]
    public static void RegisterIndexMainEndpoints(WebApplication app)
    {
        app.MapGet("/displayDate", async (IDatastarService dataStarService) => await dataStarService.PatchElementsAsync( /*lang=html*/$"""
           <div id='target'>
               <span id='date'>
                   <b>{DateTime.Now.ToString("%y-%M-%d %h:%m:%s")}</b>
                   <button data-on:click="@get('/removeDate')">Remove</button>
               </span>
           </div>
          """));

        app.MapGet("/removeDate", async (IDatastarService dataStarService) => await dataStarService.RemoveElementAsync("#date"));

        app.MapPost("/changeOutput", async (IDatastarService dataStarService) =>
        {
            var signals = await dataStarService.ReadSignalsAsync<IndexSignals>();
            var output = string.IsNullOrEmpty(signals.FormInput) ? "Type in input to get output" : $"Your input {signals.FormInput}";
            IndexSignals newSignals = new() { Output = output };
            await dataStarService.PatchSignalsAsync(newSignals);
        });

        app.MapPost("/resetInput", async (IDatastarService dataStarService) =>
        {
            await dataStarService.PatchSignalsAsync(DefaultSignals);
        });
    }
}