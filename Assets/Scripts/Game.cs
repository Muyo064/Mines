using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public enum GameMode
    {
        Easy,
        Normal,
        Hard,
    }
    public GameMode gameMode = GameMode.Normal;


    public int width;
    public int height;
    public int mineCount;

    private Board board;
    private Cell[,] cells;
    private bool gameover;
    public int cameraPadding = 1;
    private void Awake()
    {
        Application.targetFrameRate = 60;

        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();
    }

    private void NewGame()
    {
        gameover = false;

        switch (gameMode)
        {
            case GameMode.Easy:
                width = 9;
                height = 9;
                mineCount = 10;
                break;

            case GameMode.Normal:
                width = 16;
                height = 16;
                mineCount = 32;
                break;

            case (GameMode.Hard):
                width = 30;
                height = 16;
                mineCount = 99;
                break;
        }

        GenerateCells();
        GenerateMines();
        GenerateNumbers();


        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        Camera.main.orthographicSize = Mathf.Max(width, height) / 2f + cameraPadding;
        board.Draw(cells);
    }

    private void GenerateCells()
    {

        cells = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                { cell.Position = new Vector3Int(x, y, 0);
                    cell.type = Cell.Type.Empty;
                }
                cells[x, y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for (int i = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            while (cells[x, y].type == Cell.Type.Mine)
            {
                x++;

                if (x >= width)
                {
                    x = 0;
                    y++;

                    if (y >= height)
                    {
                        y = 0;
                    }
                }
            }

            cells[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = cells[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                cell.number = CountMines(x, y);

                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                cells[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (IsValid(x, y) && GetCell(x, y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
        }
        else if (!gameover)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Flag();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Reveal();
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            SceneManager.LoadScene("Start");
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void Flag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        cell.flagged = !cell.flagged;
        cells[cellPosition.x, cellPosition.y] = cell;
        board.Draw(cells);
    }

    private void Reveal()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);
        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;

            case Cell.Type.Empty:
                StartCoroutine(Flood(cell));
                CheckWinCondition();
                break;

            default:
                cell.revealed = true;
                cells[cellPosition.x, cellPosition.y] = cell;
                CheckWinCondition();
                break;
        }

        board.Draw(cells);
    }

    private IEnumerator Flood(Cell cell)
    {
        if (cell.revealed) 
            yield break;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) 
            yield break;

        cell.revealed = true;
        cells[cell.Position.x, cell.Position.y] = cell;

        board.Draw(cells);
        yield return new WaitForEndOfFrame();

        if (cell.type == Cell.Type.Empty)
        {
            StartCoroutine(Flood(GetCell(cell.Position.x - 1, cell.Position.y)));
            StartCoroutine(Flood(GetCell(cell.Position.x + 1, cell.Position.y)));
            StartCoroutine(Flood(GetCell(cell.Position.x, cell.Position.y - 1)));
            StartCoroutine(Flood(GetCell(cell.Position.x, cell.Position.y + 1)));

            StartCoroutine(Flood(GetCell(cell.Position.x - 1, cell.Position.y - 1)));
            StartCoroutine(Flood(GetCell(cell.Position.x - 1, cell.Position.y + 1)));
            StartCoroutine(Flood(GetCell(cell.Position.x + 1, cell.Position.y - 1)));
            StartCoroutine(Flood(GetCell(cell.Position.x + 1, cell.Position.y + 1)));
        }
    }

    private void Explode(Cell cell)
    {
        Debug.Log("Game Over!");
        gameover = true;

        // Set the mine as exploded
        cell.exploded = true;
        cell.revealed = true;
        cells[cell.Position.x, cell.Position.y] = cell;

        // Reveal all other mines
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = cells[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    cells[x, y] = cell;
                }
            }
        }
    }

    private void CheckWinCondition()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = cells[x, y];

                if (cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        Debug.Log("Winner!");
        gameover = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = cells[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    cells[x, y] = cell;
                }
            }
        }
    }

    private Cell GetCell(int x, int y)
    {
        if (IsValid(x, y))
        {
            return cells[x, y];
        }
        else
        {
            return new Cell();
        }
    }

    private bool IsValid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

}
