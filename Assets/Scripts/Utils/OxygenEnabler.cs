using UnityEngine;

public class OxygenEnabler : MonoBehaviour
{
    // Start is called before the first frame update
    private TimerManager _tm;
    [SerializeField] private bool oxygenShouldDeplete;
    void Start()
    {
        var timerObject = GameObject.Find("GameManagerFinal");
        if (timerObject != null)
        {
            _tm = timerObject.GetComponent<TimerManager>();
        }
        else
        {
            Debug.LogError("GameManagerFinal not found");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _tm.oxygenCanDeplete = oxygenShouldDeplete;
        }
    }
}
