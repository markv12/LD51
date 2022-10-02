using System;
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
    public SongVisualizer songVisualizer;

    public Image yourMemberImage;
    public Image otherMemberImage;
    public TMP_Text bandNameText;
    public TMP_Text otherMemberNameText;

    private void Awake() {
        doneButton.onClick.AddListener(Done);
        clearButton.onClick.AddListener(Clear);
    }

    private void Done() {
        AudioManager.Instance.PlayPlasticClickSound(1);
    }

    private void Clear() {
        AudioManager.Instance.PlayPlasticClickSound(1);
    }

    public void Startup(SessionData sessionData) {
        rectT.anchoredPosition = offScreenPos;

        yourMemberImage.sprite = sessionData.yourMember.mainSprite;
        otherMemberImage.sprite = sessionData.otherMember.mainSprite;
        bandNameText.text = sessionData.bandName + "'s New Song";
        otherMemberNameText.text = "Playing along with " + sessionData.otherName + "'s track.";

        gameObject.SetActive(true);


        StartCoroutine(StartupRoutine());

        IEnumerator StartupRoutine() {
            AudioManager.Instance.StopCrowdMurmur();
            yield return this.CreateAnimationRoutine(1.3f, (float progress) => {
                float easedProgress = Easing.easeOutSine(0, 1, progress);
                rectT.anchoredPosition = Vector2.Lerp(offScreenPos, onScreenPos, easedProgress);
            });
            yield return new WaitForSeconds(0.8f);
            songVisualizer.ShowPart(sessionData.otherPart, 0.5, 10);
        }
    }
}
