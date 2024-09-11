using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using System.Text.RegularExpressions;
namespace Game
{
    [System.Serializable]
    public class Manager : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI difficultyText;
        public Stage currentStage;
        public static Manager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            LoadStage(1);
        }

        public void LoadStage(int stageNumber)
        {
            currentStage = StageParser.ParseStage(stageNumber);
            foreach (Container.Shelf shelf in FindObjectsOfType<Container.Shelf>())
            {
                Destroy(shelf.gameObject);
            }
            foreach (SortableObject sortableObject in FindObjectsOfType<SortableObject>())
            {
                Destroy(sortableObject.gameObject);
            }
            Timer.StageTimer.Instance.Setup(currentStage.duration);
            SetDifficulty(currentStage.difficulty);
            SpawnCurrentStageShelves();
        }
        [SerializeField]
        private Container.Shelf shelfAsset;
        [SerializeField]
        private List<SortableObject> sortableTypes;
        private void SpawnCurrentStageShelves()
        {
            foreach(Shelf dataShelf in currentStage.shelves)
            {
                Vector3 spawnPos = StringToVector3(dataShelf.offsetPosition);
                var newShelf = Instantiate(shelfAsset, spawnPos, Quaternion.identity);
                Vector3 shelfScale = new Vector3(dataShelf.width, dataShelf.height, 0);
                newShelf.transform.localScale = shelfScale;

                foreach(Layer dataLayer in dataShelf.layers)
                {
                    var newSlotLayer = new Container.SlotLayer(dataLayer.slots.Count);
                    int index = 0;
                    foreach(Slot dataSlot in dataLayer.slots)
                    {
                        Container.Slot newSlot = new Container.Slot();
                        newSlot.offsetPosition = StringToVector3(dataSlot.offsetPosition);
                        var spawnType = dataSlot.spawnType;
                        newSlot.setType(spawnType);
                        var maxSortableTypesIndex = sortableTypes.Count;
                        if(spawnType > maxSortableTypesIndex)
                        {
                            Debug.LogError($"spawnType index out of range in JSON. Accepted index 0-{maxSortableTypesIndex.ToString()}.");
                            return;
                        }
                        if (spawnType == -1)
                        {
                            newSlot.spawnObject = null;
                        }
                        else
                        {
                        newSlot.spawnObject = sortableTypes[spawnType];
                        }
                        newSlotLayer.slots[index] = newSlot;
                        index++;
                    }
                    newShelf.slotLayers.Add(newSlotLayer);
                }
            } 
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
                    difficultyText.text = "Easy";
                    break;
                case 1:
                    difficultyText.text = "Medium";
                    break;
                case 2:
                    difficultyText.text = "Hard";
                    break;
                default:
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
                Debug.Log("You win!");
            }
        }
        void Update()
        {
        }
    }

    [System.Serializable]
    public class Stage
    {
        public int duration;
        public int difficulty;
        public List<Shelf> shelves;
    }

    [System.Serializable]
    public class Shelf
    {
        public string offsetPosition;
        public int width;
        public int height;
        public List<Layer> layers;
    }

    [System.Serializable]
    public class Layer
    {
        public List<Slot> slots;
    }

    [System.Serializable]
    public class Slot
    {
        public string offsetPosition;
        public int spawnType;
    }

    public static class StageParser
    {
        public static Stage ParseStage(int stageNumber)
        {
            string paddedStageNumber = stageNumber.ToString().PadLeft(2, '0');
            string stagePath = Path.Combine(Application.streamingAssetsPath, $"JSONLevels/Stage{paddedStageNumber}.json");
            string jsonString = LoadJSON(stagePath);
            var stage = JsonUtility.FromJson<Stage>(jsonString);
            return stage;
        }
        private static string LoadJSON(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            else
            {
                Debug.LogError($"Failed to find JSON file at path: {filePath}");
                return null;
            }
        }
    }
}