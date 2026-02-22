using System.Collections.Immutable;
using RhoMicro.CodeAnalysis;

namespace DataStarTester.Models.Todos;

public static partial class TodoBl
{
    public record TodoState(ImmutableList<Todo> Todos, DisplayMode DisplayMode);
    
    public static TodoState InitialState => new([ new(0, false, "Finish code"), new(1, true, "Finish Writeup") ], DisplayMode.All);
        
    public record Todo(int Id, bool Done, string Description);
        
    public enum DisplayMode
    {
        All,
        Pending,
        Completed
    }

    [UnionType<AddTodoMessage>]
    public partial struct Message;

    public record AddTodoMessage(string TodoDescription);

    public static TodoState UpdateState(TodoState state, Message message) => message.Switch(newTodo =>
        {
            var newId = state.Todos.Max(m => m.Id) + 1;
            var todo = new Todo(newId, false, newTodo.TodoDescription);
            return state with
            {
                Todos = state.Todos.Add(todo)
            };
        });


    public static string ViewUi(TodoBl.TodoState state) =>
        /*lang=html*/$"""
                          <ul id="todo-list" style="">
                              {string.Join("\n", state.Todos.Select(RenderTodo))}
                          </ul>
                          <strong id="todoCount">{state.Todos.Count(c => !c.Done)}</strong>
                      """;

    public static string RenderTodo(TodoBl.Todo todo) => 
        /*lang=html*/$"""
                          <div style="display: flex; flex-direction: row; gap: 5px;">
                              <input type="checkbox" {(todo.Done ? "checked" : "")} data-on:change="@post('/update')" />
                              <textbox>{todo.Description}</textbox>
                          </div>
                      """;
}