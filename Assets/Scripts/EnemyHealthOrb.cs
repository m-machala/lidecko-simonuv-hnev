using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthOrb : MonoBehaviour
{
    private GameObject healthOrb;

    void Start()
    {
        GameObject prefab = Resources.Load<GameObject>("Enemy Health");  
        healthOrb = Instantiate(prefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
        healthOrb.transform.SetParent(transform);

        UpdateColor();
    }

    public void UpdateColor()
    {
        var skills = GetComponent<Skills>();
        Renderer renderer = healthOrb.GetComponent<Renderer>();
        Color newColor = Color.Lerp(Color.red, Color.green, (float)skills.health / (float)skills.MaxHealth);
        renderer.material.color = newColor;
    }
}
