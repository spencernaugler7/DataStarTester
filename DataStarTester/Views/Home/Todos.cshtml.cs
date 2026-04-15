using System.Text.Json.Serialization;
using DataStarTester.Models.Todos;
using StarFederation.Datastar.DependencyInjection;

namespace DataStarTester.Views.Home;

public record TodoSignals
{
    [JsonPropertyName("todoInput")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TodoInput { get; init; }

    [JsonPropertyName("todoDone")]
    public bool TodoDone { get; init; }
}

public static class TodoPage
{
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
            var signals = await dataStarService.ReadSignalsAsync<TodoSignals>();
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
}