using UnityEngine;

public class SpearThrow : MonoBehaviour
{
    public GameObject spearPrefab; 
    public float throwSpeed = 5f; 

    public void ThrowSpear(Vector2 startPosition, Vector2 endPosition)
    {
        GameObject spear = Instantiate(spearPrefab, startPosition, Quaternion.identity); 
        StartCoroutine(MoveSpear(spear, endPosition));
    }

    private System.Collections.IEnumerator MoveSpear(GameObject spear, Vector2 targetPosition)
    {
        while ((Vector2)spear.transform.position != targetPosition)
        {
            spear.transform.position = Vector2.MoveTowards(spear.transform.position, targetPosition, throwSpeed * Time.deltaTime);
            yield return null; 
        }
        Destroy(spear);
    }
}
