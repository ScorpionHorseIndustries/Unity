using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CardboardKeep;
using NoYouDoIt.TheWorld;

namespace NoYouDoIt.Controller {

  public enum MOUSE_MODE {
    SELECT, BUILD
  }

  public enum INPUT_MODE {
    GAME, CONSOLE, SHOWING_JOBS, SHOWING_DIALOGUE
  }


  public class InputController : MonoBehaviour {
    INPUT_MODE inputMode = INPUT_MODE.GAME;
    public const int MOUSE_LEFT = 0;
    public const int MOUSE_RIGHT = 1;
    public const int MOUSE_MIDDLE = 2;

    public MOUSE_MODE mouseMode = MOUSE_MODE.SELECT;

    WorldController wcon;
    BuildController bCon;
    private const float MIN_ZOOM = 5;
    private const float MAX_ZOOM = 20;

    private float currentZoom = 0;

    public GameObject cursorPrefab;
    private List<GameObject> dragPreviewList = new List<GameObject>();
    Camera cam;
    int cx = 0, cy = 0, mx = 0, my = 0, lastX = 0, lastY = 0;
    float mxf = 0, myf = 0;

    Tile dragStart, dragEnd;
    private List<Tile> dragArea = new List<Tile>();

    Vector2 lastFrame = new Vector2();
    Vector2 dragStartPos = new Vector2();
    //Vector2 dragEndPos = new Vector2();
    Vector2 mousePos;

    Vector2 dragTL = new Vector2();
    Vector2 dragTR = new Vector2();
    Vector2 dragBL = new Vector2();
    Vector2 dragBR = new Vector2();
    Vector2 dragLeftMost = new Vector2();
    Vector2 dragRightMost = new Vector2();

    Tile tileTL = null;
    Tile tileTR = null;
    Tile tileBL = null;
    Tile tileBR = null;
    public Tile mouseOverTile { get; private set; } = null;
    public SelectionInfo selectionInfo;
    public bool first { get; private set; } = true;

    private float tileSize = 0;
    private float HALF_tileSize;
    private bool tilesFound = false;
    GameObject uConsoleGO;
    CardboardKeep.UConsole UConsoleObj;
    private bool dialogueWindowOpen = false;




    private void Awake() {

    }


    //private string buildItem = "";
    //private string buildTile = "";

    // Start is called before the first frame update
    private bool initialised = false;
    public Bounds camBounds;
    public void Init() {
      //WorldController.Instance.cbRegisterReady(Init);
      Debug.Log("init " + this.name);
      initialised = true;
      wcon = WorldController.Instance;
      cam = Camera.main;
      tileSize = World.current.tileSize;
      HALF_tileSize = tileSize / 2;
      bCon = WorldController.Instance.buildController;
      bCon.inputCon = this;
      cam.transform.position = new Vector3(World.current.width / 2, World.current.height / 2, cam.transform.position.z);
      cursorPrefab = Instantiate(cursorPrefab, this.transform.position, Quaternion.identity);
      cursorPrefab.transform.SetParent(this.transform, true);
      cursorPrefab.SetActive(false);
      //cursorPrefab.GetComponent<SpriteRenderer>().sprite = NYDISpriteManager.Instance.GetSprite("other::cursor_slice_2");
      uConsoleGO = WorldController.Instance.uConsoleObject;
      UConsoleObj = uConsoleGO.GetComponent<UConsole>();
      //WorldController.Instance.uConsoleObject.SetActive(false);

    }
    void Start() {


    }



    void UpdateCamera() {

      float lr = Input.GetAxis("Horizontal");
      float ud = Input.GetAxis("Vertical");
      //Debug.Log(lr + "," + ud);
      Vector2 diff = Vector2.zero;
      if (Input.GetMouseButton(MOUSE_MIDDLE)) {

        diff = lastFrame - mousePos;


      }
      diff.Set(diff.x + lr, diff.y + ud);

      cam.transform.Translate(diff);

      float tempzoom = cam.orthographicSize;
      currentZoom = cam.orthographicSize;
      currentZoom -= currentZoom * Input.GetAxis("Mouse ScrollWheel");

      currentZoom = Mathf.Clamp(currentZoom, MIN_ZOOM, MAX_ZOOM);

      cam.orthographicSize = currentZoom;

      if (diff.sqrMagnitude > 0 || tempzoom != currentZoom || camBounds == null) {
        camBounds = OrthographicBounds();
        //Debug.Log(camBounds);

      }

    }

    public Bounds OrthographicBounds() {
      float screenAspect = (float)Screen.width / (float)Screen.height;
      float cameraHeight = cam.orthographicSize * 2;
      Bounds bounds = new Bounds(cam.transform.position, new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
      return bounds;
    }
    // Update is called once per frame

    //state
    bool showingJobs = false;
    //state

    public void SetInputMode(INPUT_MODE inputMode) {
      this.inputMode = inputMode;
    }

    void Update() {
      if (!initialised) return;


      switch (inputMode) {
        case INPUT_MODE.GAME:
          if (first) {
            UConsoleObj.Deactivate();
            uConsoleGO.SetActive(false);
            first = false;
          }
          if (Input.GetKeyDown(KeyCode.BackQuote)) {
            ActivateConsole();
          } else if (Input.GetKeyUp(KeyCode.E)) {
            ActivateJobs();
          } else {
            if (wcon.eventSystem.IsPointerOverGameObject()) {
              return;
            }

            if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1)) {
              WorldController.Instance.SetBuildType_Clear();
              destroyCursors();
              selectionInfo = null;

            }

            mouseOverTile = null;

            mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            mxf = mousePos.x;
            myf = mousePos.y;
            mx = (int)Mathf.Round(mousePos.x);
            my = (int)Mathf.Round(mousePos.y);
            //mp.Set(mouseX, mouseY);
            cx = mx - lastX;
            cy = my - lastY;
            if (mx >= 0 && my >= 0) {
              mouseOverTile = World.current.GetTileAt(mx, my);
              if (selectionInfo != null) {
                WorldController.Instance.UpdateCurrentTile(selectionInfo);
              }
            }
            //if (mouseOverTile != null) {
            //  Debug.Log(mouseOverTile);
            //} else {
            //  Debug.Log("move over nothing");
            //}

            UpdateSelected();

            UpdateDrag();


            UpdateCamera();


            //transform.Translate(cx, cy, 0);
            lastX = mx;
            lastY = my;
            lastFrame = cam.ScreenToWorldPoint(Input.mousePosition);
          }


          break;
        case INPUT_MODE.CONSOLE:
          if (Input.GetKeyDown(KeyCode.BackQuote)) {
            DeactivateConsole();
          }
          break;
        case INPUT_MODE.SHOWING_JOBS:
          if (Input.GetKeyUp(KeyCode.E)) {
            DeactivateJobs();

          }
          break;
        case INPUT_MODE.SHOWING_DIALOGUE:
          selectionInfo = null;
          break;
        default:
          break;
      }


    }

    void ActivateJobs() {



      WorldController.Instance.jobsPanelPrefab.SetActive(true);

      WorldController.Instance.gameState = GAME_STATE.PAUSE;
      WorldController.Instance.CreateJobPanelItems();
      inputMode = INPUT_MODE.SHOWING_JOBS;




    }

    void DeactivateJobs() {
      WorldController.Instance.gameState = GAME_STATE.PLAY;
      WorldController.Instance.DestroyJobPanelItems();
      inputMode = INPUT_MODE.GAME;
      WorldController.Instance.jobsPanelPrefab.SetActive(false);

    }

    void ActivateConsole() {
      uConsoleGO.SetActive(true);
      UConsoleObj.Activate();
      WorldController.Instance.gameState = GAME_STATE.PAUSE;
      inputMode = INPUT_MODE.CONSOLE;
    }

    void DeactivateConsole() {
      UConsoleObj.Deactivate();
      uConsoleGO.SetActive(false);
      WorldController.Instance.gameState = GAME_STATE.PLAY;
      inputMode = INPUT_MODE.GAME;
    }

    /*
    void Update() {
      if (!initialised) return;

      if (first) {
        UConsoleObj.Deactivate();
        uConsoleGO.SetActive(false);
        first = false;
      }


      if (Input.GetKeyDown(KeyCode.BackQuote)) {
        //GameObject go = WorldController.Instance.uConsoleObject;
        //CardboardKeep.UConsole uc = WorldController.Instance.uConsoleObject.GetComponent<CardboardKeep.UConsole>();
        if (uConsoleGO.activeSelf) {
          UConsoleObj.Deactivate();
          uConsoleGO.SetActive(false);
          WorldController.Instance.gameState = GAME_STATE.PLAY;
        } else {
          uConsoleGO.SetActive(true);
          UConsoleObj.Activate();
          WorldController.Instance.gameState = GAME_STATE.PAUSE;

        }


      }

      if (uConsoleGO.activeSelf) {
        return;
      }





      if (Input.GetKeyUp(KeyCode.E)) {
        showingJobs = !showingJobs;


        WorldController.Instance.jobsPanelPrefab.SetActive(showingJobs);
        if (showingJobs) {
          WorldController.Instance.gameState = GAME_STATE.PAUSE;
          WorldController.Instance.CreateJobPanelItems();
        } else {
          WorldController.Instance.gameState = GAME_STATE.PLAY;
          WorldController.Instance.DestroyJobPanelItems();
        }



      }

      if (wcon.eventSystem.IsPointerOverGameObject()) {
        return;
      }

      if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1)) {
        WorldController.Instance.SetBuildType_Clear();
        destroyCursors();
        selectionInfo = null;

      }

      mouseOverTile = null;

      mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
      mxf = mousePos.x;
      myf = mousePos.y;
      mx = (int)Mathf.Round(mousePos.x);
      my = (int)Mathf.Round(mousePos.y);
      //mp.Set(mouseX, mouseY);
      cx = mx - lastX;
      cy = my - lastY;
      if (mx >= 0 && my >= 0) {
        mouseOverTile = World.current.GetTileAt(mx, my);
        if (selectionInfo != null) {
          WorldController.Instance.UpdateCurrentTile(selectionInfo);
        }
      }
      //if (mouseOverTile != null) {
      //  Debug.Log(mouseOverTile);
      //} else {
      //  Debug.Log("move over nothing");
      //}

      UpdateSelected();

      UpdateDrag();


      UpdateCamera();


      //transform.Translate(cx, cy, 0);
      lastX = mx;
      lastY = my;
      lastFrame = cam.ScreenToWorldPoint(Input.mousePosition);
    }
    */

    private void UpdateSelected() {
      if (mouseOverTile != null) {
        if (Input.GetKeyUp(KeyCode.Escape)) {
          selectionInfo = null;
        }

        if (mouseMode != MOUSE_MODE.SELECT) {
          return;
        }

        if (Input.GetMouseButtonUp(0)) {
          if (selectionInfo == null || selectionInfo.tile != mouseOverTile) {
            selectionInfo = new SelectionInfo();
            selectionInfo.tile = mouseOverTile;
            SetSelectionContents();

            for (int i = 0; i < selectionInfo.contents.Length; i += 1) {
              if (selectionInfo.contents[i] != null) {
                selectionInfo.subSelect = i;
                break;
              }
            }
          } else {

            SetSelectionContents();
            do {
              selectionInfo.subSelect = (selectionInfo.subSelect + 1) % selectionInfo.contents.Length;

            } while (selectionInfo.contents[selectionInfo.subSelect] == null);

          }
        }
      }
    }

    private void UpdateDrag() {
      if (mouseMode == MOUSE_MODE.SELECT) return;

      if (Input.GetMouseButtonDown(0)) {
        dragStart = mouseOverTile;
        dragStartPos.Set(mx, my);
      }

      if (Input.GetMouseButton(0)) {

        destroyCursors();
        //display drag area
        setDragArea();
        drawCursors();

      }

      if (Input.GetMouseButtonUp(0)) {
        destroyCursors();
        //display drag area
        setDragArea();
        drawCursors();
        if (bCon.CreateBuildJobs(dragArea)) {
          destroyCursors();
        }
      }
    }





    private void drawCursors() {
      if (tilesFound) {
        cursorPrefab.transform.position = new Vector2(tileTL.world_x - 0.5f, tileTL.world_y + 0.5f);

        float x = tileTR.world_x - tileTL.world_x;// dragRightMost.x - dragLeftMost.x;
        float y = tileTL.world_y - tileBR.world_y; // dragLeftMost.y - dragRightMost.y;
        cursorPrefab.GetComponent<SpriteRenderer>().size = new Vector2(x + 1, y + 1);
        cursorPrefab.SetActive(true);
        /*
        foreach (Tile tile in dragArea)
        {
          GameObject g = SimplePool.Spawn(cursorPrefab, new Vector2(tile.x, tile.y), Quaternion.identity);
          g.transform.SetParent(this.transform, true);
          dragPreviewList.Add(g);

        }
        */
      }

    }
    private void destroyCursors() {
      cursorPrefab.SetActive(false);
      /*
      while(dragPreviewList.Count > 0)
      {
        GameObject g = dragPreviewList[0];
        dragPreviewList.Remove(g);
        SimplePool.Despawn(g);
      }
      */
    }

    public void SetSelectionContents() {
      selectionInfo.contents = new object[selectionInfo.tile.countOfOccupied + 3];

      // Copy the character references
      for (int i = 0; i < selectionInfo.tile.countOfOccupied; i += 1) {
        selectionInfo.contents[i] = selectionInfo.tile.GetOccupant(i);
      }

      // Now assign references to the other three sub-selections available
      selectionInfo.contents[selectionInfo.contents.Length - 3] = selectionInfo.tile.installedItem;
      selectionInfo.contents[selectionInfo.contents.Length - 2] = selectionInfo.tile.GetFirstInventoryItem();
      selectionInfo.contents[selectionInfo.contents.Length - 1] = selectionInfo.tile;
    }

    private void setDragArea() {
      dragArea.Clear();
      int start_x = (int)Mathf.Min(dragStartPos.x, mx);
      int end_x = (int)Mathf.Max(dragStartPos.x, mx);

      int start_y = (int)Mathf.Min(dragStartPos.y, my);
      int end_y = (int)Mathf.Max(dragStartPos.y, my);

      dragLeftMost.Set(start_x, start_y);
      dragRightMost.Set(end_x, end_y);

      dragTL.Set(start_x, start_y);
      dragTR.Set(end_x, start_y);
      dragBL.Set(start_x, end_y);
      dragBR.Set(end_x, end_y);
      tileTL = null;
      tileTR = null;
      tileBL = null;
      tileBR = null;
      tilesFound = false;




      for (int xx = start_x; xx <= end_x; xx += 1) {
        for (int yy = start_y; yy <= end_y; yy += 1) {
          Tile tile = World.current.GetTileIfChunkExists(xx, yy);
          //Debug.Log(String.Format("x,y: {0},{1}", xx, yy));
          if (tile != null) {
            dragArea.Add(tile);
            tilesFound = true;
            tileTL = coalesce(tileTL, tile);
            tileTR = coalesce(tileTR, tile);
            tileBL = coalesce(tileBL, tile);
            tileBR = coalesce(tileBR, tile);


            if (tile.world_x <= tileTL.world_x && tile.world_y >= tileTL.world_y) {
              tileTL = tile;
            }
            if (tile.world_x >= tileTR.world_x && tile.world_y >= tileTR.world_y) {
              tileTR = tile;
            }
            if (tile.world_x <= tileBL.world_x && tile.world_y <= tileBL.world_y) {
              tileBL = tile;
            }
            if (tile.world_x >= tileBR.world_x && tile.world_y <= tileBR.world_y) {
              tileBR = tile;
            }





          }
        }
      }


    }



    Tile coalesce(Tile a, Tile b) {
      if (a == null) {
        return b;
      } else {
        return a;
      }
    }
  }


  public class SelectionInfo {
    public Tile tile;
    public string info;
    public int subSelect = 0;
    public System.Object[] contents;
  }
}