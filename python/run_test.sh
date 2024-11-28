#!/bin/bash

# Exit on error
set -e

# Check if venv directory exists, create if not
if [ ! -d "venv" ]; then
    echo "Creating virtual environment..."
    python3 -m venv venv
fi

# Activate the virtual environment
source venv/bin/activate

# Install dependencies from requirements.txt
echo "Installing dependencies..."
pip install -r requirements.txt

python -m unittest discover tests

deactivate