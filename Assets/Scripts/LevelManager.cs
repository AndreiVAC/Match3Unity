using System.Collections.Generic;
using UnityEngine;
using Container;
namespace Level
{
    [System.Serializable]
    public class Manager : MonoBehaviour
    {
        public Camera mainCamera;
        [SerializeField]
        private TMPro.TextMeshProUGUI difficultyText;
        public LevelResources levelResources;
        public int levelIndex = 0;
        public Stage currentStage;
        public static Manager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            mainCamera = Camera.main;
        }

        private void Start()
        {
            LoadCurrentStage();
        }

        public void LoadCurrentStage()
        {
            string levelPath = levelResources.jsonResourcePaths[levelIndex];
            LoadStage(levelPath);
        }
        public void LoadNextStage()
        {
            levelIndex++;
            if (levelIndex >= levelResources.jsonResourcePaths.Count)
            {
                Debug.Log("End of levels! Congratulations!");
                Timer.StageTimer.Instance.Toggle();
                return;
            }
            LoadCurrentStage();
        }
        private void LoadStage(string stagePath)
        {
            currentStage = StageParser.ParseStage(stagePath);
            foreach (GameObject shelf in GameObject.FindGameObjectsWithTag("Shelf"))
            {
                Destroy(shelf);
            }
            foreach (GameObject sortableObject in GameObject.FindGameObjectsWithTag("SortableObject"))
            {
                Destroy(sortableObject);
            }
            foreach (GameObject slot in GameObject.FindGameObjectsWithTag("Slot"))
            {
                Destroy(slot);
            }
            Timer.StageTimer.Instance.Setup(currentStage.duration);
            SetDifficulty(currentStage.difficulty);
            SpawnCurrentStageShelves();
        }
        [SerializeField]
        private Container.Shelf shelfAsset;
        [SerializeField]
        private Container.Slot slotAsset;
        [SerializeField]
        [Tooltip("Items will take ids strating from 0. ID -1 is used for empty space.")]
        public List<SortableObject> sortableTypes = new List<SortableObject>();
        private void SpawnCurrentStageShelves()
        {
            foreach (Shelf stageShelf in currentStage.shelves)
            {
                // spawn a new container
                GameObject newContainer = new GameObject("Container");
                Vector3 spawnPos = StringToVector3(stageShelf.offsetPosition);
                newContainer.transform.position = spawnPos;

                // spawn and size a new shelf inside container
                Container.Shelf newShelf = Instantiate(shelfAsset, spawnPos, Quaternion.identity);
                newShelf.transform.SetParent(newContainer.transform);
                Vector3 shelfScale = new Vector3(stageShelf.width, stageShelf.height, 0);
                newShelf.transform.localScale = shelfScale;

                // spawn slots based on the shelf's size
                int totalSlots = stageShelf.width * stageShelf.height;
                List<float> xPositionOffsets = CalculatePositionOffsets(stageShelf.width);
                List<float> yPositionOffsets = CalculatePositionOffsets(stageShelf.height);
                int slotIndex = 0;
                Layer stageShelfFirstLayer = stageShelf.layers[0];
                foreach (float xPositionOffset in xPositionOffsets)
                {
                    foreach (float yPositionOffset in yPositionOffsets)
                    {
                        Vector3 spawnPositionOffset = new Vector3(xPositionOffset, yPositionOffset, -1);
                        spawnPos = spawnPositionOffset + newShelf.transform.position;
                        Container.Slot newSlot = Instantiate(slotAsset, spawnPos, Quaternion.identity);
                        newSlot.transform.SetParent(newContainer.transform);

                        // set types for first layer
                        int spawnType;
                        if (stageShelfFirstLayer.spawns.Count <= slotIndex)
                        {
                            Debug.LogError("JSON Not all slots defined.");
                            spawnType = -1; // if JSON doesn't define this slot, leave it empty.
                        }
                        else
                        {
                            spawnType = stageShelfFirstLayer.spawns[slotIndex];
                        }

                        var maxSortableTypesIndex = sortableTypes.Count;
                        if (spawnType > maxSortableTypesIndex)
                        {
                            Debug.LogError($"spawnType index out of range in JSON. Accepted index 0-{maxSortableTypesIndex.ToString()}.");
                            return;
                        }
                        newSlot.SpawnType = spawnType;
                        newShelf.slots.Add(newSlot);
                        slotIndex++;
                    }
                }

                // set types for other layers
                foreach (Layer dataLayer in stageShelf.layers)
                {
                    List<int> spawnList = new List<int>();
                    foreach (int spawn in dataLayer.spawns)
                    {
                        spawnList.Add(spawn);
                    }
                    newShelf.slotSpawns.Add(spawnList);
                }
                newShelf.UpdateShelf();
            }
        }


        public List<float> CalculatePositionOffsets(int size)
        {
            List<float> offsets = new List<float>();
            if (size < 1)
            {
                Debug.LogError("Width or height of JSON shelf must be at least 1");
                return offsets;
            }
            float startOffset = -((size - 1) / 2.0f);

            for (int i = 0; i < size; i++)
            {
                offsets.Add(startOffset + i);
            }

            return offsets;
        }

        private Vector3 StringToVector3(string input)
        {
            string[] values = input.Split(',');
            if (values.Length == 3)
            {
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);
                return new Vector3(x, y, z);
            }
            else
            {
                Debug.LogError("Invalid position format in stage JSON. Expected format: 'x,y,z'");
                return Vector3.zero;
            }
        }

        private void SetDifficulty(int newDifficulty)
        {
            switch (newDifficulty)
            {
                case 0:
                    difficultyText.text = "Normal";
                    break;
                case 1:
                    difficultyText.text = "Hard";
                    break;
                case 2:
                    difficultyText.text = "SuperHard";
                    break;
                default:
                    Debug.LogError("JSON ERROR: Difficulty must be between 0-2.");
                    difficultyText.text = "Undefined Difficulty";
                    break;
            }
        }

        public void updateGameStateNextFrame()
        {
            Invoke("updateGameState", 0f);
        }
        private void updateGameState()
        {
            if (FindAnyObjectByType<SortableObject>() == null)
            {
                Debug.Log($"YOU WIN! Time scored: {Timer.StageTimer.Instance.GetTime()}. Loading next stage...");
                LoadNextStage();
            }
        }
    }

    [System.Serializable]
    public class Stage
    {
        public int duration;
        public int difficulty;
        public List<Shelf> shelves = new List<Shelf>();
    }

    [System.Serializable]
    public class Shelf
    {
        public string offsetPosition;
        public int width;
        public int height;
        public List<Layer> layers = new List<Layer>();
    }

    [System.Serializable]
    public class Layer
    {
        public List<int> spawns = new List<int>(); // -1 = no item
    }

    public static class StageParser
    {
        public static Stage ParseStage(string stagePath)
        {
            string jsonString = LoadJSON(stagePath);
            var stage = JsonUtility.FromJson<Stage>(jsonString);
            return stage;
        }
        private static string LoadJSON(string filePath)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(filePath);
            if (jsonFile != null)
            {
                return jsonFile.text;
            }
            else
            {
                Debug.LogError($"Failed to find JSON file at path: {filePath}");
                return null;
            }
        }
    }
}