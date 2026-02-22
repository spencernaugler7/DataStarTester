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

    public static TodoState UpdateState(TodoState state, Message message)
    {
        message.Switch(newTodo =>
        {
            var newId = state.Todos.Max(m => m.Id) + 1;
            var todo = new Todo(newId, false, newTodo.TodoDescription);
            return state with
            {
                Todos = state.Todos.Add(todo)
            };
        });
        
        return state;
    }
}