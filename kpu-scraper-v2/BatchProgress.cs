namespace kpu_scraper_v2;

public class BatchProgress : IProgress<int>
{
    private readonly int _totalTasks;
    private int _completedTasks;
    private const int BatchSize = 10;
    private readonly string _message;

    public BatchProgress(string message, int totalTasks)
    {
        _totalTasks = totalTasks;
        _message = message;
    }

    public void Report(int value)
    {
        _completedTasks++;

        if (_completedTasks % BatchSize != 0 && _completedTasks != _totalTasks) return;
        var percentComplete = ((double)_completedTasks * 100) / _totalTasks; 
        Console.Write($"\r{_message}: {percentComplete:0.00}% complete");
    }
}