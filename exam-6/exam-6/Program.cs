using exam_6;

string path = "../../../site/";
Server server = new Server();
await server.RunAsync(path, 8080);