using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 마커 검색창 관리 클래스. 
/// - 검색 인터페이스 버튼들에 트리거 할당
/// </summary>

public class SearchManager : MonoBehaviour
{
    public GameObject SearchPanel;
    public GameObject MainPanel;
    public GameObject FindPanel;
    public GameObject SelectPanel;

    public Button SearchButton;//누르면 텍스트 내용 검색
    public InputField SearchPointNameInput;//찾을 포인트명
    public Button SearchDoneButton;//포인트 검색을 종료

    public Button SelectButton;//포인트 선택 버튼
    public Button FindCancleButton;//포인트 선택 종료버튼

    public PointManager pointManager;

    // Start is called before the first frame update
    void Start()
    {
        //버튼에 콜백함수 할당
        SearchButton.onClick.AddListener(SearchButtonCallBack);
        SearchDoneButton.onClick.AddListener(SearchDoneButtonCallBack);
        SelectButton.onClick.AddListener(SelectButtonCallBack);
        FindCancleButton.onClick.AddListener(FindCancleButtonCallBack);
    }
    
    void SearchButtonCallBack() //검색 버튼 콜백함수
    {
        //1.택스트 읽기
        string pointName = SearchPointNameInput.text; // 입력한 물건 이름 저장

        if (pointName == "") //텍스트가 비었다면 검색 안함
        {
            Debug.LogWarning("[SearchManager] point name is \"\"");
            return;
        }
        //2. 포인트 검색
        List<GameObject> searchedPointList = pointManager.SearchPoints(pointName);

        if (searchedPointList.Count == 0) {
            Debug.LogWarning("[SearchManager] searched nothing ");
            return;
        }
        //3.검색된 포인트 제외 전부 비활성화
        pointManager.InActiveAll();//모든 포인트 비활성화
        foreach (GameObject point in searchedPointList) {
            point.SetActive(true);//검색된 포인트 활성화
            point.GetComponent<PointController>().PointNameText.text = point.GetComponent<PointController>().pointName;
        }

        //4.FindPanel 실행
        SearchToFindPanel();
    }
    //선택 취소버튼 콜백
    void FindCancleButtonCallBack() //검색창으로 이동
    {
        pointManager.ActiveNomalize();
        FindToSearchPanel();
    }
    //검색 종료 버튼 콜백
    void SearchDoneButtonCallBack() //메인창으로 이동
    {
        SearchToMainPanel();
    }
    //선택버튼 콜백
    void SelectButtonCallBack() //포인트 변경창으로 이동
    {
        if (pointManager.Select())
        {
            pointManager.ActiveNomalize();
            FindToSelectPanel();
        }
    }
    //-----------------------------------------------------------------------
    //판넬 이동
    void SearchToFindPanel() {
        SearchPanel.SetActive(false);
        FindPanel.SetActive(true);
        InitSearchPanel();
    }
    void FindToSearchPanel() {
        SearchPanel.SetActive(true);
        FindPanel.SetActive(false);
        InitSearchPanel();
    }
    void SearchToMainPanel() {
        SearchPanel.SetActive(false);
        MainPanel.SetActive(true);
    }
    void FindToSelectPanel()
    {
        FindPanel.SetActive(false);
        SelectPanel.SetActive(true);
    }
    //-----------------------------------------------------------------------
    void InitSearchPanel() {
        SearchPointNameInput.text = "";
        SearchPointNameInput.placeholder.GetComponent<Text>().text = "찾을 포인트명"; // 플레이스홀더 업데이트
    }

}
