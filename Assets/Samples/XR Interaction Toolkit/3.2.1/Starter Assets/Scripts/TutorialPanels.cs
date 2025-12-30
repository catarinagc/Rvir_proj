using UnityEngine;

public class TutorialPanels : MonoBehaviour
{
    public GameObject shortIntroduction;
    public GameObject leftAndRight;
    public GameObject speedControl;
    public GameObject selectingBalls;

    public void ShowShortIntroduction()
    {
        shortIntroduction.SetActive(true);
        leftAndRight.SetActive(false);
        speedControl.SetActive(false);
        selectingBalls.SetActive(false);
    }

    public void ShowLeftAndRight()
    {
        shortIntroduction.SetActive(false);
        leftAndRight.SetActive(true);
        speedControl.SetActive(false);
        selectingBalls.SetActive(false);
    }

    public void ShowSpeedControl()
    {
        shortIntroduction.SetActive(false);
        leftAndRight.SetActive(false);
        speedControl.SetActive(true);
        selectingBalls.SetActive(false);
    }

    public void ShowSelectingBalls()
    {
        shortIntroduction.SetActive(false);
        leftAndRight.SetActive(false);
        speedControl.SetActive(false);
        selectingBalls.SetActive(true);
    }
}
