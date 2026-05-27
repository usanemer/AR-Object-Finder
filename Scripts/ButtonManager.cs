using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json;
using System.IO;


/// <summary>
/// 공간 선택 씬(Scene)의 로직 및 상태를 총괄하는 관리 클래스다.
/// - 로컬 저장소(보조기억장치)로부터 공간 데이터 입출력(Read/Write) 처리
/// - 새로운 공간 생성 및 기존 공간 삭제 요청
/// - 공간 데이터의 변경 사항을 기반으로 관련 UI 동기화 및 제어
/// </summary>

public class ChoosePlaceSceneManager : MonoBehaviour
{
    public GameObject buttonPrefab;
    public RectTransform parentContainer;
    public InputField inputField;

    private PlaceData placeData;//ID, Name, nextID
    private float buttonHeight = 200f;
    private float buttonSpacing = 10f;
    private Vector2 startPosition = new Vector2(0, 500);
    private List<GameObject> buttonList;

    string path;
    string FileName;

    void Start()
    {
        buttonList = new List<GameObject>();
        path = Application.persistentDataPath + "/";
        FileName = "PlaceData";
        if (File.Exists(path + FileName))//저장파일이 있다면
            LoadData();
        else
            InitPlaceData();            
    }

    public void AddPlace()//추가 버튼을 누를경우.
    {
        if (parentContainer == null)
        {
            Debug.LogError("Parent Container is not assigned or has been destroyed!");
            return;
        }

        string placeName = inputField.text.Trim();

        placeData.places[placeData.nextPlaceID] = placeName;//ID에 place 추가
        Debug.Log($"Place Added: {placeName}, ID: {placeData.nextPlaceID}");
        placeData.nextPlaceID++;//ID 증가
        SaveData();
        //placeData만 변경하고 버튼은 RepositionButtons에서 추가삭제
        RepositionButtons();//버튼 재배치

        inputField.text = "";
        inputField.ActivateInputField();//텍스트 선택
    }

    public void SaveData()//placeData저장
    {
        string jsonData = JsonConvert.SerializeObject(placeData);
        File.WriteAllText(path + FileName, jsonData);
        Debug.Log($"Data saved. path : {path}");
    }

    private void LoadData()//placeData로드, 버튼 재배치
    {
        string jsonData = File.ReadAllText(path + FileName);
        placeData = JsonConvert.DeserializeObject<PlaceData>(jsonData);
        RepositionButtons();//버튼 재배치

        Debug.Log("Data loaded.");
    }

    private void OnPlaceButtonClicked(int placeID)//장소버튼 콜백함수
    {
        if (placeData.places.TryGetValue(placeID, out string placeName))
        {
            Debug.Log($"[CustomButtonManager] Button clicked for place: {placeName}, ID: {placeID}");

            //putScene에서 로드할 장소 정보 저장
            PlayerPrefs.SetInt("placeID", placeID);
            PlayerPrefs.SetString("placeName", placeName);

            SaveData();//place 데이터 저장
            Debug.Log($"[CustomButtonManager] Data saved: placeID = {placeID}, placeName = {placeName}");
            SceneManager.LoadScene("PutScene");//putScene으로 이동
        }
        else
        {
            Debug.LogError($"[CustomButtonManager] clicked button's Place ID {placeID} not found!");
        }
    }

    private void DeletePlace(int placeID, GameObject buttonToDelete)//삭제버튼 콜백함수
    {
        if (placeData.places.ContainsKey(placeID))
        {
            // 장소와 관련된 JSON 파일 삭제
            string placeName = placeData.places[placeID];
            string path = Application.persistentDataPath + "/";
            string fileName = placeName + placeID.ToString() + ".json";
            string fullPath = Path.Combine(path, fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Debug.Log($"Deleted points file for place: {placeName}, ID: {placeID}");
            }
            
            placeData.places.Remove(placeID); // Dictionary에서 삭제
            SaveData();
            RepositionButtons(); // 버튼 재배치
        }
        else
        {
            Debug.LogError($"Place ID {placeID} not found for deletion.");
        }
    }

    private GameObject CreateButton(int placeID, string placeName, int pos)//버튼만 생성한다.
    {
        GameObject newButton = Instantiate(buttonPrefab, parentContainer);

        // 버튼 텍스트 설정
        TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = $"{placeName}";
        }

        // 버튼 위치 및 크기 설정
        RectTransform buttonRect = newButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.sizeDelta = new Vector2(500f, buttonHeight);
            buttonRect.anchoredPosition = startPosition - new Vector2(0, pos * (buttonHeight + buttonSpacing));
        }

        // 장소 버튼 클릭 이벤트 등록 (씬 이동)
        newButton.GetComponent<Button>().onClick.AddListener(() => OnPlaceButtonClicked(placeID));

        // **삭제 버튼 이벤트 등록**
        Transform deleteButtonTransform = newButton.transform.Find("DeleteButton");
        if (deleteButtonTransform != null)
        {
            Button deleteButton = deleteButtonTransform.GetComponent<Button>();
            if (deleteButton != null)
            {
                deleteButton.onClick.AddListener(() => DeletePlace(placeID, newButton));
            }
            else
            {
                Debug.LogError("DeleteButton is missing a Button component in prefab.");
            }
        }
        else
        {
            Debug.LogError("DeleteButton not found in buttonPrefab.");
        }

        return newButton;
    }

    private void RepositionButtons()//버튼 재배치. 모든 버튼을 삭제하고, pointData에 따라서 버튼을 재배치한다.
    {
        foreach (GameObject button in buttonList) {//모든 버튼 삭제
            Destroy(button);
        }

        buttonList.Clear();
        int pos = 0;
        foreach (KeyValuePair<int, string> place in placeData.places) {
            buttonList.Add(CreateButton(place.Key, place.Value, pos));
            pos++;
        }
    }
    //place 초기화
    void InitPlaceData() {//세이브파일이 없는상황
        placeData = new PlaceData();
    }

    class PlaceData {
        public Dictionary<int, string> places;
        public int nextPlaceID;
        public PlaceData() {
            places = new Dictionary<int, string>();
            nextPlaceID = 1;//1이 최소값
        }
    }

}
