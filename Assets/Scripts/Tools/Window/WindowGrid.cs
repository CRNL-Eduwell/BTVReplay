using UnityEngine;

/// <summary>
/// Define a grid of Row * Column
/// </summary>
public class WindowGrid
{
    public Rect[] Cells { get; set; } = null;
    public Rect[] PrevisousSizeCells { get; set; } = null;
    public Vector2 MinSize { get; set; } = new Vector2(0, 0);
    public Vector2 MaxSize { get; set; } = new Vector2(0, 0);
    public int RowCount { get; private set; } = -1;
    public int ColumnCount { get; private set; } = -1;

    private RectTransform m_RectTransform = null;

    public WindowGrid(int columnCount, int rowCount, RectTransform rectTransform)
    {
        RowCount = rowCount;
        ColumnCount = columnCount;
        Cells = new Rect[RowCount * ColumnCount];
        PrevisousSizeCells = new Rect[RowCount * ColumnCount];
        m_RectTransform = rectTransform;
    }

    public void DefineGrid()
    {
        BtvLog.Log("WindowGrid => DefineGrid from Rect Size");
        for (int i = 0; i < RowCount; i++)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                int index = j + (i * ColumnCount);

                PrevisousSizeCells[index] = new Rect(Cells[index]);
                
                float width = m_RectTransform.rect.width / ColumnCount;
                float height = m_RectTransform.rect.height / RowCount;
                float x = m_RectTransform.rect.x + (0.5f * width) + (j * width);
                float y = m_RectTransform.rect.y + (0.5f * height) + (i * height);
                Cells[index] = new Rect(x, y, width, height);
            }
        }

        MinSize = new Vector2(m_RectTransform.rect.width / 4, m_RectTransform.rect.height / (RowCount * 2));
        MaxSize = new Vector2(m_RectTransform.rect.width, m_RectTransform.rect.height);
    }
}