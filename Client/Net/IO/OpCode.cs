namespace Client.Net.IO
{
    internal enum OpCode : byte
    {
        // Connection-related opcodes
        Connect = 1,          // Called when the client connects successfully
        Disconnect = 2,       // Called when the client disconnects

        // Messaging-related opcodes
        SendMessage = 10,     // Called when the client sends a message
        ReceiveMessage = 11,  // Called when the client receives a message

        // User-related opcodes
        UserOnline = 20,      // Notifies when a user joins the session

        // Error-handling opcodes
        Error = 30,           // General error reporting
        Unauthorized = 31,    // Called when a user performs an unauthorized action
        InvalidRequest = 32,  // Called when the client sends a malformed request

        // Category-related opcodes
        GetAllCategories = 40,     // Client requests all categories
        SendCategories = 41,       // Server sends all categories to the client
        AddCategory = 42,          // Client requests to add a category
        UpdateCategory = 43,       // Client requests to update a category
        DeleteCategory = 44,       // Client requests to delete a category

        // Product-related opcodes
        GetProductsByCategory = 50, // Client requests products by category
        SendProducts = 51,          // Server sends products to the client
        AddProduct = 52,            // Client requests to add a product
        UpdateProduct = 53,         // Client requests to update a product
        DeleteProduct = 54,         // Client requests to delete a product

        // Product-changes-notify opcodes
        ProductAdded = 60,          // Server notifies clients that a product was added
        ProductUpdated = 61,        // Server notifies clients that a product was updated
        ProductDeleted = 62         // Server notifies clients that a product was deleted
    }
}
