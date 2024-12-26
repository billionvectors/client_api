from transformers import AutoTokenizer, AutoModel
from asimplevectors import ASimpleVectorsClient
import torch
import asyncio

# 1. Prepare the documents and query
documents = [
    "This is a document about machine learning.",
    "This document discusses artificial intelligence.",
    "A short text on deep learning.",
    "An article on neural networks and their applications.",
    "Some notes about transformers and attention mechanisms."
]

query = "Tell me about deep learning."

# 2. Load the Hugging Face model and tokenizer
model_name = "sentence-transformers/all-MiniLM-L6-v2"
tokenizer = AutoTokenizer.from_pretrained(model_name)
model = AutoModel.from_pretrained(model_name)

# Function to compute embeddings
def compute_embeddings(texts):
    """Generate embeddings for a list of texts."""
    inputs = tokenizer(texts, padding=True, truncation=True, return_tensors="pt")
    with torch.no_grad():
        model_output = model(**inputs)
    # Use mean pooling for sentence embeddings
    embeddings = model_output.last_hidden_state.mean(dim=1)
    return embeddings.detach().cpu().numpy().astype('float32')

# Compute embeddings for the documents
document_embeddings = compute_embeddings(documents)

# 3. Initialize the ASimpleVectors client
client = ASimpleVectorsClient(host="localhost")

async def initialize_space():
    """
    Create a new space in the ASimpleVectors database and upload document embeddings.
    """
    space_name = "document_ranking"
    await client.create_space({
        "name": space_name,
        "dimension": document_embeddings.shape[1],
        "metric": "Cosine",
        "hnsw_config": {"M": 16, "ef_construct": 100},  # Configure HNSW index
    })

    # Prepare vector data for upsertion
    vector_data = [
        {
            "id": i + 1,
            "data": embedding.tolist(),
            "doc": documents[i],  # Include the raw document as metadata
            "doc_tokens": documents[i].lower().split(),  # Tokenize and store as metadata
        }
        for i, embedding in enumerate(document_embeddings)
    ]
    await client.upsert_vector(space_name, {"vectors": vector_data})
    return space_name

async def rerank(space_name):
    """
    Perform vector search and re-ranking using the `rerank` API of ASimpleVectors.
    """
    # Compute the query embedding
    query_embedding = compute_embeddings([query])[0].tolist()

    # Perform the re-ranking directly using the rerank API
    rerank_request = {
        "vector": query_embedding,
        "tokens": query.lower().split(),  # Tokenized query for BM25 re-ranking
        "top_k": 3  # Number of candidates to retrieve
    }
    print("Perform Rerank")
    rerank_results = await client.rerank(space_name, rerank_request)

    # Process and display results
    print("Query:", query)
    print("Top Re-ranked Documents (BM25):")
    for rank, result in enumerate(rerank_results, start=1):  # Iterate directly over the list
        vector_id = result.vectorUniqueId
        distance = result.distance
        bm25_score = result.bm25Score

        text = documents[vector_id - 1]
        print(f"{rank}. {text} (BM25 Score: {bm25_score:.4f}, Distance: {distance:.4f})")

# Main async function to initialize and execute the pipeline
async def main():
    """
    Main function to initialize the space, upload data, and perform search and re-ranking.
    """
    try:
        space_name = await initialize_space()  # Initialize space and upload document embeddings
        await rerank(space_name)  # Perform search and re-ranking
    finally:
        await client.close()

# Entry point
if __name__ == "__main__":
    asyncio.run(main())
