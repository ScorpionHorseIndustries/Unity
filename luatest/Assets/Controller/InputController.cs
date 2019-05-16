using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class InputController : MonoBehaviour {
  public const int MOUSE_LEFT = 0;
  public const int MOUSE_RIGHT = 1;
  public const int MOUSE_MIDDLE = 2;



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
  Tile mouseOverTile = null;
  private float tileSize = 0;
  private float HALF_tileSize;
  private bool tilesFound = false;


  //private string buildItem = "";
  //private string buildTile = "";

  // Start is called before the first frame update
  private bool initialised = false;
  void Init(string s) {
    Debug.Log("init " + this.name);
    initialised = true;
    wcon = WorldController.Instance;
    cam = Camera.main;
    tileSize = wcon.world.tileSize;
    HALF_tileSize = tileSize / 2;
    bCon = BuildController.Instance;
    bCon.inputCon = this;
    cam.transform.position = new Vector3(wcon.world.width / 2, wcon.world.height / 2, cam.transform.position.z);
  }
  void Start() {
    WorldController.Instance.cbRegisterReady(Init);


  }

  public void setBuildType_InstalledItem(string item) {
    bCon.SetBuild(BuildController.BUILDTYPE.INSTALLEDITEM, item);
  }

  public void setBuildType_Tile(string tile) {
    bCon.SetBuild(BuildController.BUILDTYPE.TILE, tile);
  }

  void updateCamera() {

    if (Input.GetMouseButton(MOUSE_MIDDLE)) {
      Vector2 diff = lastFrame - mousePos;
      cam.transform.Translate(diff);
    }
    currentZoom = cam.orthographicSize;
    currentZoom -= currentZoom * Input.GetAxis("Mouse ScrollWheel");

    currentZoom = Mathf.Clamp(currentZoom, MIN_ZOOM, MAX_ZOOM);

    cam.orthographicSize = currentZoom;



  }
  // Update is called once per frame
  void Update() {
    if (!initialised) return;
    if (wcon.eventSystem.IsPointerOverGameObject()) {
      return;
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
    mouseOverTile = WorldController.Instance.world.getTileAt(mx, my);


    updateDrag();


    updateCamera();


    //transform.Translate(cx, cy, 0);
    lastX = mx;
    lastY = my;
    lastFrame = cam.ScreenToWorldPoint(Input.mousePosition);
  }

  private void updateDrag() {
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
      bCon.CreateBuildJobs(dragArea);
    }
  }





  private void drawCursors() {
    if (tilesFound) {
      cursorPrefab.transform.position = new Vector2(tileTL.x - 0.5f, tileTL.y + 0.5f);

      float x = tileTR.x - tileTL.x;// dragRightMost.x - dragLeftMost.x;
      float y = tileTL.y - tileBR.y; // dragLeftMost.y - dragRightMost.y;
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
    /*
		while(dragPreviewList.Count > 0)
		{
			GameObject g = dragPreviewList[0];
			dragPreviewList.Remove(g);
			SimplePool.Despawn(g);
		}
		*/
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
        Tile tile = WorldController.Instance.world.getTileAt(xx, yy);

        if (tile != null) {
          dragArea.Add(tile);
          tilesFound = true;
          tileTL = coalesce(tileTL, tile);
          tileTR = coalesce(tileTR, tile);
          tileBL = coalesce(tileBL, tile);
          tileBR = coalesce(tileBR, tile);


          if (tile.x <= tileTL.x && tile.y >= tileTL.y) {
            tileTL = tile;
          }
          if (tile.x >= tileTR.x && tile.y >= tileTR.y) {
            tileTR = tile;
          }
          if (tile.x <= tileBL.x && tile.y <= tileBL.y) {
            tileBL = tile;
          }
          if (tile.x >= tileBR.x && tile.y <= tileBR.y) {
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
