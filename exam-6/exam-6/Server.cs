using System.Net;
using RazorEngine;
using RazorEngine.Templating;

namespace exam_6;


public class Server
{
    private string _siteDirectory;
    private HttpListener _listener;
    private int _port;
    private TasksDataJson _dataJson = new TasksDataJson();

    public async Task RunAsync(string path, int port)
    {
        _siteDirectory = path;
        _port = port;
        
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:" + port + "/");
        _listener.Start();
        await ListenAsync();
    }

    private async Task ListenAsync()
    {
        try
        {
            while (true)
            {
                HttpListenerContext context =  await _listener.GetContextAsync();
                Process(context); 
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void Process(HttpListenerContext context)
    {
        
        string method = context.Request.HttpMethod;
        string filename = context.Request.Url.AbsolutePath;
        string query = context.Request.Url.Query; 
        filename = filename.Substring(1);
        filename = "../../../site/" + filename;
        byte[] buffer = new byte[32 * 1024];
        if (File.Exists(filename) && (filename.Contains(".html") || filename.Contains(".css")))
        {
            try
            {
                if (method == "POST" && filename.Contains("index.html"))
                {
                    byte[] bytes = new byte[8 * 1024];
                    int bytesCount = context.Request.InputStream.Read(bytes);
                    string request = System.Text.Encoding.UTF8.GetString(bytes, 0, bytesCount);
                    string[] dataRequest = request.Split("&");
                    string name = "";
                    string title = "";
                    string description = "";
                    string action = "";
                    string idStr = "";
                    foreach (var data in dataRequest)
                    {
                        if (data.StartsWith("title="))
                            title = data.Substring(6);
                        if (data.StartsWith("name="))
                            name = data.Substring(5);
                        if (data.StartsWith("description="))
                            description = data.Substring(12);
                        if (data.StartsWith("action="))
                            action = data.Substring(7);
                        if (data.StartsWith("id="))
                            idStr = data.Substring(3);
                    }
                    
                    List<MyTask> tasks = _dataJson.FillTasks();
                    if (tasks == null)
                        tasks = new List<MyTask>();
                    
                    if (!string.IsNullOrEmpty(action))
                    {
                        var task = tasks.FirstOrDefault(t => t.Id == Convert.ToInt32(idStr));
                        if (task != null)
                        {
                            if (action == "done")
                                task.IsDone = true;
                            else if (action == "delete")
                                tasks.Remove(task);
                        }
                    }
                    else
                    {
                        int newId;
                        if (tasks.Count > 0)
                            newId = tasks.Max(t => t.Id) + 1;
                        else
                            newId = 1;
                        MyTask newTask = new MyTask
                        {
                            Id = newId,
                            Title = title,
                            Name = name,
                            Description = description,
                            IsDone = false
                        };
                        tasks.Add(newTask);
                    }
                    _dataJson._tasks = tasks;
                    _dataJson.SaveTask();
                    
                }
                
                else if (method == "GET" && filename.Contains("task.html") && !string.IsNullOrEmpty(query))
                {
                    string taskIdStr = "";
                    if (query.StartsWith("?id="))
                        taskIdStr = query.Substring(4);

                    if (Convert.ToInt32(taskIdStr) > 0)
                    {
                        List<MyTask> tasks = _dataJson.FillTasks();
                        if (tasks == null)
                            tasks = new List<MyTask>();
                        
                        var task = tasks.FirstOrDefault(t => t.Id == Convert.ToInt32(taskIdStr));

                        if (task != null)
                        {
                            string contentForGet = BuildTaskHtml(task);
                            byte[] htmlBytesForGet = System.Text.Encoding.UTF8.GetBytes(contentForGet);

                            context.Response.ContentType = "text/html";
                            context.Response.ContentLength64 = htmlBytesForGet.Length;
                            context.Response.OutputStream.Write(htmlBytesForGet, 0, htmlBytesForGet.Length);
                            context.Response.OutputStream.Close();
                            return;
                        }
                    }

                    context.Response.StatusCode = 404;
                    context.Response.OutputStream.Close();
                    return;
                }
                else
                {
                    
                    context.Response.StatusCode = 200;
                }

                string content = "";
                if (filename.EndsWith(".html"))
                {
                    content = BuildHtml(filename);
                }
                else
                {
                    content = File.ReadAllText(filename);
                }
                byte[] htmlBytes = System.Text.Encoding.UTF8.GetBytes(content);
                Stream filestream = new MemoryStream(htmlBytes);
                context.Response.ContentType = GetContentType(filename);
                context.Response.ContentLength64 = filestream.Length;
                int dataLength;
                do
                {
                    dataLength = filestream.Read(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Write(buffer, 0, dataLength);
                }while(dataLength > 0);
                filestream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                context.Response.StatusCode = 500;
            }
        }
        else
        {
            context.Response.StatusCode = 404;
        }
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Flush();
        context.Response.OutputStream.Close();
    }

    private string GetContentType(string filename)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>()
        {
            {".css", "text/css"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".js", "application/x-javascript"},
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".json", "application/json"}
        };
        string contentType = "";
        string fileExtension = Path.GetExtension(filename);
        dictionary.TryGetValue(fileExtension, out contentType);
        return contentType;
    }

    public void Stop()
    {
        _listener.Abort();
        _listener.Stop();
    }

    private string BuildHtml(string filename)
    {
        Console.WriteLine(filename);
        string html = "";
        string layoutPath = "../../../site/layout.html";
        string filepath = filename;
        var razorService = Engine.Razor;

        if (!razorService.IsTemplateCached("layout", null))
            razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
        if (!razorService.IsTemplateCached("filename", null))
        {
            razorService.AddTemplate(filename, File.ReadAllText(filepath));
            razorService.Compile(filename);
        }
        
        List<MyTask> tasks = _dataJson.FillTasks();
        if (tasks == null)
        {
            tasks = new List<MyTask>();
        }
        html = razorService.Run(filename, null, new
        {
            Tasks = tasks
        });
        return html;
    }
    
    private string BuildTaskHtml(MyTask task)
    {
        string layoutPath = "../../../site/layout.html";
        string taskPath = "../../../site/task.html";
        var razorService = Engine.Razor;

        if (!razorService.IsTemplateCached("layout", null))
            razorService.AddTemplate("layout", File.ReadAllText(layoutPath));
        if (!razorService.IsTemplateCached("task", null))
        {
            razorService.AddTemplate("task", File.ReadAllText(taskPath));
            razorService.Compile("task");
        }

        string html = razorService.Run("task", null, task);
        return html;
    }
}