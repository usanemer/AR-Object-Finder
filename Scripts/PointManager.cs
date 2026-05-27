using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text; // StringBuilder
using Newtonsoft.Json;//Json
using System.IO;
using TMPro;


/// <summary>
/// 마커 관리 클래스. 마커와 관련된 모든 처리를 담당한다. 
/// (코드에서 Point는 마커를 의미한다. 코드 작성당시엔 마커를 포인트라고 지칭했다. 최종 발표 전에 명칭을 바꿨다.)
/// 
/// [데이터 구조]
/// - 마커 : Dictionary<string, Category> string 타입인 key는 카테고리명. Category 클래스 안에 마커 리스트가 존재한다.
///     카테고리명으로 탐색할 수 있도록 Dictionary를 사용했고, 카테고리 활성화 여부와 마커 리스트를 저장하기 위해 Category클래스를 사용했다.
/// [주요 기능]
/// - 선택한 공간 정보에 해당하는 마커 정보를 읽거나 쓴다.
/// - 마커 추가 제거
/// - 마커 정보 수정
/// - 마커 Transform 수정
/// - 마커 색상, 모양 수정
/// - 카테고리 생성/제거, 활성화/비활성화
/// </summary>


public class PointManager : MonoBehaviour
{
    public GameObject user;//사용자 위치
    public GameObject pointPrefab;
    public TMP_Text PointNameTextPrefab;//기본값은 None
    public GameObject crossHair;
    public Mesh[] meshs;
    public Color32[] colors;
    public int moveSpeed;
    public int scaleSpeed;
    public int rotateSpeed;

    //저장파일 경로
    string path;
    string FileName;

    Dictionary<string, Category> categories; // key는 카테고리 명, value는 카테고리 클래스(카테고리 내의 포인터 리스트+활성화여부 bool값)
    GameObject selectedPoint; // 선택된 포인트
    static int NextPointID = 0;
    int mode = 0; // 0은 이동, 1은 크기변환, 2는 회전
    int placeID;//공간 ID
    string placeName;//공간 이름

    PointController pointController;

    Vector3[] pos; // RLUDFB(right left up down front back)
    Vector3[] scale; // RLUDFB
    Vector3[] rotate;// RLUDFB

    // 다른 스크립트들의 초기화보다 시간적으로 먼저 진행되는 초기화 함수.
    void Awake()
    {
        print("PointManager Awake");
        //선택된 마커 없음
        selectedPoint = null;
        //공간 정보 읽기
        placeID = PlayerPrefs.GetInt("placeID");
        placeName = PlayerPrefs.GetString("placeName" );
        //공간 정보가 저장된 경로 생성
        path = Application.persistentDataPath + "/";
        FileName = placeName + placeID.ToString();
        //마커 Transform 변환용 벡터 초기화
        InitVector();
        //경로상에 파일이 존재한다면 읽어오기
        if (File.Exists(path + FileName))
        {
            LoadPoints();
            print($"Load {placeName}");
        }
        else InitCategory();//경로상에 파일이 존재하지 않으면 초기화
    }

    //------------------------------------------------------------------------------------
    // 포인터 생성 삭제
    public void AddPoint()
    {
        // 원하는 초기 위치와 회전 설정
        Vector3 customPosition = new Vector3(0, 2, 0); // 초기 위치
        Quaternion customRotation = Quaternion.identity; // 초기 회전

        // 포인트 생성
        selectedPoint = Instantiate(pointPrefab, customPosition, customRotation);
        pointController = selectedPoint.GetComponent<PointController>();
        //포인터 텍스트 생성
        if (PointNameTextPrefab != null)
        {
            pointController.PointNameText = Instantiate(PointNameTextPrefab, customPosition, customRotation);
        }
        else
            Debug.LogError("[PointManager] point name text prefab is null");
        // 정보 설정
        pointController.ID = NextPointID;
        pointController.User = user;
        selectedPoint.name = "None";

        //카테고리에 추가
        categories["None"].pointList.Add(selectedPoint);

        // 로그 출력
        Debug.Log($"Point added. ID: {NextPointID}, Position: {customPosition}");

        // ID 증가
        NextPointID++;
    }

    public void DeletePoint() // 선택한 포인트 삭제
    {
        if (selectedPoint == null)
        {
            Debug.LogWarning("No selected point to delete!");
            return;
        }
        categories[pointController.categoryName].pointList.Remove(selectedPoint);
        Debug.Log($"Point deleted. Name: {pointController.pointName}"); // 로그 추가
        Destroy(pointController.PointNameText);
        Destroy(selectedPoint);
        
        selectedPoint = null;
    }
    //------------------------------------------------------------------------------------
    // transform 수정
    public void ChangeTransform(int num) // 스케일과 위치, 각도를 바꿈
    {
        if (selectedPoint == null)
        {
            Debug.LogWarning("No selected point to transform!");
            return;
        }

        Debug.Log($"ChangeTransform called. Mode: {mode}, Direction: {num}"); // 로그 추가

        switch (mode)
        {
            case 0:
                ChangePos(num);
                break;
            case 1:
                ChangeScale(num);
                break;
            case 2:
                ChangeRotation(num);
                break;
        }
    }

    public void EditPos() // 상하좌우 버튼이 위치를 변환한다.
    {
        mode = 0;
        Debug.Log("EditPos called. Mode set to Position."); // 로그 추가
    }

    public void EditScale() // 상하좌우 버튼이 크기를 변환한다.
    {
        mode = 1;
        Debug.Log("EditScale called. Mode set to Scale."); // 로그 추가
    }

    public void EditRotation() // 상하좌우 버튼이 포인터를 회전시킨다.
    {
        mode = 2;
        Debug.Log("EditRotation called. Mode set to Rotation."); // 로그 추가
    }

    //------------------------------------------------------------------------------------
    // 포인터 도형 변경
    public void ChangeColor(int num) // color변수값으로 선택된 포인트 색을 바꾼다.
    {
        if (selectedPoint == null)
        {
            Debug.LogWarning("No selected point to change color!");
            return;
        }
        if (num < 0 || num >= colors.Length)
        {
            Debug.LogError($"Invalid color index: {num}. Array size: {colors.Length}");
            return;
        }
        selectedPoint.GetComponent<MeshRenderer>().material.color = colors[num];
        pointController.color = num;
        Debug.Log($"Color changed to index: {num}"); // 로그 추가
    }
    public void ChangeMesh(int num) // 누른 버튼에 해당되는 도형으로 포인트를 바꾼다.
    {
        if (selectedPoint == null)
        {
            Debug.LogWarning("No selected point to change mesh!");
            return;
        }
        if (num < 0 || num >= meshs.Length)
        {
            Debug.LogError($"Invalid mesh index: {num}. Array size: {meshs.Length}");
            return;
        }
        selectedPoint.GetComponent<MeshFilter>().mesh = meshs[num];
        pointController.mesh = num;
        Debug.Log($"Mesh changed to index: {num}"); // 로그 추가
    }
    //------------------------------------------------------------------------------------
    // 포인터 정보 수정
    public void ChangePointName(string pointName)
    {
        if (selectedPoint == null)
        { //선택된 포인트가 없다면
            Debug.LogWarning("[PointManager] No selected point to rename!");
            return;
        }
        //pointController.pointName = pointName;//포인트 이름 파라미터로 변경
        selectedPoint.GetComponent<PointController>().pointName = pointName;//포인트 이름 파라미터로 변경
        selectedPoint.name = pointName;//포인트 객체 이름도 변경
        pointController.PointNameText.text = pointName;
        Debug.Log($"[PointManager] Point renamed to: {pointName}"); // 로그 추가
    }
    public void ChangeMemo(string memo) {
        if (selectedPoint == null)
        { //선택된 포인트가 없다면
            Debug.LogWarning("[PointManager] No selected point to rename!");
            return;
        }
        pointController.memo = memo;
        Debug.Log($"[PointManager] Point \"{pointController.pointName}\" memo changed"); // 로그 추가
    }
    //------------------------------------------------------------------------------------
    // 카테고리 관련
    public void AddCategory(string name) // 카테고리명 텍스트에 입력한 값으로 카테고리를 새로 생성한다.
    {
        if (!categories.ContainsKey(name))
        {
            categories[name] = new Category();
            Debug.Log($"[PointManager]Category added: {name}"); // 로그 추가
        }
    }

    public void ChangePointCategory(string categoryName) // 선택한 포인트의 카테고리를 카테고리명 텍스트값으로 바꾼다.
    {
        if (selectedPoint == null)
        {
            Debug.LogWarning("[PointManager]No selected point to change category!");
            return;
        }
        categories[pointController.categoryName].pointList.Remove(selectedPoint); // 카테고리에서 선택된 포인트 제거
        pointController.categoryName = categoryName; // 선택된 포인트 바꿀 카테고리로 카테고리명 변경
        AddCategory(categoryName); // 카테고리가 없다면 새로 생성
        categories[categoryName].pointList.Add(selectedPoint); // 카테고리에 선택된 포인트 추가
        selectedPoint.SetActive(CategoryIsActive(categoryName));
        Debug.Log($"[PointManager] Point category changed to: {categoryName}"); // 로그 추가
    }

    public void UnActiveCategory(string name) // 카테고리를 비활성화 한다.
    {
        categories[name].isActive = false;
        foreach (GameObject point in categories[name].pointList)
        {
            point.SetActive(false);
            point.GetComponent<PointController>().PointNameText.text = "";
        }
        Debug.Log($"[PointManager] Category deactivated: {name}"); // 로그 추가
    }

    public void ActiveCategory(string name) // 카테고리를 활성화 한다.
    {
        categories[name].isActive = true;
        foreach (GameObject point in categories[name].pointList)
        {
            point.SetActive(true);
            point.GetComponent<PointController>().PointNameText.text = point.GetComponent<PointController>().pointName;
        }
        Debug.Log($"[PointManager] Category activated: {name}"); // 로그 추가
    }

    public void DeleteCategory(string name) // 카테고리를 삭제한다. 삭제된 카테고리 내의 포인트들은 None카테고리로
    {
        categories["None"].pointList.AddRange(categories[name].pointList);
        categories.Remove(name);
        Debug.Log($"[PointManager] Category deleted: {name}"); // 로그 추가
    }

    public void PrintAllCategory() // 모든 카테고리의 내용을 Log 출력한다.
    {
        StringBuilder stringBuilder = new StringBuilder();
        
        foreach (KeyValuePair<string, Category> category in categories) // 카테고리 하나씩 탐색
        {
            stringBuilder.Append(category.Key);
            stringBuilder.Append(" : ");
            foreach (GameObject point in category.Value.pointList)
            {
                stringBuilder.Append(point.GetComponent<PointController>().pointName);
                stringBuilder.Append(" ");
            }
            stringBuilder.Append("| ");
        }
        Debug.Log("[PointManager] " + stringBuilder.ToString());
    }

    public bool CategoryIsActive(string categoryName) //파라미터로 받은 카테고리의 활성화 여부 반환
    {
        return categories[categoryName].isActive;
    }

    public bool CategoryContain(string categoryName) {
        return categories.ContainsKey(categoryName);
    }
    //------------------------------------------------------------------------------------
    //포인터 선택관련 함수
    public bool Select()//에임에 맞춰진 포인터 선택.
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2 + 160, 0);

        // 화면 중앙에서 Ray 생성
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        RaycastHit hit;
        print($"{ray.origin.x}, {ray.origin.y}, {ray.origin.z} -> {ray.direction.x}, {ray.direction.y}, {ray.direction.z}");
        if (Physics.Raycast(ray, out hit, 10000))
        {
            GameObject touchedObject = hit.transform.gameObject;
            print(touchedObject.GetComponent<PointController>().pointName);

            // PointController가 있는지 확인
            print(touchedObject.GetComponent<PointController>().pointName);

            // 현재 선택된 포인트로 설정
            print(touchedObject.GetComponent<PointController>().pointName);
            selectedPoint = touchedObject;
            pointController = selectedPoint.GetComponent<PointController>();
            Debug.Log("[PointManager] Selected Point: " + pointController.pointName);
            return true;
        }
        else
        {
            Debug.LogWarning($"[PointManager] selected nothing");
            return false;
        }
    }

    public void Select(GameObject NewSelectPoint) // 기존 포인터 해제후, 파라미터로 들어온 포인트 선택
    {
        if (NewSelectPoint == null) {
            Debug.LogError("[PointManager] New select point is null");
        }
        SelectEnd();//선택되있던 포인트를 선택 해제한다.
        selectedPoint = NewSelectPoint;
        selectedPoint.SetActive(true);
    }

    public bool pointNotSelected() {
        if (selectedPoint == null) return true;
        return false;
    }

    //선택했던 포인트 선택 해제.
    public void SelectEnd() // selectPoint null 입력. 카테고리 비활성화시 포인트도 비활성화하고 선택 해제
    {
        string categoryName = pointController.categoryName;
        if (!categories[categoryName].isActive)//비활성화시
            selectedPoint.SetActive(false);//비활성화
        selectedPoint = null;
    }
    //------------------------------------------------------------------------------------
    //정보 반환
    public Dictionary<string, bool> GetCategoriesStates() //모든 카테고리의 활성화 여부를 리스트로 반환. 
    {
        Dictionary<string, bool> categoriesStates = new Dictionary<string, bool>();
        if (categories.Count == 0)
        {
            print("category is empty");
        }
        else
            foreach (KeyValuePair<string, Category> category in categories)
            {
                categoriesStates.Add(category.Key, category.Value.isActive);
            }
        return categoriesStates;
    }
    public List<string> GetCategoriesNameList()
    {
        List<string> categoryNameList = new List<string>();
        foreach (string categoryName in categories.Keys)
        {
            categoryNameList.Add(categoryName);
        }
        return categoryNameList;
    }
    public string GetPointMemo()//선택된 포인트의 메모 반환
    {
        return pointController.memo;
    }
    public string GetPointName()//선택된 포인트의 이름 반환

    {
        return pointController.pointName;
    }
    public string GetPointCategory()//선택된 포인트의 카테고리 반환
    {
        return pointController.categoryName;
    }
    //------------------------------------------------------------------------------------
    // 
    void ChangePos(int num) // 상하좌우 이동 연산.
    {
        Vector3 userPos;
        Vector3 pointPos;
        Vector3 targetDir;
        Vector3 rotationAxis;
        Vector3 front = new Vector3(0, 0, -1);
        float angle;
        userPos = user.transform.position;
        pointPos = selectedPoint.transform.position;
        pointPos.y = userPos.y;
        targetDir = (userPos - pointPos).normalized;
        rotationAxis = Vector3.Cross(front, targetDir);
        angle = Mathf.Acos(Vector3.Dot(front, targetDir)) * Mathf.Rad2Deg;

        Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
        selectedPoint.transform.Translate(rotation * pos[num] * moveSpeed, Space.World);
        pointController.PointNameText.transform.Translate(rotation * pos[num] * moveSpeed, Space.World);
        Debug.Log($"[PointManager] Position changed. Direction: {pos[num]}"); // 로그 추가
    }
    void ChangeScale(int num) // 상하좌우 크기 변환 연산
    {
        selectedPoint.transform.localScale += scale[num] * scaleSpeed;
        Debug.Log($"[PointManager] Scale changed. Scale vector: {scale[num]}"); // 로그 추가
    }
    void ChangeRotation(int num) // 포인트의 Rotation 변환
    {
        selectedPoint.transform.Rotate(rotate[num] * rotateSpeed, Space.World);
        Debug.Log($"[PointManager] Rotation changed. Rotation vector: {rotate[num]}"); // 로그 추가
    }
    void InitVector() // vector 배열들을 초기화 한다.
    {
        pos = new Vector3[6] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        scale = new Vector3[6] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        rotate = new Vector3[6] { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    }
    void InitCategory() // 카테고리를 초기화 한다.("None" 카테고리 생성)
    {
        categories = new Dictionary<string, Category>();
        if (!categories.ContainsKey("None"))
        {
            categories["None"] = new Category();
        }
    }
    //------------------------------------------------------------------------------------
    //저장 로드
    public void SavePoints()//포인트 정보를 저장한다.
    {
        //다음 포인터 ID저장
        PointsData data = new PointsData();
        string jsonData;
        data.CategoriesToData(categories);
        data.NextPointID = NextPointID;
        jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path + FileName, jsonData);
        print(jsonData);
        print(path);

        print("[PointManager] Save scene");
    }
    public void LoadPoints()//저장된 카테고리, 포인터 다시 생성
    {
        string jsonData = File.ReadAllText(path + FileName);
        bool isActive;
        Vector3 prefabScale = new Vector3(
            pointPrefab.transform.localScale.x,
            pointPrefab.transform.localScale.y,
            pointPrefab.transform.localScale.z
            );
        PointsData data = JsonConvert.DeserializeObject<PointsData>(jsonData);//json을 다시 데이터로

        categories = new Dictionary<string, Category>();//카테고리 초기화
        NextPointID = data.NextPointID;//다음 포인터 ID불러오기

        foreach (KeyValuePair<string, CategoryData> categoryData in data.categoriesData)
        {
            categories[categoryData.Key] = new Category();//카테고리 생성
            isActive = categoryData.Value.isActive;
            categories[categoryData.Key].isActive = isActive;//카테고리 활성화 여부 로드

            foreach (PointData pointData in categoryData.Value.pointDataList)
            {
                selectedPoint = Instantiate(
                                        pointPrefab,
                                        new Vector3(pointData.posx, pointData.posy, pointData.posz),//위치
                                        Quaternion.Euler(pointData.rotationx, pointData.rotationy, pointData.rotationz)//각도
                                    );//포인터 생성
                pointController = selectedPoint.GetComponent<PointController>();
                selectedPoint.transform.localScale += new Vector3(pointData.scalex - prefabScale.x, pointData.scaley - prefabScale.y, pointData.scalez - prefabScale.z);//크기
                selectedPoint.transform.Rotate(pointData.rotationx, pointData.rotationy, pointData.rotationz);
                print($"{pointData.pointName}.rotate : {pointData.rotationx}, {pointData.rotationy}, {pointData.rotationz}");
                ChangeColor(pointData.color);//색
                ChangeMesh(pointData.mesh);//모양
                pointController.pointName = pointData.pointName;//이름
                selectedPoint.gameObject.name = pointData.pointName;
                pointController.categoryName = pointData.categoryName;//카테고리명
                pointController.ID = pointData.ID;//ID
                pointController.memo = pointData.memo;//메모
                pointController.PointNameText = Instantiate(//텍스트 생성
                                        PointNameTextPrefab,
                                        new Vector3(pointData.posx, pointData.posy, pointData.posz),//위치
                                        Quaternion.Euler(pointData.rotationx, pointData.rotationy, pointData.rotationz)//각도
                                    );
                pointController.PointNameText.text = pointData.pointName;//텍스트에 포인트명 
                pointController.User = user;
                categories[categoryData.Key].pointList.Add(selectedPoint);//카테고리에 추가
                if (!isActive)
                {
                    selectedPoint.SetActive(false);//활/비활
                }
            }
        }
        print("[PointManager] Load scene");
    }
    //------------------------------------------------------------------------------------
    //포인트 검색 함수
    public List<GameObject> SearchPoints(string pointName) //검색한 이름의 포인트들을 리스트로 반환
    {
        List<GameObject> searchPoints = new List<GameObject>(); 
        foreach (KeyValuePair<string, Category> category in categories) {
            foreach (GameObject point in category.Value.pointList) {
                if (point.GetComponent<PointController>().pointName == pointName)
                {
                    searchPoints.Add(point);
                }
            }
        }
        return searchPoints;
    }
    //------------------------------------------------------------------------------------
    //전체 비활성화, 복구
    public void InActiveAll()//포인트 전부 비활성화
    {
        if (categories.Count == 0) {
            Debug.LogWarning("[PointManager] no points");
            return;
        }
        foreach (KeyValuePair<string, Category> category in categories)
        {
            foreach (GameObject point in category.Value.pointList)
            {
                point.SetActive(false);//모든 포인트 비활성화
                point.GetComponent<PointController>().PointNameText.text = "";
            }
        }
        Debug.Log("[PointManager] All points inactivated");
    }
    public void ActiveNomalize() // 포인트 활성화, 비활성화 상황 복구
    {
        bool categoryIsActive;
        foreach (KeyValuePair<string, Category> category in categories)
        {
            categoryIsActive = category.Value.isActive;
            foreach (GameObject point in category.Value.pointList)
            {
                point.SetActive(categoryIsActive);//설정된 활성화 여부에 따라 활/비활성화.
                if (categoryIsActive) {
                    point.GetComponent<PointController>().PointNameText.text = point.GetComponent<PointController>().pointName;
                }
            }
        }
        if (selectedPoint == null)
        {
            Debug.Log("[PointManager] selected point is null");
            return;
        }
        Debug.Log("[PointManager] All points active nomalize");
        selectedPoint.SetActive(true);//포인트 활성화
        selectedPoint.GetComponent<PointController>().PointNameText.text = selectedPoint.GetComponent<PointController>().pointName;
    }
    //------------------------------------------------------------------------------------

    // 마커 데이터 클래스
 
    class PointsData
    {
        public Dictionary<string, CategoryData> categoriesData;
        public int NextPointID;
        public PointsData()
        {
            categoriesData = new Dictionary<string, CategoryData>();
        }

        public void CategoriesToData(Dictionary<string, Category> categories)
        {
            CategoryData categoryData;
            PointController pointController;
            Transform transform;
            foreach (KeyValuePair<string, Category> category in categories)
            {
                categoriesData[category.Key] = new CategoryData();
                categoryData = categoriesData[category.Key];
                categoryData.isActive = category.Value.isActive;
                foreach (GameObject point in category.Value.pointList)
                {
                    pointController = point.GetComponent<PointController>();
                    transform = point.GetComponent<Transform>();
                    categoryData.pointDataList.Add(
                        new PointData()
                        {
                            ID = pointController.ID,
                            pointName = pointController.pointName,
                            categoryName = pointController.categoryName,
                            color = pointController.color,
                            mesh = pointController.mesh,
                            memo = pointController.memo,
                            posx = transform.position.x,
                            posy = transform.position.y,
                            posz = transform.position.z,
                            rotationx = transform.rotation.x,
                            rotationy = transform.rotation.y,
                            rotationz = transform.rotation.z,
                            scalex = transform.localScale.x,
                            scaley = transform.localScale.y,
                            scalez = transform.localScale.z
                        }
                    );
                }
            }
            print("[InfoPanel] points turned into data");
        }
    }
    class CategoryData
    {
        public bool isActive;
        public List<PointData> pointDataList;

        public CategoryData()
        {
            pointDataList = new List<PointData>();
        }
    }
    class PointData
    {
        public int ID;
        public string pointName;
        public string categoryName;
        public int color;
        public int mesh;
        public string memo;
        public float posx, posy, posz;
        public float rotationx, rotationy, rotationz;
        public float scalex, scaley, scalez;
    }
    public class Category
    {
        public bool isActive;
        public List<GameObject> pointList;

        public Category()
        {
            pointList = new List<GameObject>();
            isActive = true;//기본은 활성화
        }
    }
}
