using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMFreelookSetting : MonoBehaviour
{
    CinemachineFreeLook freeLook;
    public float scrollSpeed = 2000.0f;
    // Start is called before the first frame update

    private void Awake()
    {
        CinemachineCore.GetInputAxis = ClickControl;
    }
    void Start()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
    }

    public float ClickControl(string axis)
    {
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        freeLook.m_Lens.FieldOfView += (scrollWheel * Time.deltaTime* scrollSpeed);

        return UnityEngine.Input.GetAxis(axis);
    }
}
