using UnityEngine;
using System.Collections.Generic;

public class SectionManager : MonoBehaviour
{
    [Header("여러 개의 Section 프리팹 목록")]
    [Tooltip("다양한 형태의 Section 프리팹들을 저장하고 관리할 때 사용합니다.")]
    [SerializeField] private List<GameObject> sectionPrefabs = new List<GameObject>();

    public List<GameObject> SectionPrefabs
    {
        get => sectionPrefabs;
        set => sectionPrefabs = value;
    }

}
