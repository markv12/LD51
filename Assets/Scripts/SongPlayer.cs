using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SongPlayer : MonoBehaviour {

    public AudioSourcePool audioSourcePool;

    private bool loop = false;

    private Song currentSong;
    private double dspStartOffset;
    private float startOffset;
    public void PlaySong(Song song, double startWait,  bool _loop) {
        StopSong();
        loop = _loop;
        currentSong = song;
        dspStartOffset = DspTimeEstimator.Instance.DspTime + startWait;
        startOffset = Time.time + (float)startWait;
        QueueSongAtOffset(song, dspStartOffset, startOffset);
    }

    public void PlayPart(InstrumentTrack part, double startWait, bool _loop) {
        Song song = new Song() {
            length = 10,
            parts = new InstrumentTrack[] { part },
        };
        PlaySong(song, startWait, _loop);
    }

    public void StopSong() {
        loop = false;
        noteQueue.Clear();
        for (int i = 0; i < activeSources.Count; i++) {
            activeSources[i].Stop();
        }
    }

    private readonly List<QueuedNote> noteQueue = new List<QueuedNote>();
    private void Update() {
        if(loop && (Time.time - startOffset) > (currentSong.length - 0.5f)) {
            dspStartOffset += currentSong.length;
            startOffset += currentSong.length;
            QueueSongAtOffset(currentSong, dspStartOffset, startOffset);
        }
        for (int i = 0; i < noteQueue.Count; i++) {
            QueuedNote note = noteQueue[i];
            if((note.startTime - Time.time) < 0.5f) {
                PlayNote(note);
                noteQueue.RemoveAt(i);
                i--;
            }
        }
    }


    private readonly List<AudioSource> activeSources = new List<AudioSource>();
    public const float FADE_TIME = 0.2f;
    private void PlayNote(QueuedNote note) {
        AudioSource audioSource = audioSourcePool.GetAudioSource(note.instrumentNote);
        activeSources.Add(audioSource);
        audioSource.PlayScheduled(note.dspStartTime);
        StartCoroutine(EndRoutine());

        IEnumerator EndRoutine() {
            if(note.endTime > 0) {
                yield return new WaitForSeconds(note.endTime - Time.time);
                float startVolume = audioSource.volume;
                this.CreateAnimationRoutine(FADE_TIME, (float progress) => {
                    audioSource.volume = Mathf.Lerp(startVolume, 0, progress);
                }, () => {
                    DisposeAudioSource(audioSource);
                });
            } else {
                float clipLength = audioSource.clip == null ? FADE_TIME : audioSource.clip.length;
                float waitTime = (float)(note.dspStartTime - DspTimeEstimator.Instance.DspTime) + clipLength + FADE_TIME;
                yield return new WaitForSeconds(waitTime);
                DisposeAudioSource(audioSource);
            }
        }
    }

    private void DisposeAudioSource(AudioSource audioSource) {
        activeSources.Remove(audioSource);
        audioSourcePool.DisposeAudioSource(audioSource);
    }

    public const double SECONDS_PER_BEAT = 0.625;

    private void QueueSongAtOffset(Song song, double dspStartOffset, float startOffset) {
        BandMemberMasterList iml = BandMemberMasterList.Instance;
        for (int i = 0; i < song.parts.Length; i++) {
            InstrumentTrack mainTrack = song.parts[i];
            BandMember bandMember = iml.GetBandMemberForId(mainTrack.instrument);
            for (int j = 0; j < mainTrack.notes.Count; j++) {
                List<Note> noteList = mainTrack.notes[j];
                for (int k = 0; k < noteList.Count; k++) {
                    Note note = noteList[k];
                    double startTime = note.start * SECONDS_PER_BEAT;
                    double endTime = note.end * SECONDS_PER_BEAT;
                    InstrumentNote instrumentNote = bandMember.GetInstrumentNote(j);
                    QueueNote(dspStartOffset, startOffset, startTime, endTime, instrumentNote);
                }
            }
        }
    }

    private void QueueNote(double dspStartOffset, float startOffset, double startTime, double _endTime, InstrumentNote _instrumentNote) {
        double endTime = (_endTime > 0) ? (startOffset + _endTime + FADE_TIME) : 0;
        noteQueue.Add(new QueuedNote() {
            dspStartTime = dspStartOffset + startTime,
            startTime = startOffset + startTime,
            endTime = (float)endTime,
            instrumentNote = _instrumentNote
        });
    }

    private struct QueuedNote {
        public double dspStartTime;
        public double startTime;
        public float endTime;
        public InstrumentNote instrumentNote;
    }
}
