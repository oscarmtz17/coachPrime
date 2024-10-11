public class HelloWorldService : IHelloWorldService
{
    public string GetHelloWorld()
    {
        return "Hello world";
    }
}

public interface IHelloWorldService
{
    string GetHelloWorld();
}