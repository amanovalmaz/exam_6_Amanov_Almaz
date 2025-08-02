namespace exam_6;

public class MyTask
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Name { get; set; }
    public string CreateDate { get; set; }
    public DateTime CompletedDate { get; set; }
    public bool IsDone { get; set; }
    public string Description { get; set; }
    public static int _idCount = 1;

    public MyTask()
    {
        Id = _idCount;
        _idCount++;
        CreateDate = DateTime.Today.ToShortDateString();
    }
}