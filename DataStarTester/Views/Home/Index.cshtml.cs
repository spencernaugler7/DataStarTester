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

        [JsonPropertyName("todoCount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TodoCount { get; init; } = null;
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
            group.MapGet("/init", async (IDatastarService dataStarService) => {

                await dataStarService.PatchElementsAsync($"""
                    <ul id="todo-list" style="">
                        {string.Join("\n", TodoEntities.DefaultTodos.Select(TodoActions.RenderTodo))}
                    </ul>
                """);

                // update our todo count
                var signals = await dataStarService.ReadSignalsAsync<MySignals>();
                var newSignals = signals with { TodoCount = TodoEntities.DefaultTodos.Count(w => w.Done == false) };
                await dataStarService.PatchSignalsAsync(newSignals);
            });

            group.MapPut("{todoId}", async (int todoId, IDatastarService datastarService) =>
            {

            });
            group.MapPut("/mode/{modeId}", async (int modeId) => "blah");
            group.MapPut("/blah", async () => "blah");
            group.MapPut("/reset", async () => "blah");
            return group;
        }
        #endregion
    }
}
