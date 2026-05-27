using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 카테고리 설정창 관리 클래스. 
/// 카테고리 설정창 UI와 버튼 콜백 함수들을 관리한다.
/// </summary>

public class CategorySettingManager : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject CategorySettingPanel;

    public InputField inputField; // Legacy InputField 연결
    public Button addButton; // 추가 버튼

    public Button finishButton; // 완료 버튼
    
    public RectTransform contentPanel; // 버튼을 추가할 배경
    
    public GameObject CategoryButtonPrefab; // 카테고리 버튼 프리팹
    public GameObject DeleteButtonPrefab;//삭제버튼 프리팹
    public PointManager pointManager;//포인트 매니저 인스턴스

    float buttonSpacing = 120f;// 버튼 간 간격
    Vector2 startPosition = new Vector2(0, 300);// 첫 버튼 위치
    List<GameObject> CategoryButtonList;//카테고리 버튼 리스트
    List<GameObject> DeleteButtonList;//삭제 버튼 리스트

    /// <summary>
    /// 시작 시 UI 초기화
    /// </summary>
    void Start()
    {
        print("ListManager start");
        //초기화
        CategoryButtonList = new List<GameObject>();
        DeleteButtonList = new List<GameObject>();
        //할당된 버튼이 하나라도 없다면 에러 출력
        if (addButton == null || DeleteButtonPrefab == null || finishButton == null || contentPanel == null || CategoryButtonPrefab == null || CategorySettingPanel == null)
        {
            Debug.LogError("One or more UI components are not assigned in the Inspector!");
            return;
        }

        // 버튼 이벤트 리스너 등록
        addButton.onClick.AddListener(AddCategoryButton);//버튼이 눌리면 카테고리가 활성화/비활성화 전환
        finishButton.onClick.AddListener(FinishCategorySetup);//종료
        //버튼 배치
        RepositionButtons();
    }

    /// <summary>
    /// ButtonList 버튼 모두 제거
    /// </summary>
    void ClearButtons()
    {
        foreach (GameObject button in CategoryButtonList) 
            Destroy(button);

        foreach (GameObject button in DeleteButtonList)
            Destroy(button);
        CategoryButtonList.Clear();
        DeleteButtonList.Clear();
    }

    /// <summary>
    /// 카테고리 추가버튼 핸들러
    /// </summary>
    void AddCategoryButton()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            string newCategoryName = inputField.text;

            if (!pointManager.CategoryContain(newCategoryName))//추가할 카테고리가 존재 하지 않는다면
            {
                print($"add category {newCategoryName}");
                pointManager.AddCategory(newCategoryName);//pointManager의 AddCategory로 카테고리 생성
                RepositionButtons();//카테고리 버튼 생성, 기본 활성화
                inputField.text = "";
                inputField.ActivateInputField();
            }
            else//추가할 카테고리가 이미 존재하는 카테고리라면?
            {
                Debug.LogWarning("Category already exists!");
            
        }
        else
        {//카테고리명을 비워뒀다면
            Debug.LogWarning("Input field is empty!");
        }
    }

    /// <summary>
    /// 버튼 생성 함수
    /// 매개변수 정보에 따라서 버튼을 생성한다.
    /// </summary>
    void CreateButton(string categoryName, bool isActive, int pos)
    {
        bool isNone = categoryName == "None";
        GameObject NewCategoryButton = Instantiate(CategoryButtonPrefab, contentPanel);
        NewCategoryButton.name = categoryName + "Button";

        // 버튼 텍스트 설정
        TextMeshProUGUI buttonText = NewCategoryButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = categoryName;
        }
        
        //버튼 위치 조정
        RectTransform CategoryButtonRect = NewCategoryButton.GetComponent<RectTransform>();
        if (CategoryButtonRect != null)
        {
            CategoryButtonRect.anchoredPosition = startPosition - new Vector2(0, pos * buttonSpacing);
        }

        // 버튼 클릭 이벤트 추가
        Button CategoryButton = NewCategoryButton.GetComponent<Button>();
        CategoryButton.onClick.AddListener(() => ToggleCategoryState(categoryName, NewCategoryButton));
        // 버튼 초기 시각적 상태 설정
        UpdateButtonVisualState(NewCategoryButton, isActive);// 새 버튼을 상태에따라 색상 변경
        CategoryButtonList.Add(NewCategoryButton);//버튼 리스트에 버튼 추가

        if (!isNone)
        {
            //삭제버튼 생성
            GameObject NewDeleteButton = Instantiate(DeleteButtonPrefab, contentPanel);
            NewDeleteButton.name = categoryName + "DeleteButton";

            //삭제버튼 배치
            RectTransform DeleteButtonRect = NewDeleteButton.GetComponent<RectTransform>();
            if (DeleteButtonRect != null)
            {
                DeleteButtonRect.anchoredPosition = startPosition - new Vector2(-300, pos * buttonSpacing);
            }
            //버튼에 콜백함수 등록
            Button DeleteButton = NewDeleteButton.GetComponent<Button>();
            DeleteButton.onClick.AddListener(() => DeleteCategoryButton(categoryName));
            //리스트에 버튼 추가
            DeleteButtonList.Add(NewDeleteButton);
        }
    }

    /// <summary>
    /// 버튼 활성화/비활성화 토글
    /// </summary>
    void ToggleCategoryState(string categoryName, GameObject button)
    {
        bool currentActive = pointManager.CategoryIsActive(categoryName);
        if (currentActive)//활성화된 상태라면
            pointManager.UnActiveCategory(categoryName);
        else//비활성화 상태라면
            pointManager.ActiveCategory(categoryName);
        currentActive = !currentActive;//현재 상태 변환
        UpdateButtonVisualState(button, currentActive);//버튼 색 변경
        Debug.Log($"Category '{categoryName}' is now {(currentActive ? "active" : "inactive")}.");
    }

    /// <summary>
    /// 현재 버튼 상태에 따라 색상 변경
    /// </summary>
    void UpdateButtonVisualState(GameObject button, bool isActive)
    {
        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = isActive ? Color.blue : Color.gray; // 활성화: 파랑, 비활성화: 회색
        }
    }

    /// <summary>
    /// 카테고리 삭제
    /// 삭제된 카테고리 버튼의 아래 버튼들이 삭제된 카테고리 버튼 자리를 대체한다.
    /// 모든 버튼을 지우고 삭제된 버튼을 제외한 버튼들을 다시 순서대로 생성함으로 구현했다.
    /// </summary>
    void DeleteCategoryButton(string categoryName)
    {
        pointManager.DeleteCategory(categoryName);//카테고리 삭제
        Debug.Log($"Deleted category: {categoryName}");//
        RepositionButtons();//버튼 재배치
    }

    /// <summary>
    /// 버튼 재정렬
    /// 모든 버튼을 제거하고 다시 생성한다.
    /// </summary>
    void RepositionButtons()
    {
        Dictionary<string, bool> categoriesStates = pointManager.GetCategoriesStates();
        ClearButtons();//버튼 전체 제거
        if (categoriesStates.Count != 0) {
            int pos = 0;
            foreach (KeyValuePair<string, bool> categoryState in categoriesStates) {
                CreateButton(categoryState.Key, categoryState.Value, pos);
                pos++;
            }
        }
        else {
            Debug.LogError("Category is empty");
        }
    }

    /// <summary>
    /// 완료 버튼 누를 때 호출
    /// </summary>
    void FinishCategorySetup()
    {
        if (CategorySettingPanel != null)
        {
            CategorySettingPanel.SetActive(false);
            MainPanel.SetActive(true);
        }
    }
}