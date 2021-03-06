using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public int width = 6;
    public int height = 6;

    [SerializeField]
    CreateRiver createRiver;
    public GridCell cellPrefab;

    GridCell[] cells;

    private void Awake()
    {

        //Store all cells in an array to be accessed later
        cells = new GridCell[height * width];
        for (int x = 0, i = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(startCreatingRivers());
    }

    void CreateCell(int x, int z, int i)
    {
        //Set world position of cell
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (GridMetrics.edgeRadius * 2f);
        position.y = 0f;
        position.z = z * (GridMetrics.cornerRadius * 1.5f);

        GridCell cell = cells[i] = Instantiate<GridCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        //Set coordinates of cells. Shift to keep both x and z cooridnates on straight lines.
        cell.GetComponent<GridCell>().coordinates = new Vector3(x - Mathf.FloorToInt(z / 2f),-(x - Mathf.FloorToInt(z / 2f)) - z ,z);
    }

    //Create rivers after rest of grid is made.
    IEnumerator startCreatingRivers()
    {
        yield return new WaitForSeconds(0.1f);
        createRiver.CreateNewRiver();

    }
}