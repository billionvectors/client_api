# asimplevectors Client Library (C#)

[asimplevectors](https://docs.asimplevectors.com/) is a high-performance vector database optimized for retrieval-augmented generation (RAG) vector database.
**asimplevectors Client** is a C# library designed to interact with the asimplevectors service. It provides a fully asynchronous API for managing vector spaces, performing searches, handling snapshots, and applying RBAC-based security.

## Features

- **Space Management**: Create, update, delete, and list spaces with configurable settings.
- **Versioning**: Manage versions within spaces, including creating, updating, and retrieving specific versions.
- **Vector Operations**: Upsert, retrieve, and search vectors with support for metadata.
- **RBAC Security**: Manage tokens for role-based access control (RBAC).
- **Snapshot Management**: Create and manage snapshots of spaces.
- **Async Support**: Fully asynchronous API for modern, high-performance applications.
- **Rerank Capability**: Provides reranking of initial search results using advanced scoring techniques like *BM25*. This feature ensures highly relevant results for document retrieval use cases.

## Installation

Add the `asimplevectors.Client` package to your project using NuGet:

```bash
dotnet add package asimplevectors.Client
```

## Requirements

- [asimplevectors](https://github.com/billionvectors/asimplevectors)
- .NET Core 6.0+
- Dependencies managed through NuGet

## Usage

### Initialization

```csharp
using asimplevectors.Services;

// Initialize the client
var client = new asimplevectorsClient("http://localhost:21001");

// Example API call
var spaces = await client.ListSpacesAsync();
Console.WriteLine(spaces);
```

### Example: Space Management

```csharp
using asimplevectors.Models;
using asimplevectors.Services;

var client = new asimplevectorsClient("http://localhost:21001");

// Create a space
var createSpaceData = new SpaceRequest
{
    Name = "spacename",
    Dimension = 128,
    Metric = "L2"
};
await client.CreateSpaceAsync(createSpaceData);
Console.WriteLine("Space created successfully.");

// List spaces
var spaces = await client.ListSpacesAsync();
Console.WriteLine("Available spaces: " + spaces);
```

### Example: Vector Operations

```csharp
using asimplevectors.Models;
using asimplevectors.Services;

var client = new asimplevectorsClient("http://localhost:21001");

// Upsert vectors
var vectorData = new VectorRequest
{
    Vectors = new[]
    {
        new VectorData
        {
            Id = 1,
            Data = new float[] { 0.1f, 0.2f, 0.3f, 0.4f },
            Metadata = new Dictionary<string, object> { { "label", "first" } }
        }
    }
};
await client.CreateVectorAsync("spacename", vectorData);
Console.WriteLine("Vector upserted successfully.");

// Retrieve vectors by version
var vectors = await client.GetVectorsByVersionAsync("spacename", 0);
Console.WriteLine("Retrieved vectors: " + vectors);
```

### Example: RBAC Token Management

```csharp
using asimplevectors.Models;
using asimplevectors.Services;

var client = new asimplevectorsClient("http://localhost:21001");

// Create an RBAC token
var tokenData = new RbacTokenRequest
{
    UserId = 1,
    Space = 2,
    Vector = 2
};
var token = await client.CreateRbacTokenAsync(tokenData);
Console.WriteLine("Token created successfully: " + token);

// List RBAC tokens
var tokens = await client.ListRbacTokensAsync();
Console.WriteLine("Available tokens: " + tokens);
```

## Development

### Setting up the development environment

1. **Setup the asimplevectors server using Docker**:

   ```bash
   docker pull billionvectors/asimplevectors:latest
   docker run -p 21001:21001 -p 21002:21002 billionvectors/asimplevectors:latest
   ```

2. **Clone the repository**:

   ```bash
   git clone https://github.com/billionvectors/client_api.git
   cd client_api/csharp
   ```

3. **Run the examples**:

   ```bash
   dotnet run --project example/example_space.csproj
   ```

---