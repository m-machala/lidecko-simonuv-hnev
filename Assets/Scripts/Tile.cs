using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Color originalColor;
    [Range(0, 1)] public float colorChangeStrength = 0.5f;
    private GroundManager groundManager;

    public void setGroundManager(GroundManager groundManager) {
        this.groundManager = groundManager;
    }

    void Start()
    {
        originalColor = GetRenderer().material.color;
    }

    Renderer GetRenderer() {
        return GetComponent<Renderer>();
    }

    public void TintTile(Color color) {
        Renderer renderer = GetRenderer();
        Color newColor = Color.Lerp(renderer.material.color, color, colorChangeStrength);
        renderer.material.color = newColor;
    }

    public void UntintTile() {
        GetRenderer().material.color = originalColor;
    }

    void OnMouseDown() {
        groundManager.TileClicked(new Vector2((int)transform.position.x, (int)transform.position.z));
    }
}
