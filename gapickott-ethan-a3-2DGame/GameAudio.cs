using System;
using NAudio.Wave;

namespace Game10003
{
    public class GameAudio
    {
        private IWavePlayer wavePlayer;
        private AudioFileReader audioFileReader;
        private bool isSongPlaying = false;  // Tracks the song playing

        public GameAudio()
        {
            wavePlayer = new WaveOutEvent();  // Initialize the audio player
        }

        // Method to play the song once (no looping)
        public void PlaySong()
        {
            if (isSongPlaying) return;  // Don't play the song if it's already playing (for redundancy)

            string songPath = "Song/Song.mp3"; // Path to the song file (very creative name)

            if (System.IO.File.Exists(songPath))
            {
                // Reads the MP3 file
                audioFileReader = new AudioFileReader(songPath);
                wavePlayer.Init(audioFileReader);

                // Start playing song audio
                wavePlayer.Play();
                isSongPlaying = true;

                // Method to stop song
                wavePlayer.PlaybackStopped += (sender, e) => {
                    audioFileReader.Dispose();
                    wavePlayer.Dispose();
                    isSongPlaying = false;
                };
            }
        }

        public void StopSong()
        {
            if (wavePlayer != null && isSongPlaying)
            {
                wavePlayer.Stop();  // Stops the song
                isSongPlaying = false;
            }
        }
    }
}
