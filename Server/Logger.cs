using System;

namespace Server
{
    internal class Logger
    {
        public string Context { get; }

        public Logger() : this("Server") { }

        public Logger(string context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Log(string message) => Log(message, Context, ConsoleColor.Blue);

        public void Log(string message, string context) => Log(message, context, ConsoleColor.Blue);

        public void Error(string message) => Log(message, Context, ConsoleColor.Red);

        public void Error(string message, string context) => Log(message, context, ConsoleColor.Red);

        public void Warning(string message) => Log(message, Context, ConsoleColor.Yellow);

        public void Warning(string message, string context) => Log(message, context, ConsoleColor.Yellow);

        public void Log(string message, string context, ConsoleColor color)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            if (string.IsNullOrWhiteSpace(context))
                throw new ArgumentException("Context cannot be null or empty.", nameof(context));

            try
            {
                Console.ForegroundColor = color;
            }
            catch (ArgumentException)
            {
                Console.ForegroundColor = ConsoleColor.Gray; // Fallback to default if an invalid color is provided.
            }

            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{context}] {message}");
            Console.ResetColor();
        }
    }
}
