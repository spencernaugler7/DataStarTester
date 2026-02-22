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

    [UnionType<AddTodoMessage, ViewAllTodos, ViewPendingTodos, ViewCompletedTodos, ToggleTodo>]
    public partial struct Message;

    public record AddTodoMessage(string TodoDescription, bool TodoDone);
    public record ViewAllTodos;
    public record ViewPendingTodos;
    public record ViewCompletedTodos;
    public record ToggleTodo(int Id);

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