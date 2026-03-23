using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.Code;


string currentMode = "tutor";

TornadoApi api = new TornadoApi(
    new Uri("http://127.0.0.1:1234"),
    string.Empty,
    LLmProviders.OpenAi);

Conversation chat = CreateConversation(api, currentMode);

while (true)
{
    Console.WriteLine();
    Console.WriteLine($"Current mode: {currentMode}");
    Console.Write("You: ");
    string? userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    if (userInput.Equals("/exit", StringComparison.OrdinalIgnoreCase))
        break;

    if (userInput.Equals("/help", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Commands:");
        Console.WriteLine("/mode tutor");
        Console.WriteLine("/mode quiz");
        Console.WriteLine("/mode hint");
        Console.WriteLine("/help");
        Console.WriteLine("/exit");
        continue;
    }

    if (userInput.StartsWith("/mode ", StringComparison.OrdinalIgnoreCase))
    {
        string requestedMode = userInput.Substring(6).Trim().ToLower();

        if (requestedMode == "tutor" || requestedMode == "quiz" || requestedMode == "hint")
        {
            currentMode = requestedMode;
            chat = CreateConversation(api, currentMode);
            Console.WriteLine($"Switched to {currentMode} mode.");
        }
        else
        {
            Console.WriteLine("Unknown mode.");
        }

        continue;
    }

    Console.Write("Bot: ");
    await chat.AppendUserInput(userInput).StreamResponse(WriteOutput);
    Console.WriteLine();
}


static void WriteOutput(string output)
{
    if (output.Contains("**"))
    {
        if (Console.ForegroundColor == ConsoleColor.Gray)
            Console.ForegroundColor = ConsoleColor.Green;
        else
            Console.ForegroundColor = ConsoleColor.Gray;

    }
    else
        Console.Write(output);
}

static Conversation CreateConversation(TornadoApi api, string mode)
{
    Conversation chat = api.Chat.CreateConversation(new ChatModel("google/gemma-3-4b"));

    string systemPrompt = mode switch
    {
        "quiz" => "You are a programming study coach. Ask a guiding question before giving answers. Encourage thinking.",
        "hint" => "You are a programming hint bot. Give only a small hint or next step. Keep responses short.",
        _ => "You are a helpful programming tutor for beginner students. Explain clearly and use short examples when useful."
    };

    chat.AppendSystemMessage(systemPrompt);
    return chat;
}