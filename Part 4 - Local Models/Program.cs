using Microsoft.SemanticKernel;
# pragma warning disable SKEXP0010

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: "phi3",
    apiKey: null,
    endpoint: new Uri("http://localhost:11434")
);

var kernel = builder.Build();

Console.Write("User: ");
var input = Console.ReadLine();
var response = await kernel.InvokePromptAsync(input);
Console.WriteLine(response.GetValue<string>());
Console.WriteLine("------------------------------------------------------------------------");
Console.ReadLine();