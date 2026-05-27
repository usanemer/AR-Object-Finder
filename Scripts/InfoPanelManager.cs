using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 마커 정보창 관리 클래스.
/// - 마커 정보 수정시 데이터 변경 및 관련 UI 동기화
/// </summary>

public class InfoPanelManager : MonoBehaviour
{
    public GameObject InfoPanel; // Panel GameObject
    public GameObject SelectPanel;

    public InputField pointNameInput; // 물건 이름 입력 필드
    public InputField memoInputField; // 메모 입력 필드
    public TMP_Dropdown categoryDropdown; // 카테고리 드롭다운
    public Button showInfoButton; // 인포 시작 버튼
    public Button saveNameButton; // 물건 이름 저장 버튼
    public Button DoneButton; // 완료 버튼
    public Button saveMemoButton; // 메모 수정버튼
    public Button closeButton; // 닫기 버튼

    public PointManager pointManager; // PointManager 참조 (Inspector에서 할당)
    List<string> categoryNameList; //카테고리 이름 리스트

    void Start()
    {
        print("[InfoPanel] InfoPanel start");
        categoryNameList = new List<string>();
        // 이벤트 리스너 추가
        showInfoButton.onClick.AddListener(UpdateInfo);
        saveNameButton.onClick.AddListener(ChangePointName);
        saveMemoButton.onClick.AddListener(ChangePointMemo);
        DoneButton.onClick.AddListener(DoneCallback);
        closeButton.onClick.AddListener(DoneCallback);
    }

    // 물건 이름 저장 및 플레이스홀더 업데이트
    public void ChangePointName()
    {
        string pointName = pointNameInput.text; // 입력한 물건 이름 저장
        pointNameInput.placeholder.GetComponent<Text>().text = pointName; // 플레이스홀더 업데이트

        Debug.Log("[InfoPanel] SaveItemName called.");

        // PointManager에 이름 업데이트 요청
        if (pointManager != null)
        {
            Debug.Log("[InfoPanel] Calling UpdatePointName...");
            pointManager.ChangePointName(pointName); // PointManager로 포인트 이름 전달
        }
        else
        {
            Debug.LogWarning("[InfoPanel] PointManager is not assigned in InfoPanel!");
        }

        Debug.Log($"[InfoPanel] Saved Item Name: {pointName}");
    }
    // 드롭다운 초기화
    // 드롭다운의 내용을 전부 삭제하고 새로 생성
    void UpdateDropdown(int value)
    {
        categoryDropdown.ClearOptions();//전부 삭제
        categoryDropdown.AddOptions(categoryNameList);//업데이트된 카테고리 추가
        categoryDropdown.onValueChanged.AddListener(delegate {UpdatePointCategory();});
        //기본값 선택 (첫 번째 항목인 "카테고리 없음")
        categoryDropdown.value = value;
        //UpdatePointCategory();//초기 선택된 값 반영
    }

    // 선택된 카테고리로 포인트 변경
    void UpdatePointCategory()
    {
        string selectedCategory = categoryDropdown.options[categoryDropdown.value].text;
        pointManager.ChangePointCategory(selectedCategory);
        Debug.Log($"[InfoPanel] Selected Category: {selectedCategory}");
    }

    //메모 저장
    public void ChangePointMemo() {
        pointManager.ChangeMemo(memoInputField.text);
    }

    // Panel 닫기
    public void DoneCallback()
    {
        print("[InfoManager] Done");
        // Panel 닫기
        HidePanel();
        SelectPanel.SetActive(true); 
    }

    // Panel 숨기기
    public void HidePanel()
    {
        InfoPanel.SetActive(false);
        print("[InfoManager] Hide info panel");
    }

    // Panel 열기 (외부에서 호출 가능)
    public void ShowPanel()
    {
        InfoPanel.SetActive(true);
    }

    public void UpdateInfo() //인포창이 활성화 될때 이름, 카테고리, 메모도 같이 업데이트
    {
        if (pointManager.pointNotSelected())
        {
            HidePanel();
            Debug.LogWarning("[InfoPanel] point not selected");
            return;
        }
        else {
            categoryNameList = pointManager.GetCategoriesNameList();//카테고리 리스트 초기화
            pointNameInput.placeholder.GetComponent<Text>().text = pointManager.GetPointName(); // 플레이스홀더 업데이트
            pointNameInput.text = "";
            UpdateDropdown(categoryNameList.IndexOf(pointManager.GetPointCategory()));//선택된 포인트의 카테고리로 드랍다운의 선택값 변경
            memoInputField.text = pointManager.GetPointMemo();
            Debug.Log("[InfoPanel] Updated info");
        }
    }
}
