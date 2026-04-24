using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineralController : MonoBehaviour
{
    public GameObject chunkPrefab;
    public int maxChunks = 3;
    private int chunksBroken = 0;

    public void BreakOffChunk(Vector3 hitPoint)
    {
        if (chunksBroken >= maxChunks) return;

        GameObject chunk = Instantiate(chunkPrefab, hitPoint, Quaternion.identity);
        Rigidbody rb = chunk.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce((Vector3.up + Vector3.forward) * Random.Range(2f, 4f), ForceMode.Impulse);
        }

        chunksBroken++;
        // Можно добавить визуальное повреждение
    }

}