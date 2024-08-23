using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(
    deploymentName: "open-ai-gtp4o-poc-ag-deployment",
    endpoint: "https://open-ai-gtp4o-poc-ag.openai.azure.com/",
    apiKey: "508bf29262e04363a4d4811b910d3e4d"
);
var kernel = builder.Build();

Console.WriteLine(await kernel.InvokePromptAsync("What is Gen AI?"));