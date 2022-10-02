using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordUI : MonoBehaviour {
    public RectTransform rectT;
    public Vector2 offScreenPos;
    public Vector2 onScreenPos;

    public Button doneButton;
    public Button clearButton;
    public Button backButton;
    public SongVisualizer songVisualizer;

    public SongRecorder songRecorder;
    public SongPlayer otherPartPlayer;
    public SongPlayer[] yourPartPlayers;
    public SongPlayer metronomePlayer;

    public Image yourMemberImage;
    public Image otherMemberImage;
    public TMP_Text bandNameText;
    public TMP_Text otherMemberNameText;

    public BandPickUI bandPickUI;

    private const double STANDARD_WAIT = 0.5;

    private void Awake() {
        doneButton.onClick.AddListener(Done);
        clearButton.onClick.AddListener(ClearYourPart);
        backButton.onClick.AddListener(Back);
    }

    private void Done() {
        AudioManager.Instance.PlayPlasticClickSound(1);
        StopAllMusic();
        bandPickUI.gameObject.SetActive(true);
        MoveUI(offScreenPos);
    }

    private void ClearYourPart() {
        AudioManager.Instance.PlayPlasticClickSound(1);
        StopYourPart();
        songRecorder.Clear();
        songVisualizer.ClearNoteSquares();
    }

    private void StopYourPart() {
        for (int i = 0; i < yourPartPlayers.Length; i++) {
            yourPartPlayers[i].StopSong();
        }
    }

    private void Back() {
        LoadingScreen.ShowTransition(BackRoutine());

        songRecorder.StopRecording();
        metronomePlayer.StopSong();
        otherPartPlayer.StopSong();
        ClearYourPart();
        songVisualizer.ClearLinesAndStop();

        IEnumerator BackRoutine() {
            yield return null;
            rectT.anchoredPosition = offScreenPos;
            AudioManager.Instance.StartCrowdMurmur(1f);
            gameObject.SetActive(false);
        }
    }

    private void StopAllMusic() {
        songRecorder.StopRecording();
        metronomePlayer.StopSong();
        otherPartPlayer.StopSong();
        StopYourPart();
    }

    public const float MOVE_IN_TIME = 1.333f;
    public SessionData currentSessionData;
    public void Startup(SessionData sessionData) {
        rectT.anchoredPosition = offScreenPos;

        yourMemberImage.sprite = sessionData.yourMember.mainSprite;
        otherMemberImage.sprite = sessionData.otherMember.mainSprite;
        bandNameText.text = sessionData.bandName + "'s New Song";
        otherMemberNameText.text = "Playing along with " + sessionData.otherName + "'s track.";
        bandPickUI.ShowSessionData(sessionData);
        bandPickUI.LoadParts();

        gameObject.SetActive(true);

        StartCoroutine(StartupRoutine());

        IEnumerator StartupRoutine() {
            AudioManager.Instance.StopCrowdMurmur();
            yield return MoveUI(onScreenPos);
            yield return new WaitForSeconds(0.8f);
            songVisualizer.ShowInstrument(sessionData.yourMember, STANDARD_WAIT, 10);
            metronomePlayer.PlaySong(Song.METRONOME_SONG, STANDARD_WAIT, true);
            metronomePlayer.onLoop = (double dspStartOffset, float startOffset) => {
                GetYourPartPlayer().PlayPartAtTime(songRecorder.currentTrack, dspStartOffset, startOffset, false);
            };
            otherPartPlayer.PlayPart(sessionData.otherPart, STANDARD_WAIT, true);
            songRecorder.StartRecording(sessionData.yourMember, STANDARD_WAIT, songVisualizer.AddNote);
        }
    }

    private Coroutine MoveUI(Vector2 goalPos) {
        Vector2 startPos = rectT.anchoredPosition;
        return this.CreateAnimationRoutine(MOVE_IN_TIME, (float progress) => {
            float easedProgress = Easing.easeOutSine(0, 1, progress);
            rectT.anchoredPosition = Vector2.Lerp(startPos, goalPos, easedProgress);
        });
    }

    private int yourPartPlayerIndex = 0;
    private SongPlayer GetYourPartPlayer() {
        SongPlayer result = yourPartPlayers[yourPartPlayerIndex];
        yourPartPlayerIndex = (yourPartPlayerIndex + 1) % yourPartPlayers.Length;
        return result;
    }
}
