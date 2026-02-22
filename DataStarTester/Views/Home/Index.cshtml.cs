using StarFederation.Datastar.DependencyInjection;
using System.Text.Json.Serialization;
using DataStarTester.Models.Todos;

namespace DataStarTester.Views.Home;

public record MySignals
{
    [JsonPropertyName("formInput")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FormInput { get; init; }

    [JsonPropertyName("output")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Output { get; init; }

    [JsonPropertyName("todoInput")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TodoInput { get; init; }
}

public static class InMemoryDb
{
    public static TodoBl.TodoState CurrentState { get; set; } = TodoBl.InitialState;
}

public static class IndexEndpoints
{
    #region SignalsTest
    private static readonly MySignals defaultSignals = new() { FormInput = "", Output = "empty" };

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
            MySignals signals = await dataStarService.ReadSignalsAsync<MySignals>();
            MySignals newSignals = new() { Output = $"Your input: {signals.FormInput}" };
            await dataStarService.PatchSignalsAsync(newSignals);
        });

        app.MapPost("/resetInput", async (IDatastarService dataStarService) =>
        {
            await dataStarService.PatchSignalsAsync(defaultSignals);
        });
    }
    #endregion

    #region Todo
    [RegisterEndpoint]
    public static void RegisterIndexTodoEndpoints(WebApplication app)
    {
        var todoGroup = app.MapGroup("/examples/todomvc");
           
        todoGroup.MapGet("/init", async (IDatastarService dataStarService) => { await InitializeUi(dataStarService); });

        todoGroup.MapPut("/{todoId:int}", async (int todoId, IDatastarService dataStarService) =>
        {
            var signals = await dataStarService.ReadSignalsAsync<MySignals>();
            var newMessage = todoId switch
            {
                -1 => new TodoBl.AddTodoMessage(signals.TodoInput)
            };
            
            InMemoryDb.CurrentState = TodoBl.UpdateState(InMemoryDb.CurrentState, newMessage);
            await dataStarService.PatchElementsAsync(TodoBl.ViewUi(InMemoryDb.CurrentState));
        });

        todoGroup.MapPut("/mode/{modeId:int}", async (int modeId) =>
        {
            
        });
            
        // group.MapPut("/blah", async () => "blah");
        // group.MapPut("/reset", async () => "blah");
    }

    private static async Task InitializeUi(IDatastarService dataStarService)
    {
        var ui = TodoBl.ViewUi(InMemoryDb.CurrentState);
        await dataStarService.PatchElementsAsync(ui);
    }
        
    #endregion
}