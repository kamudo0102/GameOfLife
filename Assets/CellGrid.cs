using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Unity.Mathematics;
using System;


public class CellGrid : MonoBehaviour
{
    [Header("UI")]
    public GameObject pauseText;
    public GameObject clickText;
    [SerializeField] TextMeshProUGUI fpsText;

    [Header("CellGrid")]
    public GameObject cellPrefab; 
    Cell[,] cells; 
    float cellSize = 1f; 
    public int size = 10;  
    int aliveNeighbor;
    [SerializeField] int spawnChancePercentage = 15;


    bool pause = false;
    int constantValue = 5;
    
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = constantValue;
        CreateGrid();
    }

    void Update()
    {
        if (!pause)
        {
            UpdateGrid();
            Swapbuffer();
            pauseText.SetActive(true);
            clickText.SetActive(false);
        }
        else
        {
            OnMouseDown();
            pauseText.SetActive(false);
            clickText.SetActive(true);
        }
        fpsText.text = (Application.targetFrameRate + " fps").ToString();
        Application.targetFrameRate = Mathf.Clamp(Application.targetFrameRate, 0, 60);
    }

    public void SpeedUp()
    {
       
        Application.targetFrameRate += constantValue;
        if (Application.targetFrameRate > 0)
        {
            Resume();
        }
    }

    public void SpeedDown()
    {
       
        Application.targetFrameRate -= constantValue;
        if (Application.targetFrameRate == 0)
        {
            Pause();
        }
    }
    public void Restart()
    {
        pause = false;
      
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                
                if (UnityEngine.Random.Range(0, 100) < spawnChancePercentage)
                {
                    cells[x, y].currentGeneration = true;
                }
            }

        }

    }

    public void Pause()
    {
        pause = true;
    }

    public void Resume()
    {
        pause = false;
    }

    public void CreateGrid()
    {
        int numberOfColums = size;
        int numberOfRows = size;
        float cameraOrthographicSize = size * cellSize / 2f;
        Camera.main.orthographicSize = cameraOrthographicSize;
        cells = new Cell[numberOfColums, numberOfRows];
        for (int y = 0; y < numberOfRows; y++)
        {
            for (int x = 0; x < numberOfColums; x++)
            {
                Vector2 newPos = new Vector2(x * cellSize - cameraOrthographicSize + 0.5f, y * cellSize - cameraOrthographicSize + 0.5f);
                var newCell = Instantiate(cellPrefab, newPos, Quaternion.identity);
                newCell.transform.localScale = Vector2.one * cellSize;
                cells[x, y] = newCell.GetComponent<Cell>();
                if (UnityEngine.Random.Range(0, 100) < spawnChancePercentage)
                {
                    cells[x, y].currentGeneration = true;
                }

            }
        }
    }

    public void OnMouseDown()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray raypos = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hitCell;
            hitCell = Physics2D.Raycast(raypos.origin,raypos.direction);
            if(hitCell.collider != null)
            {
                GameObject clickCell = hitCell.collider.gameObject;         
                if (clickCell.GetComponent<Cell>().currentGeneration == true)
                {
                    clickCell.GetComponent<Cell>().currentGeneration = false;
                    clickCell.GetComponent <Cell>().spriteRenderer.color = new Color(0, 0, 0, 0); 
                }
                else
                {
                    clickCell.GetComponent<Cell>().currentGeneration= true;
                    clickCell.GetComponent<Cell>().spriteRenderer.color = new Color(1, 0, 1, 1);
                }
            }
        }
    }
    public void UpdateGrid()
    {
     
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                aliveNeighbor = 0;
                CountAliveNeighbor(x, y);
                bool aliveCurrentGeneration = cells[x, y].currentGeneration;
                bool aliveNextGeneration = (aliveCurrentGeneration && (aliveNeighbor == 2 || aliveNeighbor == 3) ) || (!aliveCurrentGeneration && aliveNeighbor == 3);
                FadeAnimation(x, y);
                cells[x, y].nextGeneration = aliveNextGeneration;
            }

        }

    }
    private void FadeAnimation(int x, int y)
    {
        Color color;
        if (!cells[x, y].currentGeneration && !cells[x, y].nextGeneration)
        {
            color = cells[x, y].spriteRenderer.color;
            color.a *= 0.7f;
            cells[x, y].spriteRenderer.color = color;
        }
        else if (cells[x, y].currentGeneration && cells[x, y].nextGeneration)
        {
            color = cells[x, y].spriteRenderer.color;
            color = new Color(1, 0, 1, 1);
            cells[x, y].spriteRenderer.color = color;
        }  
    }

   

    public int Wrap(int i, int size)
    {
        return (i + size) % size;
    }

    public void Swapbuffer()
    {
        for (int y = 0; y < size; y++)
          for (int x = 0; x < size; x++)
              cells[x, y].currentGeneration = cells[x, y].nextGeneration;
    }

    public void CountAliveNeighbor(int x, int y)
    {
        for (int nx = -1; nx <= 1; nx++)
        {
            for (int ny = -1; ny <= 1; ny++)
            {
                if (nx == 0 && ny == 0) continue;

                int neighborX = Wrap(x + nx, size);
                int neighborY = Wrap(y + ny, size);

                if (cells[neighborX, neighborY].currentGeneration)
                {
                    aliveNeighbor++;
                }
            }
        }

    }


}