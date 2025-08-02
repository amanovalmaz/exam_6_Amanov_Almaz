using System.Net;
using RazorEngine;
using RazorEngine.Templating;

namespace exam_6;


public class Server
{
    private string _siteDirectory;
    private HttpListener _listener;
    private int _port;

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
        filename = filename.Substring(1);
        filename = "../../../site/" + filename;
        byte[] buffer = new byte[32 * 1024];
        if (File.Exists(filename) && (filename.Contains(".html") || filename.Contains(".css")))
        {
            try
            {

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
        html = razorService.Run(filename, null, new
        {
            
            
        });
        return html;
    }
}