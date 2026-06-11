using UnityEngine;
using UnityEngine.EventSystems;
using System;

//Note : Need to have an image or raw image component on the same level, otherwise capture of window doesn't work
public class WindowLayout : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private RectTransform m_rectTransform = null;

    private WindowGrid TwoBy3 = null;
    private WindowGrid OneBy3 = null;
    private WindowGrid TwoBy2 = null;
    private bool m_Loaded = false;

    private void Awake()
    {
        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        TwoBy3 = new WindowGrid(2, 3, m_rectTransform);
        OneBy3 = new WindowGrid(1, 3, m_rectTransform);
        TwoBy2 = new WindowGrid(2, 2, m_rectTransform);
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (TwoBy3 == null || OneBy3 == null || TwoBy2 == null) return;

        TwoBy3.DefineGrid();
        OneBy3.DefineGrid();
        TwoBy2.DefineGrid();

        Resize(!m_Loaded);
    }

    private void OnLoaderMessage(LoaderMessage message)
    {
        if (message.Task == LoaderMessage.LoaderTask.MediaLoader)
        {
            m_Loaded = true;
        }
    }

    private void Resize(bool defaultPosition = false)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject currentChildObject = gameObject.transform.GetChild(i).gameObject;
            RectTransform r = currentChildObject.GetComponent<RectTransform>();
            Window w = currentChildObject.GetComponent<Window>();

            if (r != null && w != null)
            {
                Rect cellRect = defaultPosition ? TwoBy2.Cells[w.windowId] : GetCellSize(r, w.windowId);
                if (!float.IsNaN(cellRect.width) && !float.IsNaN(cellRect.height) && !float.IsNaN(cellRect.x))
                {
                    r.sizeDelta = new Vector2(cellRect.width, cellRect.height);
                    r.localPosition = new Vector3(cellRect.x, cellRect.y, r.localPosition.z);
                }
            }
        }
    }

    private Rect GetCellSize(RectTransform r, int windowIndex)
    {
        if (r.sizeDelta.x > TwoBy3.PrevisousSizeCells[windowIndex].width && r.sizeDelta.y <= TwoBy3.PrevisousSizeCells[windowIndex].height)
        {
            //Debug.Log("Look at 1 by 3");
            return OneBy3.Cells[windowIndex];
        }
        else if (r.sizeDelta.x <= TwoBy3.PrevisousSizeCells[windowIndex].width && r.sizeDelta.y <= TwoBy3.PrevisousSizeCells[windowIndex].height)
        {
            //Debug.Log("Look at 2 by 3");
            return TwoBy3.Cells[windowIndex];
        }
        else
        {
            //Debug.Log("Look at 2 by 2");
            return TwoBy2.Cells[windowIndex];
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        bool nothingDone = true;
        if (Window.itemBeingDragged != null)
        {
            Window.itemBeingDragged.transform.SetParent(transform);
            RectTransform currentRectTransform = Window.itemBeingDragged.GetComponent<RectTransform>();
            Window currentWindowManager = Window.itemBeingDragged.GetComponent<Window>();

            WindowGrid grid = GetItemGridFromSize(currentRectTransform.rect, currentWindowManager.windowId);
            for (int i = 0; i < grid.Cells.Length; i++)
            {
                float x = currentRectTransform.localPosition.x + 0.5f * grid.Cells[i].width;
                float y = currentRectTransform.localPosition.y + 0.5f * grid.Cells[i].height;
                Vector3 itemPosition = new Vector3(x, y);
                if (grid.Cells[i].Contains(itemPosition))
                {
                    currentRectTransform.localPosition = new Vector3(grid.Cells[i].x, grid.Cells[i].y, currentRectTransform.localPosition.z);
                    currentRectTransform.sizeDelta = new Vector2(grid.Cells[i].width, grid.Cells[i].height);
                    currentWindowManager.minSizeWindow = grid.MinSize;
                    currentWindowManager.maxSizeWindow = grid.MaxSize;
                    currentWindowManager.windowId = i;
                    currentWindowManager.GridLayout = GetLayoutFromGrid(grid);
                    nothingDone = false;
                    break;
                }
            }

            if (nothingDone) //Put window back at previous location
            {
                Window.itemBeingDragged.transform.SetParent(currentWindowManager.initialParent);
                currentRectTransform.localPosition = new Vector3(currentWindowManager.initialPosition.x, currentWindowManager.initialPosition.y);
                currentRectTransform.sizeDelta = new Vector2(currentWindowManager.initialSizeDelta.x, currentWindowManager.initialSizeDelta.y);
            }
        }
    }

    public void ForceDrop(GameObject objectToDrop, GridLayout gridLayout, int windowID)
    {
        if (objectToDrop != null)
        {
            objectToDrop.transform.SetParent(transform);

            WindowGrid grid = GetGridFromNb(gridLayout);
            RectTransform rectTransform = objectToDrop.GetComponent<RectTransform>();
            Window window = objectToDrop.GetComponent<Window>();

            rectTransform.localPosition = new Vector3(grid.Cells[windowID].x, grid.Cells[windowID].y, rectTransform.localPosition.z);
            rectTransform.sizeDelta = new Vector2(grid.Cells[windowID].width, grid.Cells[windowID].height);
            window.minSizeWindow = grid.MinSize;
            window.maxSizeWindow = grid.MaxSize;
            window.windowId = windowID;
            window.GridLayout = gridLayout;
        }
    }

    private WindowGrid GetItemGridFromSize(Rect itemRect, int windowIndex)
    {
        //Need to perform float comparaison due to round up error with an epsilon value, here : 0.0001f
        if (itemRect.width > TwoBy3.Cells[windowIndex].width && (itemRect.height - (1.5f * TwoBy3.Cells[windowIndex].height) < 0.0001f))
        {
            return OneBy3;
        }
        else if (itemRect.width <= TwoBy3.Cells[windowIndex].width && itemRect.height <= TwoBy3.Cells[windowIndex].height)
        {
            return TwoBy3;
        }
        else
        {
            return TwoBy2;
        }
    }

    private GridLayout GetLayoutFromGrid(WindowGrid grid)
    {
        if (grid.ColumnCount == 2 && grid.RowCount == 3) return GridLayout.TwoBy3;
        else if (grid.ColumnCount == 1 && grid.RowCount == 3) return GridLayout.OneBy3;
        else if (grid.ColumnCount == 2 && grid.RowCount == 2) return GridLayout.TwoBy2;
        else
        {
            UnityEngine.Debug.Log("WindowLayout.GetLayoutFromGrid : grid parameters not supported, returning default 2by2");
            return GridLayout.TwoBy2;
        }
    }

    private WindowGrid GetGridFromNb(GridLayout grid)
    {
        switch (grid)
        {
            case GridLayout.TwoBy3: return TwoBy3;
            case GridLayout.OneBy3: return OneBy3;
            case GridLayout.TwoBy2: return TwoBy2;
            default:
                {
                    UnityEngine.Debug.LogError("WindowLayout.GetGridFromNb : GridNb unknown, returning default 2by2");
                    return TwoBy2;
                }
        }
    }
}