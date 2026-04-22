extern "C" {
    void StartBackgroundUpload(const char* url, const char* filePath) {}
    float GetUploadProgress() { return 0.0f; }
    void CancelUpload() {}
    void SetupCompletionCallback(void* callback) {}
}

// android linker stubs for iOS native symbols referenced by MultiSet SDK 
// by TimofeiS 