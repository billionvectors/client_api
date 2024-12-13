### TypeScript Version of README.md

# asimplevectors Client Library (TypeScript)

asimplevectors Client is a TypeScript library providing an asynchronous API client to interact with the asimplevectors service, supporting vector management, search, space configuration, and RBAC-based security.

## Features

- **Space Management**: Create, update, delete, and list spaces with flexible configurations.
- **Versioning**: Manage versions for spaces, including creating and retrieving specific versions.
- **Vector Operations**: Upsert, retrieve, and search vectors with support for arrays and metadata.
- **RBAC Security**: Manage tokens for role-based access control (RBAC) and apply them to secure API calls.
- **Snapshot Management**: Create and manage snapshots of vector spaces.
- **Async Support**: Fully asynchronous API for high-performance applications.

## Installation

Install the library using npm or yarn:

```bash
npm install asimplevectors
```

Or with yarn:

```bash
yarn add asimplevectors
```

## Requirements

- [asimplevectors](https://github.com/billionvectors/asimplevectors)
- Node.js 16+
- Dependencies managed via npm or yarn

## Usage

### Initialization

```typescript
import { ASimpleVectorsClient } from "asimplevectors";

// Initialize the client
const client = new ASimpleVectorsClient("localhost", 21001);

// Use the client in async functions
async function initializeClient() {
  // Example API call
  const spaces = await client.listSpaces();
  console.log(spaces);
}
```

### Example: Space Management

```typescript
import { ASimpleVectorsClient } from "asimplevectors";

async function main(): Promise<void> {
  const client = new ASimpleVectorsClient("localhost", 21001);

  // Create a space
  const createSpaceData = {
    name: "spacename",
    dimension: 128,
    metric: "L2",
  };
  await client.createSpace(createSpaceData);
  console.log("Space created successfully.");

  // List spaces
  const spaces = await client.listSpaces();
  console.log("Available spaces:", spaces);

  await client["axiosInstance"].post("/cluster/close");
}

main().catch(console.error);
```

### Example: Vector Operations

```typescript
import { ASimpleVectorsClient } from "asimplevectors";

async function vectorOperations(): Promise<void> {
  const client = new ASimpleVectorsClient("localhost", 21001);

  // Upsert vectors
  const vectorData = {
    vectors: [
      { id: 1, data: [0.1, 0.2, 0.3, 0.4], metadata: { label: "first" } },
    ],
  };
  await client.createVector("spacename", vectorData);
  console.log("Vector upserted successfully.");

  // Retrieve vectors by version
  const vectors = await client.getVectorsByVersion("spacename", 0);
  console.log("Retrieved vectors:", vectors);

  await client["axiosInstance"].post("/cluster/close");
}

vectorOperations().catch(console.error);
```

### Example: RBAC Token Management

```typescript
import { ASimpleVectorsClient } from "asimplevectors";

async function manageTokens(): Promise<void> {
  const client = new ASimpleVectorsClient("localhost", 21001);

  // Create an RBAC token
  const tokenData = {
    user_id: 1,
    space: 2,
    vector: 2,
  };
  await client.createRbacToken(tokenData);
  console.log("Token created successfully.");

  // List RBAC tokens
  const tokens = await client.listRbacTokens();
  console.log("Available tokens:", tokens);

  await client["axiosInstance"].post("/cluster/close");
}

manageTokens().catch(console.error);
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
   cd client_api/typescript
   ```

3. **Run the examples**:

   ```bash
   npx tsc example/example_search.ts --outDir example
   node example/example_search.js
   ```

---

## Contributing

We welcome contributions to improve this library. Submit issues or pull requests on the [GitHub repository](https://github.com/billionvectors/client_api).

---

### Differences from Python Version

- The TypeScript client is designed for JavaScript/TypeScript environments, ensuring compatibility with Node.js.
- Uses JavaScript arrays instead of numpy for vector data.
- Provides type safety and improved developer experience with TypeScript.

Let me know if additional details are needed! 🚀