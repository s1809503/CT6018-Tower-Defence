using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Pathfind : MonoBehaviour
{
    GameObject currentCell;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

   

    public List<GameObject> FindPath(GameObject startPoint, GameObject endPoint)
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Cell");
        foreach (GameObject go in gos)
        {
            go.GetComponent<GridCell>().isSelector = false;
        }

        List<GameObject> openPathCells = new List<GameObject>();
        List<GameObject> closedPathCells = new List<GameObject>();

        currentCell = startPoint;

        // Add the start tile to the open list
        openPathCells.Add(currentCell);

        currentCell.GetComponent<GridCell>().g = 0;
        currentCell.GetComponent<GridCell>().h = GetEstimatedPathCost(currentCell.transform.position, endPoint.transform.position);


        while (openPathCells.Count != 0)
        {
            //sort open list
            openPathCells = openPathCells.OrderBy(x => x.GetComponent<GridCell>().f).ThenByDescending(x => x.GetComponent<GridCell>().g).ToList();
            currentCell = openPathCells[0];

            //Remove current cell from open list and add to closed list
            openPathCells.Remove(currentCell);
            closedPathCells.Add(currentCell);

            int g = currentCell.GetComponent<GridCell>().g + 1;

            //Check if endpoint is in closed list
            if (closedPathCells.Contains(endPoint))
            {
                //We have found a path
                break;
            }

            currentCell.GetComponent<GridCell>().adjacentCells = currentCell.GetComponent<GridCell>().FindAdjacentCells();

            foreach (GameObject adjacentCell in currentCell.GetComponent<GridCell>().adjacentCells)
            {
                //Ignore unwalkable cell
                if(adjacentCell.GetComponent<GridCell>().isObstacle)
                {
                    continue;
                }
                //Ifnore if already in closed list
                if(closedPathCells.Contains(adjacentCell))
                {
                    continue;
                }
                // If not in open list add and compute g and h
                if(!(openPathCells.Contains(adjacentCell)))
                {
                    adjacentCell.GetComponent<GridCell>().g = g;
                    adjacentCell.GetComponent<GridCell>().h = GetEstimatedPathCost(adjacentCell.transform.position, endPoint.transform.position);
                    openPathCells.Add(adjacentCell);
                    adjacentCell.GetComponent<GridCell>().previousCell = currentCell;
                }
                //Otherwise check if F value can be lwoered with current G
                else if (adjacentCell.GetComponent<GridCell>().f > g + adjacentCell.GetComponent<GridCell>().h)
                {
                    adjacentCell.GetComponent<GridCell>().g = g;
                    adjacentCell.GetComponent<GridCell>().previousCell = currentCell;
                }
            }
        }

        List<GameObject> finalPathCells = new List<GameObject>();

        if(closedPathCells.Contains(endPoint))
        {
            currentCell = endPoint;
            finalPathCells.Add(currentCell);

            for (int i = endPoint.GetComponent<GridCell>().g - 1; i >= 0; i--)
            {
                foreach(GameObject cell in closedPathCells)
                {
                    if(cell.GetComponent<GridCell>().g == i && cell.GetComponent<GridCell>().adjacentCells.Contains(currentCell))
                    {
                        currentCell = cell;
                        currentCell.GetComponent<GridCell>().isSelector = true;
                        finalPathCells.Add(currentCell);
                    }
                }

                //currentCell = closedPathCells.Find(x => x.GetComponent<GridCell>().g == i && GameObject.ReferenceEquals(currentCell.GetComponent<GridCell>().previousCell, currentCell));
                //finalPathCells.Add(currentCell);
                //currentCell.GetComponent<GridCell>().isSelector = true;
            }
            finalPathCells.Reverse();
        }
        return finalPathCells;
    }

    static int GetEstimatedPathCost(Vector3 currentPosition, Vector3 endPosition)
    {
        Vector3 diff = currentPosition - endPosition;
        float curDistance = diff.sqrMagnitude;
        return Mathf.FloorToInt(curDistance);
    }
}