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
    
    public MyTask()
    {
        CreateDate = DateTime.Today.ToShortDateString();
    }
}