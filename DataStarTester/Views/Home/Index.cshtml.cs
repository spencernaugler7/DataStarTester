using StarFederation.Datastar.DependencyInjection;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace DataStarTester.Views.Home
{
    public record MySignals
    {
        [JsonPropertyName("formInput")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FormInput { get; init; } = null;

        [JsonPropertyName("output")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Output { get; init; } = null;

        [JsonPropertyName("todoInput")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TodoInput { get; init; } = null;
    }


    public static class TodoBusinessLogic
    {
        public record Todo(int Id, bool Done, string Description);

        public static string RenderTodo(this Todo todo) => $"""
            <div style="display: flex; flex-direction: row; gap: 5px;">
            <input type="checkbox" {(todo.Done ? "checked" : "")} data-on:change="@post('/update'" />
            <textbox>{todo.Description}</textbox>
            </div>
        """;

        public static Todo NewTodo(string todoInput)
        {
            int maxId = Todos.Max(s => s.Id);
            int newId = maxId++;

            var newTodo = new Todo(newId, false, todoInput);
            return newTodo;
        }

        public static ImmutableList<Todo> Todos =
        [
            new(0, false, "Finish code"),
            new(1, true, "Finish Writeup")
        ];
    }

    public static class IndexEndpoints
    {
        #region SignalsTest
        private static readonly MySignals defaultSignals = new() { FormInput = "", Output = "empty" };

        [RegisterEndpoint]
        public static void RegisterIndexMainEndpoints(WebApplication app)
        {
            app.MapGet("/displayDate", async (IDatastarService dataStarService) =>
            {
                string today = DateTime.Now.ToString("%y-%M-%d %h:%m:%s");
                await dataStarService.PatchElementsAsync(/*lang=html*/$"""
                    <div id='target'><span id='date'><b>{today}</b><button data-on:click="@get('/removeDate')">Remove</button></span></div>
                """);
            });

            app.MapGet("/removeDate", async (IDatastarService dataStarService) =>
            {
                await dataStarService.RemoveElementAsync("#date");
            });

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
            app.MapGroup("/examples/todomvc")
                .RegisterTodoActions();
        }

        private static RouteGroupBuilder RegisterTodoActions(this RouteGroupBuilder group)
        {
            group.MapGet("/init", async (IDatastarService dataStarService) => {
                var ui = UpdateUi(TodoBusinessLogic.Todos);
                await dataStarService.PatchElementsAsync(ui);
            });

            group.MapPut("/{todoId:int}", async (int todoId, IDatastarService dataStarService) =>
            {
                var signals = await dataStarService.ReadSignalsAsync<MySignals>();
                if (todoId == -1)
                {
                    var newTodo = TodoBusinessLogic.NewTodo(signals.TodoInput);
                    TodoBusinessLogic.Todos = TodoBusinessLogic.Todos.Append(newTodo).ToImmutableList();
                }
                var newUi = UpdateUi(TodoBusinessLogic.Todos);

                await dataStarService.PatchElementsAsync(newUi);
            });

            // group.MapPut("/mode/{modeId}", async (int modeId) => "blah");
            // group.MapPut("/blah", async () => "blah");
            // group.MapPut("/reset", async () => "blah");
            return group;
        }

        private static string UpdateUi(ImmutableList<TodoBusinessLogic.Todo> todos)
        {
            var todoCount = todos.Count(w => !w.Done);
            return $"""
                <ul id="todo-list" style="">
                    {string.Join("\n", todos.Select(s => s.RenderTodo()))}
                </ul>
                <strong id="todoCount">{todoCount}</strong>
            """;
        }

        #endregion
    }
}
