# Qwen/Ollama Debugging Guide

## Problem Summary
Qwen is taking a long time to respond and returning empty messages when queried through the application's text window.

## Changes Made
Added comprehensive logging to `QwenLLMService.cs` to help diagnose the issue:
- Request details (API URL, model name, prompt length)
- Response status codes
- Raw JSON responses from Ollama
- Specific error messages for timeouts, HTTP errors, and empty responses

## Diagnostic Steps

### 1. Verify Ollama is Running
```bash
# Check if Ollama is running
curl http://localhost:11434/api/tags

# Expected output: JSON listing available models
```

### 2. Verify Qwen Model is Downloaded
```bash
# List all downloaded models
ollama list

# If qwen3:4b is not listed, download it:
ollama pull qwen3:4b
```

### 3. Test Ollama API Directly
```bash
# Test with a simple prompt
curl http://localhost:11434/api/generate -d '{
  "model": "qwen3:4b",
  "prompt": "Hello, how are you?",
  "stream": false
}'
```

**Expected response:**
```json
{
  "model": "qwen3:4b",
  "created_at": "...",
  "response": "I'm doing well, thank you for asking!",
  "done": true,
  ...
}
```

**If you get an empty `response` field**, the problem is with Ollama/Qwen itself, not the application.

### 4. Check Application Logs
After running the application with the new logging:
- Look for log entries starting with `QwenLLMService`
- Check for "Response JSON:" entries to see the raw response
- Look for timeout errors or HTTP errors

## Common Issues and Solutions

### Issue 1: Model Not Downloaded
**Symptom:** Error message about model not found

**Solution:**
```bash
ollama pull qwen3:4b
```

### Issue 2: Ollama Not Running
**Symptom:** "Error communicating with Qwen: ... Please check that Ollama is running"

**Solution:**
```bash
# Start Ollama manually
ollama serve
```

Or enable auto-start in `appsettings.json`:
```json
"OllamaSettings": {
  "AutoStart": true,
  "AutoStop": true
}
```

### Issue 3: Prompt Too Long / Timeout
**Symptom:** "Request timed out after 120 seconds"

**Solution:** Increase timeout in `appsettings.json`:
```json
"QwenSettings": {
  "TimeoutSeconds": 300
}
```

### Issue 4: Model Generating Empty Responses
**Symptom:** Ollama responds but `response` field is empty

**Possible causes:**
1. **Model loading issue** - Try pulling the model again
2. **Insufficient resources** - Qwen3:4b needs ~4GB RAM
3. **Corrupted model** - Remove and re-download:
   ```bash
   ollama rm qwen3:4b
   ollama pull qwen3:4b
   ```

### Issue 5: Alternative Model
**Symptom:** qwen3:4b doesn't work well

**Solution:** Try a different model in `appsettings.json`:
```json
"QwenSettings": {
  "ModelName": "qwen2.5:latest"
}
```

Or use a smaller/larger model:
- `qwen2.5:7b` - Larger, more capable
- `qwen2.5:1.5b` - Smaller, faster
- `mistral:latest` - Alternative LLM
- `llama3:latest` - Alternative LLM

## Alternative to Ollama

If Ollama continues to have issues, you could:

1. **Use a cloud API** (OpenAI, Anthropic, etc.)
   - Would require implementing a new ILLMService
   - More reliable but costs money

2. **Use a different local inference engine**
   - LM Studio
   - llama.cpp
   - Would require code changes to interface with different API

## Next Steps

1. Run the diagnostic commands above
2. Run the application and check the logs
3. Share the log output to identify the specific issue
4. Based on the logs, apply the appropriate solution

## Getting Logs

The application uses .NET logging. To see debug-level logs:

### Windows (Visual Studio)
- Check the Debug Output window
- Or add to `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "DnDAI.Services.QwenLLMService": "Debug"
  }
}
```

### Linux/Mac (Console)
```bash
export DOTNET_LOGGING__CONSOLE__LOGLEVEL__DEFAULT=Debug
dotnet run
```
