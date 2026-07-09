using UnityEngine;

public class BMOHidingScript : MonoBehaviour
{
    [SerializeField][Range(0f, 100f)] private float successChance = 35f; // Chance that BMO is allowed to exist

    private void Start()
    {
        if (ShouldForceFail())
        {
            Destroy(gameObject);
        }
        else
        {
            base.transform.position = new Vector3(75f, -36.5f, 200f);
        }
    }

    private bool ShouldForceFail()
    {

        float roll = Random.Range(0f, 100f);


        return roll > successChance;
    }
}