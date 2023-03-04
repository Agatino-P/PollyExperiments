using OneOf.Types;
namespace PollyExperiments;

public class Unreliable
{
    public string? Unpredictable()
    {
        string fileContent = File.ReadAllText("trigger.txt");
        return fileContent switch
        {
            "Null" => default,
            _ => fileContent
        };

    }
}
