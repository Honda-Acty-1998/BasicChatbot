using LlmTornado;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.Code;

//  UX IMPROVEMENT: Welcome Message 
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("==================================================");
Console.WriteLine("       WELCOME TO THE AI STUDY COACH            ");
Console.WriteLine("   Modes: Tutor, Quiz, Hint | Type /help        ");
Console.WriteLine("==================================================");
Console.ResetColor();

string currentMode = "tutor";

// Initialize API
TornadoApi api = new TornadoApi(
    new Uri("http://127.0.0.1:1234"),
    string.Empty,
    LLmProviders.OpenAi);

// Initialize Conversation
Conversation chat = CreateConversation(api, currentMode);

while (true)
{
    Console.WriteLine();
    //  UX IMPROVEMENT: Mode Display 
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($"[Current mode: {currentMode.ToUpper()}]");
    Console.ResetColor();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("You: ");
    Console.ResetColor();

    string? userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput))
        continue;

    // Commands 
    if (userInput.Equals("/exit", StringComparison.OrdinalIgnoreCase))
        break;

    if (userInput.Equals("/help", StringComparison.OrdinalIgnoreCase))
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n--- Command Menu ---");
        Console.WriteLine("/mode tutor - Detailed explanations");
        Console.WriteLine("/mode quiz  - Tests your knowledge");
        Console.WriteLine("/mode hint  - Small clues only");
        Console.WriteLine("/clear      - Restart the conversation history");
        Console.WriteLine("/exit       - Close the program");
        Console.ResetColor();
        continue;
    }

    // UX IMPROVEMENT: Clear Conversation Command
    if (userInput.Equals("/clear", StringComparison.OrdinalIgnoreCase))
    {
        chat = CreateConversation(api, currentMode);
        Console.WriteLine("System: Conversation history cleared.");
        continue;
    }

    if (userInput.StartsWith("/mode ", StringComparison.OrdinalIgnoreCase))
    {
        string requestedMode = userInput.Substring(6).Trim().ToLower();

        if (requestedMode == "tutor" || requestedMode == "quiz" || requestedMode == "hint")
        {
            currentMode = requestedMode;
            chat = CreateConversation(api, currentMode);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✔ Switched to {currentMode} mode.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Unknown mode. Try: tutor, quiz, or hint.");
            Console.ResetColor();
        }
        continue;
    }

    //  Bot Response 
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("Bot: ");

    // Using simple streaming without the broken bold logic to ensure stability
    await chat.AppendUserInput(userInput).StreamResponse(token =>
    {
        Console.Write(token);
    });

    Console.WriteLine();
    Console.ResetColor();
}

static Conversation CreateConversation(TornadoApi api, string mode)
{
    //Ensure the model name matches what you have loaded in LM Studio
    Conversation chat = api.Chat.CreateConversation(new ChatModel("google/gemma-3-4b"));

    string systemPrompt = mode switch
    {
        "quiz" => "You are a programming study coach. Do NOT give answers. Instead, ask the user a short question that guides them to the answer. Keep it interactive.",
        "hint" => "You are a programming hint bot. Provide only a tiny clue or the very next step. Do not provide the full solution.",
        _ => "You are a helpful programming tutor. Explain concepts using simple metaphors and provide a very short code example."
    };

    chat.AppendSystemMessage(systemPrompt);
    return chat;
}