#!/bin/bash

# Exit on error
set -e

# Function to print usage
usage() {
    echo "Usage: $0 [test_name]"
    echo "If no test_name is provided, the default 'example_usage.py' will be executed."
    exit 1
}

# Check if venv directory exists, create if not
if [ ! -d "venv" ]; then
    echo "Creating virtual environment..."
    python3 -m venv venv
fi

# Activate the virtual environment
source venv/bin/activate

# Install dependencies from requirements.txt
echo "Installing dependencies..."
pip install -r requirements.txt > /dev/null 2>&1
pip install -r example/requirements.txt > /dev/null 2>&1

# Determine the test script to run
TEST_NAME=${1:-"usage"}
TEST_SCRIPT="example/example_${TEST_NAME}.py"

# Check if the specified test script exists
if [ ! -f "$TEST_SCRIPT" ]; then
    echo "Error: Test script '$TEST_SCRIPT' does not exist."
    deactivate
    exit 1
fi

# Run the example Python script with PYTHONPATH set to the project root
echo "Running $TEST_SCRIPT..."
PYTHONPATH=. python "$TEST_SCRIPT"

# Deactivate the virtual environment after execution
deactivate
echo "Script finished successfully."
