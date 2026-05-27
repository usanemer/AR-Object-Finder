using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 도형 변경 인터페이스의 버튼 콜백함수 할당
/// </summary>


public class EditDiagramManager : MonoBehaviour
{


    public GameObject EditDiagramPanel;
    public GameObject SelectPanel;

    public Button Rectangle;//0
    public Button Triangle;//1
    public Button Circle;//2
    public Button White;//0
    public Button Red;//1
    public Button Blue;//2

    public Button Done;

    public PointManager pointManager;


    // Start is called before the first frame update
    void Start()
    {
        Rectangle.onClick.AddListener(() => pointManager.ChangeMesh(0));
        Triangle.onClick.AddListener(() => pointManager.ChangeMesh(1));
        Circle.onClick.AddListener(() => pointManager.ChangeMesh(2));

        White.onClick.AddListener(() => pointManager.ChangeColor(0));
        Red.onClick.AddListener(() => pointManager.ChangeColor(1));
        Blue.onClick.AddListener(() => pointManager.ChangeColor(2));

        Done.onClick.AddListener(DoneCallBack);
    }

    void DoneCallBack() {
        EditDiagramPanel.SetActive(false);
        SelectPanel.SetActive(true);
    }


}
