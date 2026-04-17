using System.Text.Json.Serialization;
using StarFederation.Datastar.DependencyInjection;
using System.Collections.Immutable;
using RhoMicro.CodeAnalysis;

namespace DataStarTester.Views.Home;

public record TodoSignals
{
    [JsonPropertyName("todoInput")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TodoInput { get; init; }

    [JsonPropertyName("todoDone")]
    public bool TodoDone { get; init; }
}

public record TodoState(ImmutableList<Todo> Todos, DisplayMode DisplayMode);
    
public record Todo(int Id, bool Done, string Description);
    
public enum DisplayMode
{
    All,
    Pending,
    Completed
}

[UnionType<AddTodoMessage, ViewAllTodos, ViewPendingTodos, ViewCompletedTodos, ToggleTodo>]
public partial struct Message;

public record AddTodoMessage(string TodoDescription, bool TodoDone);
public record ViewAllTodos;
public record ViewPendingTodos;
public record ViewCompletedTodos;
public record ToggleTodo(int Id);

public static partial class TodosPage
{

    public static TodoState InitialState => new([ new(0, false, "Finish code"), new(1, true, "Finish Writeup") ], DisplayMode.All);

    public static class InMemoryDb
    {
        public static TodoState CurrentState { get; set; } = InitialState;
    }

    [RegisterEndpoint]
    public static void RegisterIndexTodoEndpoints(WebApplication app)
    {
        var todoGroup = app.MapGroup("/examples/todomvc");
        
        todoGroup.MapGet("/init", async (IDatastarService dataStarService) => 
        {         
            var ui = GetElementPatches(InMemoryDb.CurrentState);
            await dataStarService.PatchElementsAsync(ui); 
        });

        todoGroup.MapPut("/{todoId:int}", async (int todoId, IDatastarService dataStarService) =>
        {
            var signals = await dataStarService.ReadSignalsAsync<TodoSignals>();
            var message = todoId switch
            {
                -1 => new AddTodoMessage(signals.TodoInput, signals.TodoDone)
            };

            await UpdateUi(message, dataStarService);
        });

        todoGroup.MapPut("/mode/{modeId:int}", async (int modeId, IDatastarService datastarService) =>
        {
            Message message = modeId switch
            {
                0 => new ViewAllTodos(),
                1 => new ViewPendingTodos(),
                2 => new ViewCompletedTodos(),
                _ => new ViewAllTodos()
            };
            await UpdateUi(message, datastarService);
        });
            
        todoGroup.MapPut("/toggle/{todoId:int}", async (int todoId, IDatastarService datastarService) => 
        {
            await UpdateUi(new ToggleTodo(todoId), datastarService);
        });
    }

    private static async Task UpdateUi(Message message, IDatastarService datastarService)
    {
        InMemoryDb.CurrentState = UpdateState(InMemoryDb.CurrentState, message);
        var patches = GetElementPatches(InMemoryDb.CurrentState);
        await datastarService.PatchElementsAsync(patches);
    }

    public static TodoState UpdateState(TodoState state, Message message) => message.Switch(
        newTodo =>
        {
            var newId = state.Todos.Max(m => m.Id) + 1;
            var todo = new Todo(newId, newTodo.TodoDone, newTodo.TodoDescription);
            return state with
            {
                Todos = state.Todos.Add(todo)
            };
        },
        toggleTodos => {
            var todoToReplace = state.Todos.FirstOrDefault(f => f.Id == toggleTodos.Id);
            return state with { Todos = state.Todos.Replace(todoToReplace, todoToReplace with { Done = !todoToReplace.Done }) };
        },
        viewAllTodos =>  state with { DisplayMode = DisplayMode.All },
        viewCompletedTodos => state with { DisplayMode = DisplayMode.Completed },
        viewPendingTodos => state with { DisplayMode = DisplayMode.Pending }
    );

    public static string GetElementPatches(TodoState state)
    {
        var renderedTodos = state.Todos
            .Where(w => FilterTodo(w, state.DisplayMode))
            .Select(RenderTodo)
            .DefaultIfEmpty(string.Empty);

        var renderedTodoList = string.Join("\n", renderedTodos);
        
        var pendingText =  state.Todos.Count(w => !w.Done) switch
        {
            0 => "Congrats!! All done!",
            1 => "1 pending todo",
            var count => $"{count} pending todos"
        };

        return /*lang=html*/$"""
            <ul id="todo-list" style="">
                {renderedTodoList}
            </ul>
            <strong id="todoCount">{pendingText}</strong>
        """;
    }

    private static bool FilterTodo(Todo todo, DisplayMode displayMode) => displayMode switch
    {
        DisplayMode.Pending => !todo.Done,
        DisplayMode.Completed => todo.Done,
        DisplayMode.All => true,
        _ => true
    };

    public static string RenderTodo(Todo todo) => /*lang=html*/$"""
        <div id="todoId{todo.Id}" style="display: flex; flex-direction: row; gap: 5px;">
            <input type="checkbox" {(todo.Done ? "checked" : "")} data-on:change="@put('/examples/todomvc/toggle/{todo.Id}')" />
            <textbox>{todo.Description}</textbox>
        </div>
    """;
}