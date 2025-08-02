using System.Text.Json;

namespace exam_6;

public class TasksDataJson
{
    private const string _path = "../../../tasks.json";
    public List<MyTask> _tasks = new List<MyTask>();
    
    public  void SaveTask()
    {
        string json = JsonSerializer.Serialize(_tasks, new JsonSerializerOptions{WriteIndented = true,Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping} );
        File.WriteAllText(_path, json);
    }
    public  List<MyTask> FillTasks()
    {
        try
        {
            if (!File.Exists(_path))
            {
                Console.WriteLine("Сохраненный файл не найден");
                return null;
            }
            string json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<MyTask>>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
    }
}