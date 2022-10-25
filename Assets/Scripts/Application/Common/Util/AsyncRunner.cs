using System.Threading.Tasks;

public static class AsyncRunner { 
    public static async void RunAsync(this Task task) {
        await task;
    }
}