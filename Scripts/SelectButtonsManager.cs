using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// 마커 선택 인터페이스의 버튼들의 콜백함수 할당
/// </summary>



public class SelectButtonsManager : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject SelectPanel;
    public GameObject EditDiagramPanel;
    public GameObject InfoPanel;

    public Button ToFrontButton;
    public Button ToBackButton;
    public Button ToDownButton;
    public Button ToUPButton;
    public Button ToLeftButton;
    public Button ToRightButton;
    public Button SetRotateButton;
    public Button ChangePositionButton;
    public Button ChangeScaleButton;
    public Button ChangeDiagramButton;
    public Button DoneButton;
    public Button ShowInfoButton;

    public PointManager pointManager;

    // Start is called before the first frame update
    void Start()
    {
        ToFrontButton.onClick.AddListener(() => pointManager.ChangeTransform(4));
        ToBackButton.onClick.AddListener(() => pointManager.ChangeTransform(5));
        ToDownButton.onClick.AddListener(() => pointManager.ChangeTransform(3));
        ToUPButton.onClick.AddListener(() => pointManager.ChangeTransform(2));
        ToLeftButton.onClick.AddListener(() => pointManager.ChangeTransform(1));
        ToRightButton.onClick.AddListener(() => pointManager.ChangeTransform(0));
        SetRotateButton.onClick.AddListener(pointManager.EditRotation);
        ChangePositionButton.onClick.AddListener(pointManager.EditPos);
        ChangeScaleButton.onClick.AddListener(pointManager.EditScale);
        ChangeDiagramButton.onClick.AddListener(ChangeDiagramButtonCallBack);
        DoneButton.onClick.AddListener(DoneButtonCallBack);
        ShowInfoButton.onClick.AddListener(ShowInfoButtonCallBack);
    }


    public void ChangeDiagramButtonCallBack() {//도형변경 버튼 콜백함수
        SelectPanel.SetActive(false);
        EditDiagramPanel.SetActive(true);
    }
    public void DoneButtonCallBack() {//완료버튼 콜백함수
        SelectPanel.SetActive(false);
        MainPanel.SetActive(true);
        pointManager.SelectEnd();
    }

    public void ShowInfoButtonCallBack()//정보 확인 버튼 콜백함수
    {
        SelectPanel.SetActive(false);
        InfoPanel.SetActive(true);
    }
}
