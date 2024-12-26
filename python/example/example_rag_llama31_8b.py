from transformers import AutoTokenizer, AutoModel, AutoModelForCausalLM
from asimplevectors import ASimpleVectorsClient
import torch
import asyncio

# 1. Prepare the documents and initialize query
documents = [
    "Machine learning is a field of artificial intelligence focused on building systems that learn from data.",
    "Deep learning is a subset of machine learning based on neural networks.",
    "Artificial intelligence involves creating systems that mimic human intelligence.",
    "Neural networks are mathematical models inspired by the structure of the brain.",
    "Transformers are a type of neural network architecture used for language processing."
]

query = "What is deep learning?"

# 2. Load Hugging Face models for embedding and generation
embedding_model_name = "sentence-transformers/all-MiniLM-L6-v2"

# Initialize tokenizer and model for embeddings
retrieval_tokenizer = AutoTokenizer.from_pretrained(embedding_model_name)
retrieval_model = AutoModel.from_pretrained(embedding_model_name) 

generation_model_name = "meta-llama/Llama-3.1-8B"
token = "<your_huggingface_token>"  # HuggingFace Token

# Initialize tokenizer and model for generation (Llama 3.1)
generation_tokenizer = AutoTokenizer.from_pretrained(generation_model_name, token=token)
generation_model = AutoModelForCausalLM.from_pretrained(generation_model_name, token=token)
generation_tokenizer.pad_token = generation_tokenizer.eos_token

# Function to compute embeddings
def compute_embeddings(texts):
    """Generate embeddings for a list of texts."""
    inputs = retrieval_tokenizer(texts, padding=True, truncation=True, return_tensors="pt")
    with torch.no_grad():
        model_output = retrieval_model(**inputs)
    embeddings = model_output.last_hidden_state.mean(dim=1)  # Mean pooling
    return embeddings.detach().cpu().numpy().astype('float32')

# Compute embeddings for the documents
document_embeddings = compute_embeddings(documents)

# 3. Initialize the ASimpleVectors client
client = ASimpleVectorsClient(host="localhost")

async def initialize_space():
    """
    Create a new space in the ASimpleVectors database and upload document embeddings.
    """
    space_name = "rag_example"
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
        }
        for i, embedding in enumerate(document_embeddings)
    ]
    await client.upsert_vector(space_name, {"vectors": vector_data})
    return space_name

async def perform_rag(space_name):
    """
    Perform Retrieval-Augmented Generation (RAG).
    """
    # Step 1: Retrieve relevant documents using vector search
    query_embedding = compute_embeddings([query])[0].tolist()
    search_request = {
        "vector": query_embedding,
        "top_k": 3
    }
    search_results = await client.search(space_name, search_request)
    print(f"SearchResults: {search_results}")

    # Step 2: Extract top retrieved documents
    retrieved_docs = []
    for result in search_results:
        vector_id = result.label
        retrieved_docs.append(documents[vector_id - 1])

    # Step 3: Combine retrieved documents into a prompt for the generator
    context = " ".join(retrieved_docs)
    input_text = f"Query: {query}\nContext: {context}\nAnswer:"

    # Step 4: Generate the response
    inputs = generation_tokenizer(input_text, return_tensors="pt", max_length=1024, padding=True, truncation=True)
    with torch.no_grad():
        summary_ids = generation_model.generate(
            inputs["input_ids"], 
            attention_mask=inputs["attention_mask"], 
            max_length=200, 
            num_beams=5, 
            early_stopping=True)
    response = generation_tokenizer.decode(summary_ids[0], skip_special_tokens=True)

    # Output the results
    print(f"Query: {query}")
    print("\nRetrieved Documents:")
    for doc in retrieved_docs:
        print(f"- {doc}")
    print("\nGenerated Response:")
    print(response)

# Main async function to initialize the space and perform RAG
async def main():
    """
    Main function to initialize the space, upload embeddings, and perform RAG.
    """
    try:
        space_name = await initialize_space()  # Initialize space and upload document embeddings
        await perform_rag(space_name)  # Perform RAG
    finally:
        await client.close()

# Entry point
if __name__ == "__main__":
    asyncio.run(main())
