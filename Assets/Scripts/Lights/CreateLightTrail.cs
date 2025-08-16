using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CreateLightTrail : MonoBehaviour
{
    public GameObject lightTrail;
    private Light2D _lightTrail;
    private int _maxPoints = 9;
    private List<Vector3> lightTrailPoints = new List<Vector3>();
    void Start()
    {
        GameObject light = Instantiate(lightTrail, transform.position, Quaternion.identity);
        _lightTrail = light.GetComponent<Light2D>();
        SetLightTrailPoints();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetLightTrailPoints();
    }

    void SetLightTrailPoints()
    {
        List<Vector3> tempPoints = new List<Vector3>();

        Vector3 localPosition = _lightTrail.transform.InverseTransformPoint(transform.position);
        lightTrailPoints.Add(localPosition);

        if(lightTrailPoints.Count > _maxPoints) 
        {
            lightTrailPoints.RemoveAt(0);
        }

        foreach(Vector3 point in lightTrailPoints) 
        {
            Vector3 pointWithOffset = new Vector3(point.x, point.y + 0.25f, point.z);
            tempPoints.Add(pointWithOffset);
        }

        if (tempPoints.Count > 2) 
        {
            List<Vector3> reversedPoints = new List<Vector3>(tempPoints);
            reversedPoints.Reverse();
            foreach (Vector3 point in reversedPoints) 
            {   Vector3 pointWithOffset = new Vector3(point.x, point.y - 0.75f, point.z);
                tempPoints.Add(pointWithOffset);
            }
        }

        if (tempPoints.Count >= 3) 
        {
            _lightTrail.SetShapePath(tempPoints.ToArray());
        }
    }

    private void OnDestroy()
    {
        if (_lightTrail)
        {
            _lightTrail.GetComponent<LightTrailFadeOut>().beginFade = true;
        }
    }
}
