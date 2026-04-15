using StarFederation.Datastar.DependencyInjection;
using System.Text.Json.Serialization;
using DataStarTester.Models.Todos;
using StarFederation.Datastar.ModelBinding;

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

    [JsonPropertyName("todoDone")]
    public bool TodoDone { get; init; }
}

public static class InMemoryDb
{
    public static TodoBl.TodoState CurrentState { get; set; } = TodoBl.InitialState;
}

public static class IndexEndpoints
{
    #region SignalsTest
    private static MySignals DefaultSignals { get; } = new() { FormInput = "", Output = "empty" };

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
            var signals = await dataStarService.ReadSignalsAsync<MySignals>();
            var output = string.IsNullOrEmpty(signals.FormInput) ? "Type in input to get output" : $"Your input {signals.FormInput}";
            MySignals newSignals = new() { Output = output };
            await dataStarService.PatchSignalsAsync(newSignals);
        });

        app.MapPost("/resetInput", async (IDatastarService dataStarService) =>
        {
            await dataStarService.PatchSignalsAsync(DefaultSignals);
        });
    }
    #endregion

    #region Todo
    [RegisterEndpoint]
    public static void RegisterIndexTodoEndpoints(WebApplication app)
    {
        var todoGroup = app.MapGroup("/examples/todomvc");
           
        todoGroup.MapGet("/init", async (IDatastarService dataStarService) => 
        {         
            var ui = TodoBl.GetElementPatches(InMemoryDb.CurrentState);
            await dataStarService.PatchElementsAsync(ui); 
        });

        todoGroup.MapPut("/{todoId:int}", async (int todoId, IDatastarService dataStarService) =>
        {
            var signals = await dataStarService.ReadSignalsAsync<MySignals>();
            var message = todoId switch
            {
                -1 => new TodoBl.AddTodoMessage(signals.TodoInput, signals.TodoDone)
            };

            await UpdateUi(message, dataStarService);
        });

        todoGroup.MapPut("/mode/{modeId:int}", async (int modeId, IDatastarService datastarService) =>
        {
            TodoBl.Message message = modeId switch
            {
                0 => new TodoBl.ViewAllTodos(),
                1 => new TodoBl.ViewPendingTodos(),
                2 => new TodoBl.ViewCompletedTodos(),
                _ => new TodoBl.ViewAllTodos()
            };
            await UpdateUi(message, datastarService);
        });
            
        todoGroup.MapPut("/toggle/{todoId:int}", async (int todoId, IDatastarService datastarService) => 
        {
            await UpdateUi(new TodoBl.ToggleTodo(todoId), datastarService);
        });
    }

    private static async Task UpdateUi(TodoBl.Message message, IDatastarService datastarService)
    {
        InMemoryDb.CurrentState = TodoBl.UpdateState(InMemoryDb.CurrentState, message);
        var patches = TodoBl.GetElementPatches(InMemoryDb.CurrentState);
        await datastarService.PatchElementsAsync(patches);
    }
    #endregion
}