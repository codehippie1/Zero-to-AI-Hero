using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "<Your_Deployment_Name>",
    endpoint: "<Azure-Deployment-Endpoint-Ends-In:openai.azure.com>",
    apiKey: "<Your_API_Key>"
);

builder.Plugins.AddFromType<TripPlanner>(); // <----- This is anew fellow on this Part 3 - TripPlanner. Let's add it to the Kernel
builder.Plugins.AddFromType<TimeTeller>(); // <----- This is the same fellow plugin from Part 2
builder.Plugins.AddFromType<ElectricCar>(); // <----- This is the same fellow plugin from Part 2
builder.Plugins.AddFromType<WeatherForecaster>(); // <----- New plugin. We don't want to end up in beach with rain, right?
var kernel = builder.Build();

IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

ChatHistory chatMessages = new ChatHistory("""
You are a friendly assistant who likes to follow the rules. You will complete required steps
and request approval before taking any consequential actions. If the user doesn't provide
enough information for you to complete a task, you will keep asking questions until you have
enough information to complete the task.
""");


while (true)
{
    Console.Write("User > ");
    chatMessages.AddUserMessage(Console.ReadLine()!);

    OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
    var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
        chatMessages,
        executionSettings: settings,
        kernel: kernel);

    Console.Write("Assistant > ");
    // Stream the results
    string fullMessage = "";
    await foreach (var content in result)
    {
        Console.Write(content.Content);
        fullMessage += content.Content;
    }
    Console.WriteLine("\n--------------------------------------------------------------");

    // Add the message from the agent to the chat history
    chatMessages.AddAssistantMessage(fullMessage);
}

public class TripPlanner // <------------ Trip planner plugin. An expert on planning trips
{
    [KernelFunction]
    [Description("Returns back the required steps necessary to plan a one day travel to a destination by an electric car.")]
    [return: Description("The list of steps needed to plan a one day travel by an electric car")]
    public async Task<string> GenerateRequiredStepsAsync(
        Kernel kernel,
        [Description("A 2-3 sentence description of where is a good place to go to today")] string destination,
        [Description("The time of the day to start the trip")] string timeOfDay)
    {
        // Prompt the LLM to generate a list of steps to complete the task
        var result = await kernel.InvokePromptAsync($"""
        I'm going to plan a short one day vacation to {destination}. I would like to start around {timeOfDay}.
        Before I do that, can you succinctly recommend the top 2 steps I should take in a numbered list?
        I want to make sure I don't forget to pack anything for the weather at my destination and my car is sufficiently charged before I start the journey.
        """, new() {
            { "destination", destination },
            { "timeOfDay", timeOfDay }
        });

        // Return the plan back to the agent
        return result.ToString();
    }
}

public class TimeTeller // <------------ Time teller plugin. An expert on time, peak and off-peak periods
{
    [KernelFunction]
    [Description("This function retrieves the current time.")]
    [return: Description("The current time.")]
    public string GetCurrentTime() => DateTime.Now.ToString("F");

    [KernelFunction]
    [Description("This function checks if the current time is off-peak.")]
    [return: Description("True if the current time is off-peak; otherwise, false.")]
    public bool IsOffPeak() => DateTime.Now.Hour < 7 || DateTime.Now.Hour >= 21;
}

public class WeatherForecaster // <------------ Weather plugin. An expert on weather. Can tell the weather at a given destination
{
    [KernelFunction]
    [Description("This function retrieves weather at given destination.")]
    [return: Description("Weather at given destination.")]
    public string GetTodaysWeather([Description("The destination to retrieve the weather for.")] string destination)
    {
        // <--------- This is where you would call a fancy weather API to get the weather for the given <<destination>>.
        // We are just simulating a random weather here.
        string[] weatherPatterns = { "Sunny", "Cloudy", "Windy", "Rainy", "Snowy" };
        Random rand = new Random();
        return weatherPatterns[rand.Next(weatherPatterns.Length)];
    }
}

public class ElectricCar // <------------ Car plugin. Knows about states and conditions of the electric car. Also can charge the car.
{
    private bool isCarCharging = false;
    private int batteryLevel = 0;
    private CancellationTokenSource source;

    // Mimic charging the electric car, using a periodic timer.
    private async Task AddJuice()
    {
        source = new CancellationTokenSource();
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(source.Token))
        {
            batteryLevel++;
            if (batteryLevel == 100)
            {
                isCarCharging = false;
                Console.WriteLine("\rBattery is full.");
                source.Cancel();
                return;
            }
            //Console.WriteLine($"Charging {batteryLevel}%");
            Console.Write("\rCharging {0}%", batteryLevel);
        }
    }

    [KernelFunction]
    [Description("This function checks if the electric car is currently charging.")]
    [return: Description("True if the car is charging; otherwise, false.")]
    public bool IsCarCharging() => isCarCharging;

    [KernelFunction]
    [Description("This function returns the current battery level of the electric car.")]
    [return: Description("The current battery level.")]
    public int GetBatteryLevel() => batteryLevel;


    [KernelFunction]
    [Description("This function starts charging the electric car.")]
    [return: Description("A message indicating the status of the charging process.")]
    public string StartCharging()
    {
        if (isCarCharging)
        {
            return "Car is already charging.";
        }
        else if (batteryLevel == 100)
        {
            return "Battery is already full.";
        }

        Task.Run(AddJuice);

        isCarCharging = true;
        return "Charging started.";
    }

    [KernelFunction]
    [Description("This function stops charging the electric car.")]
    [return: Description("A message indicating the status of the charging process.")]
    public string StopCharging()
    {
        if (!isCarCharging)
        {
            return "Car is not charging.";
        }
        isCarCharging = false;
        source?.Cancel();
        return "Charging stopped.";
    }
}