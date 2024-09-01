using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.ComponentModel;

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "<YOUR_DEPLOYMENT_NAME>",
    endpoint: "<YOUR_ENDPOINT>",
    apiKey: "<YOUR_AZURE_OPENAI_API_KEY>"
);

builder.Plugins.AddFromType<TimeTeller>();
builder.Plugins.AddFromType<ElectricCar>();
var kernel = builder.Build();

OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
while (true)
{
    Console.Write("User > ");
    string userMessage = Console.ReadLine();
    Console.WriteLine("Agent > " + await kernel.InvokePromptAsync(userMessage, new(settings)));
    Console.WriteLine("--------------------------------------------------------------");
}

public class TimeTeller
{
    [Description("This function retrieves the current time.")]
    [KernelFunction]
    public string GetCurrentTime() => DateTime.Now.ToString("F");

    [Description("This function checks if in off-peak period between 9pm and 7am")]
    [KernelFunction]
    public bool IsOffPeak() => DateTime.Now.Hour < 7 || DateTime.Now.Hour >= 21;
}

public class ElectricCar
{
    private bool isCarCharging = false;

    [Description("This function checks if the electric car is charging.")]
    [KernelFunction]
    public bool IsCarCharging() => isCarCharging;

    [Description("This function starts charging the electric car.")]
    [KernelFunction]
    public string StartCharging()
    {
        if (isCarCharging)
        {
            return "Car is already charging.";
        }
        isCarCharging = true;

        return "Charging started.";
    }

    [Description("This function stops charging the electric car.")]
    [KernelFunction]
    public string StopCharging()
    {
        if (!isCarCharging)
        {
            return "Car is not charging.";
        }
        isCarCharging = false;
        return "Charging stopped.";
    }
}