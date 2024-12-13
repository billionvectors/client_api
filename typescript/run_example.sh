#!/bin/bash


cd example
npm install
npm install -g typescript

# Determine the test script to run
TEST_NAME=${1:-"usage"}
TEST_TS_SCRIPT="example_${TEST_NAME}.ts"
TEST_JS_SCRIPT="example_${TEST_NAME}.js"

# Check if the specified test script exists
if [ ! -f "$TEST_TS_SCRIPT" ]; then
    echo "Error: Test script '$TEST_TS_SCRIPT' does not exist."
    exit 1
fi

if [ ! -f "$TEST_JS_SCRIPT" ]; then
    npx tsc $TEST_TS_SCRIPT
fi

node $TEST_JS_SCRIPT

echo "Script finished successfully."
