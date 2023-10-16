using UnityEngine;
using System.Collections.Generic;  // List를 사용하기 위해 추가

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnableObject
    {
        public GameObject prefab;
        [Range(0f, 1f)]
        public float spawnChance;
    }

    public List<SpawnableObject> objects = new List<SpawnableObject>();

    public float minSpawnRate = 1f;
    public float maxSpawnRate = 2f;

    private void OnEnable()
    {
        Invoke(nameof(Spawn), Random.Range(minSpawnRate, maxSpawnRate));
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Spawn()
    {
        float spawnChance = Random.value;

        foreach (var obj in objects)
        {
            if (spawnChance < obj.spawnChance)
            {
                GameObject obstacle = Instantiate(obj.prefab);
                obstacle.transform.position += transform.position;
                break;
            }

            spawnChance -= obj.spawnChance;
        }

        Invoke(nameof(Spawn), Random.Range(minSpawnRate, maxSpawnRate));
    }

    public void UpdateTree2Sprite(Texture2D newTexture)
    {
        // 첫 번째 오브젝트의 스프라이트를 업데이트하려면 아래의 코드를 사용하세요.
        if (objects.Count > 0)
        {
            SpriteRenderer sr = objects[0].prefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                sr.sprite = newSprite;
            }
        }
    }

    public void AddNewObject(GameObject newPrefab, float newSpawnChance)
    {
        SpawnableObject newObj = new SpawnableObject
        {
            prefab = newPrefab,
            spawnChance = newSpawnChance
        };

        objects.Add(newObj);
    }
    public void CreateAndAddNewObject(Texture2D newTexture, float newSpawnChance)
    {
        // 새로운 게임 오브젝트와 스프라이트 렌더러 생성
        GameObject newObject = new GameObject("DynamicSpawnedObject");
        SpriteRenderer sr = newObject.AddComponent<SpriteRenderer>();
        
        Sprite newSprite = Sprite.Create(newTexture, new Rect(0, 0, newTexture.width, newTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        sr.sprite = newSprite;
        
        // 스폰 가능한 오브젝트 리스트에 추가
        AddNewObject(newObject, newSpawnChance);
    }

}
