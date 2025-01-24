namespace Client.Net.IO
{
    internal enum OpCode : byte
    {
        // Connection-related opcodes
        Connect = 1,         // Called when the client connects successfully
        Disconnect = 2,      // Called when the client disconnects
        Ping = 3,            // Keeps the connection alive

        // Messaging-related opcodes
        SendMessage = 10,    // Called when the client sends a message
        ReceiveMessage = 11, // Called when the client receives a message

        // User-related opcodes
        UserJoined = 20,     // Notifies when a user joins the session
        UserLeft = 21,       // Notifies when a user leaves the session
        UserTyping = 22,     // Indicates a user is typing

        // Error-handling opcodes
        Error = 30,          // General error reporting
        Unauthorized = 31,   // Called when a user performs an unauthorized action
        InvalidRequest = 32  // Called when the client sends a malformed request
    }
}
