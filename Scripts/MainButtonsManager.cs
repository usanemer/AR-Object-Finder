using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



/// <summary>
/// 기본 인터페이스의 버튼들에 콜백함수를 할당한다.
/// </summary>



public class MainButtonsManager : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject SelectPanel;
    public GameObject SearchPanel;
    public GameObject CategoryPanel;

    public Button PCAButton;
    public Button SaveButton;
    public Button GoBackButton;
    public Button SelectButton;
    public Button AddButton;
    public Button SettingCategoryButton;
    public Button DeletePointButton;
    public Button SearchButton;

    public PointManager pointManager;


    // Start is called before the first frame update
    void Start()
    {
        PCAButton.onClick.AddListener(pointManager.PrintAllCategory);
        SaveButton.onClick.AddListener(pointManager.SavePoints);
        GoBackButton.onClick.AddListener(GoBackButtonCallBack);
        SelectButton.onClick.AddListener(SelectButtonCallBack);
        AddButton.onClick.AddListener(pointManager.AddPoint);
        DeletePointButton.onClick.AddListener(pointManager.DeletePoint);
        SettingCategoryButton.onClick.AddListener(SettingCategoryButtonCallBack);
        SearchButton.onClick.AddListener(MainToSearchPanel);
    }
    void SelectButtonCallBack() {
        if(pointManager.Select())//마커 선택 콜백함수
        {
            //pointManager.Select()가 마커를 선택했다면 true 선택하지 않았다면 false
            MainPanel.SetActive(false);
            SelectPanel.SetActive(true);
        }
    }
    void SettingCategoryButtonCallBack() {//카테고리 설정버튼 콜백함수
        CategoryPanel.SetActive(true);
        MainPanel.SetActive(false);
    }
    void GoBackButtonCallBack()//뒤로가기 버튼 콜백함수
    {
        SceneManager.LoadScene("FirstScene", LoadSceneMode.Single);
    }
    void MainToSearchPanel() {
        SearchPanel.SetActive(true);
        MainPanel.SetActive(false);
    }
}
