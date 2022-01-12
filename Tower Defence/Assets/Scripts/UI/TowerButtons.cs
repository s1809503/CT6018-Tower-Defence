using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class TowerButtons : MonoBehaviour
{
    [SerializeField]
    string towerName;

    [SerializeField]
    string[] spawnerNames;


    public Toggle towerToggle;
    public Toggle[] otherTowersToggle;
    private Image toggleBackground;
    GameObject[] gos;
    public Camera cam;
    public GameObject Tower;
    public GameObject mainBase;
    public int maxPathLength = 500;
    List<GameObject> Spawners = new List<GameObject>();

    public double cost;
    public int numTowers = 0;
    [SerializeField]
    double baseValue;
    [SerializeField]
    float exponenntialRatio = 1.25f;

    [SerializeField]
    GameObject score;

    [SerializeField]
    Text costText;

    [SerializeField]
    List<UpgradeButton> upgradeButtons;

    [SerializeField]
    List<int> upgradeButtonValues;

    //Upgrade Buttons for the towers, and number of towers of that type needed to become available
    Dictionary<UpgradeButton, int> upgradeButtonsPair;


    GameObject currentHit;
    GameObject previousHit;
    bool isPlacable = true;

    // Start is called before the first frame update
    void Start()
    {
        upgradeButtonsPair = new Dictionary<UpgradeButton, int>();
        int i = 0;
        foreach(UpgradeButton button in upgradeButtons)
        {
            upgradeButtonsPair.Add(button, upgradeButtonValues[i]);
            i++;
        }

        //Make upgrade buttons available if requirements for number of towers is met.
        foreach (KeyValuePair<UpgradeButton, int> button in upgradeButtonsPair)
        {
            if (button.Value <= numTowers)
            {
                button.Key.isAvailable = true;
            }
        }

        gos = GameObject.FindGameObjectsWithTag("Cell");
        towerToggle.onValueChanged.AddListener(OnToggleValueChanged);
        toggleBackground = towerToggle.GetComponentInChildren<Image>();
    }

    private void OnToggleValueChanged(bool isOn)
    {
        ColorBlock cb = towerToggle.colors;
        if (isOn == true)
        {
            toggleBackground.color = Color.blue;
            foreach(Toggle tog in otherTowersToggle)
            {
                tog.isOn = false;
            }
        }
        else
        {
            toggleBackground.color = new Color32(44, 44, 44, 255)/*Color.white*/;
        }
    }

    private void Update()
    {
        cost = System.Math.Floor(baseValue * System.Math.Pow(exponenntialRatio, numTowers));
        if (numTowers == 0 && towerName == "Turret")
        {
            cost = 0;
        }
        double exponent = (System.Math.Floor(System.Math.Log10(System.Math.Abs(cost))));
        double mantissa = (cost / System.Math.Pow(10, exponent));

        if (cost >= 1000000)
        {
            costText.text = mantissa.ToString("F3") + "e" + exponent.ToString();
        }
        else
        {
            costText.text = cost.ToString();
        }

        if (towerToggle.isOn)
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;

                

                if(objectHit.gameObject.tag == towerName && Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
                {
                    objectHit.parent.GetComponent<GridCell>().notSelectable = false;
                    objectHit.parent.GetComponent<GridCell>().isObstacle = false;
                    objectHit.parent = null;
                    Destroy(objectHit.gameObject);
                    numTowers -= 1;
                    if (!(numTowers == 0 && towerName == "Turret"))
                    {
                        score.GetComponent<Score>().score +=  System.Math.Floor(baseValue * System.Math.Pow(exponenntialRatio, numTowers) / 5f);
                    }
                    GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
                    foreach (GameObject monster in monsters)
                    {
                        monster.GetComponent<MonsterController>().needToPathfind = true;
                    }
                    return;
                }

                if (objectHit.gameObject.tag == towerName && Input.GetKeyDown(KeyCode.R) && !EventSystem.current.IsPointerOverGameObject())
                {
                    objectHit.transform.localRotation = Quaternion.Euler(objectHit.transform.localRotation.eulerAngles.x, objectHit.transform.localRotation.eulerAngles.y + 60, objectHit.transform.localRotation.eulerAngles.z);
                }

                foreach (GameObject go in gos)
                {
                    if (GameObject.ReferenceEquals(objectHit.gameObject, go) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        currentHit = go;
                        go.GetComponent<GridCell>().isSelector = true;

                        if(!go.GetComponent<GridCell>().isObstacle)
                        {
                            go.GetComponent<GridCell>().isObstacle = true;
                            GameObject endPoint = mainBase.GetComponent<MainBase>().closest;
                            if(currentHit != previousHit)
                            {
                                isPlacable = true;
                                Spawners.Clear();
                                foreach (string name in spawnerNames)
                                {
                                    List<GameObject> spawners = new List<GameObject>();
                                    spawners = GameObject.FindGameObjectsWithTag(name).ToList();
                                    foreach(GameObject spawn in spawners)
                                    {
                                        Spawners.Add(spawn);
                                    }
                                }
                                //Spawners = GameObject.FindGameObjectsWithTag("Spawner").ToList();

                                foreach (GameObject spawner in Spawners)
                                {
                                    List<GameObject> Path = new List<GameObject>();
                                    Path = gameObject.GetComponent<Pathfind>().FindPath(spawner.transform.parent.gameObject, endPoint);
                                    if (Path.Count == 0)
                                    {
                                        isPlacable = false;
                                    }
                                }
                            }
                            if (!go.GetComponent<GridCell>().notSelectable && isPlacable)
                            {
                                if (Input.GetMouseButtonDown(0) && score.GetComponent<Score>().score >= cost)
                                {
                                    GameObject towerInstant = Instantiate(Tower);
                                    towerInstant.transform.localScale = new Vector3(1,1,1);
                                    towerInstant.transform.parent = go.transform;
                                    towerInstant.transform.localPosition = Tower.transform.position;
                                    towerInstant.transform.localRotation = Tower.transform.rotation;
                                    go.GetComponent<GridCell>().notSelectable = true;
                                    score.GetComponent<Score>().score -= cost;
                                    numTowers += 1;
                                    GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
                                    foreach (GameObject monster in monsters)
                                    {
                                        monster.GetComponent<MonsterController>().needToPathfind = true;
                                    }

                                    //Make upgrade buttons available if requirements for number of towers is met.
                                    foreach(KeyValuePair<UpgradeButton, int> button in upgradeButtonsPair)
                                    {
                                        if(button.Value <= numTowers)
                                        {
                                            button.Key.isAvailable = true;
                                        }
                                    }
                                }
                                else
                                {
                                    go.GetComponent<GridCell>().isObstacle = false;
                                }
                            }
                            else
                            {
                                go.GetComponent<GridCell>().isObstacle = false;
                                go.GetComponent<GridCell>().selector.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                            }
                        }
                        previousHit = currentHit;
                    }
                    else
                    {
                        go.GetComponent<GridCell>().isSelector = false;                      
                    }
                }
            }            
        }
        else
        {
            toggleBackground.color = new Color32(44,44,44,255)/*Color.white*/;
            currentHit = null;
            previousHit = null;
        }
    }
}