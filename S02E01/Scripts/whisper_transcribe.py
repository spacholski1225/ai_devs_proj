# whisper_transcribe.py
import sys
import whisper

def transcribe_audio(file_path):
    model = whisper.load_model("base")
    result = model.transcribe(file_path)
    return result["text"]

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python whisper_transcribe.py <audio_file_path>")
        sys.exit(1)
    
    file_path = sys.argv[1]
    text = transcribe_audio(file_path)
    print("Transcription result:", text)
