using DataStarTester.Views.Home.TodosNamespace;
using Microsoft.AspNetCore.Components.Forms;
using StarFederation.Datastar.DependencyInjection;
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

        //public string Serialize() => ...
    }

    namespace TodosNamespace
    {
        public record Todo(bool Done, string Description);

        public static class TodoEntities
        {
            public static readonly List<Todo> DefaultTodos =
            [
                new(false, "Finish code"),
                new(true, "Finish Writeup")
            ];
        }

        public static class TodoActions
        {
            public static string RenderTodo(Todo todo) => $"""
                <div style="display: flex; flex-direction: row; gap: 5px;">
                <input type="checkbox" {(todo.Done ? "checked" : "")} data-on:change="@post('/update'" />
                <textbox>{todo.Description}</textbox>
                </div>
            """;
        }
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

            static string RenderTodoActions(List<Todo> TodoState) => $"""
                <span>
                    <strong>{TodoState.Count(w => w.Done == false)}</strong> items pending
                </span>
                <button class="small info" data-on:click="@@put('/examples/todomvc/mode/0')">
                    All
                </button>
                <button class="small" data-on:click="@@put('/examples/todomvc/mode/1')">
                    Pending
                </button>
                <button class="small" data-on:click="@@put('/examples/todomvc/mode/2')">
                    Completed
                </button>
                <button class="error small" aria-disabled="true">
                    Delete
                </button>
                <button class="warning small" data-on:click="@@put('/examples/todomvc/reset')">
                    Reset
                </button>
            """;

            group.MapGet("/updates", async (IDatastarService dataStarService) => await dataStarService.PatchElementsAsync($"""
            <ul id="todo-list" style="">
                {string.Join("\n", TodoEntities.DefaultTodos.Select(TodoActions.RenderTodo))}
            </ul>
            <div id="todo-actions">
                {RenderTodoActions(TodoEntities.DefaultTodos)}
            </div>
        """));

            group.MapPut("/mode/{id}", async () => "blah");
            group.MapPut("/blah", async () => "blah");
            group.MapPut("/reset", async () => "blah");
            return group;
        }
        #endregion
    }
}
