using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "<Your_Deployment_Name>",
    endpoint: "<Azure-Deployment-Endpoint-Ends-In:openai.azure.com>",
    apiKey: "<Your_API_Key>"
);
var kernel = builder.Build();

Console.WriteLine(await kernel.InvokePromptAsync("What is Gen AI?"));