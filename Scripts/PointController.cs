using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



/// <summary>
/// 마커 정보를 저장하고, 마커 텍스트를 사용자 방향으로 회전시키는 클래스.
/// 코드를 단순화를 위해 마커 관리 코드에서 모든 마커 각도를 수정하기보단 마커 자체에서 자신의 텍스트 각도를 수정한다.
/// </summary>


public class PointController : MonoBehaviour
{
    public string pointName;
    public string categoryName;
    public int ID;
    public int color;
    public int mesh;
    public string memo;
    public TMP_Text PointNameText;
    public GameObject User;

    public void Update()
    {
        PointNameText.transform.LookAt(User.transform); 
        PointNameText.transform.Rotate(0, 180, 0);
    }

}
