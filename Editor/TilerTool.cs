using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Tiler.Editor
{
    [EditorTool("Tiler Tool", typeof(Tiler))]
    public class TilerTool : EditorTool, IDrawSelectedHandles {
        private Tiler tiler;

        private Plane tilerBasePlane;
        private bool lastMouseOverTile = false;
        private Vector2Int highlightedTile = new Vector2Int(int.MinValue, int.MinValue);

        void OnEnable() {
            tiler = target as Tiler;
        }

        public override void OnActivated()
        {
            tilerBasePlane = new Plane(Vector3.up, new Vector3(0, tiler.transform.position.y, 0));
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView sceneView)) {
                return;
            }

            var ev = Event.current;

            // Whenever modifier keys are pressed, ignore all the events.
            if (ev.modifiers != EventModifiers.None) {
                return;
            }

            switch (ev.type) {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    // Check if the mouse is over a tile.
                    var mouseOverTile = IsMouseOverTile(ev.mousePosition, out highlightedTile);

                    // Trigger extra repaint if the mouse has just left a tile, to clear the highlighted tile.
                    if (!mouseOverTile && lastMouseOverTile) {
                        sceneView.Repaint();
                    }

                    // Repaint if the mouse is over any tile.
                    if (mouseOverTile) {
                        sceneView.Repaint();
                    }

                    // Update last mouse over tile.
                    lastMouseOverTile = mouseOverTile;
                    break;

                case EventType.MouseDown:
                    // TODO: Check if the tile is already occupied.
                    if (lastMouseOverTile) {
                        PlaceTile(highlightedTile);
                    }
                    ev.Use();
                    break;

                case EventType.Repaint:
                    // Draw tile under mouse cursor if the mouse is over a tile and not on another control.
                    if (lastMouseOverTile) {
                        var faceColor = Color.white;
                        faceColor.a = 0.5f;
                        Handles.color = faceColor;
                        DrawHighlightedTile(highlightedTile);
                    }
                    break;
            }
        }

        private void DrawHighlightedTile(Vector2Int tile) {
            // Compute tile corners.
            Vector3[] corners = new Vector3[] {
                tiler.Origin + new Vector3(tile.x * tiler.Size, 0, tile.y * tiler.Size),
                tiler.Origin + new Vector3(tile.x * tiler.Size + tiler.Size, 0, tile.y * tiler.Size),
                tiler.Origin + new Vector3(tile.x * tiler.Size + tiler.Size, 0, tile.y * tiler.Size + tiler.Size),
                tiler.Origin + new Vector3(tile.x * tiler.Size, 0, tile.y * tiler.Size + tiler.Size)
            };

            // Draw solid tile.
            Handles.DrawSolidRectangleWithOutline(corners, Handles.color, Color.clear);

            // Draw outline.
            Handles.color = Color.black;
            Handles.DrawSolidRectangleWithOutline(corners, Color.clear, Color.black);
        }

        private bool IsMouseOverTile(Vector2 mousePosition, out Vector2Int tile) {
            // Initialize tile to invalid value.
            tile = new Vector2Int(int.MinValue, int.MinValue);

            // Convert mouse position to world ray.
            var ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            // Check if the ray intersects with the tiler's base plane.
            if (tilerBasePlane.Raycast(ray, out float distance)) {
                Vector3 wp = ray.origin + ray.direction.normalized * distance;
                Vector3 relativePos = wp - tiler.Origin;

                // Compute tile coordinates.
                int tileX = Mathf.FloorToInt(relativePos.x / tiler.Size);
                int tileZ = Mathf.FloorToInt(relativePos.z / tiler.Size);

                // Check if the tile is within the grid bounds.
                if (tileX >= 0 && tileX < tiler.Rows && tileZ >= 0 && tileZ < tiler.Columns) {
                    tile = new Vector2Int(tileX, tileZ);
                    return true;
                }
            }

            return false;
        }

        private void PlaceTile(Vector2Int tile) {
            var tileWorldCenter = GetTileWorldCenter(tile);
            var tileObject = Instantiate(tiler.TilePrefab, tileWorldCenter, Quaternion.identity);
            tileObject.transform.parent = tiler.transform;
        }

        private Vector3 GetTileWorldOrigin(Vector2Int tile) {
            return tiler.Origin + new Vector3(tile.x * tiler.Size, 0, tile.y * tiler.Size);
        }

        private Vector3 GetTileWorldCenter(Vector2Int tile) {
            return GetTileWorldOrigin(tile) + new Vector3(tiler.Size / 2f, 0, tiler.Size / 2f);
        }

        public void OnDrawHandles()
        {
            Handles.color = Color.white;

            // Draw row (x) lines 
            for (int x = 0; x <= tiler.Rows; x++)
            {
                Vector3 start = tiler.Origin + new Vector3(x * tiler.Size, 0, 0);
                Vector3 end = start + new Vector3(0, 0, tiler.Columns * tiler.Size);
                Handles.DrawLine(start, end);
            }

            // Draw column(z) lines
            for (int z = 0; z <= tiler.Columns; z++)
            {
                Vector3 start = tiler.Origin + new Vector3(0, 0, z * tiler.Size);
                Vector3 end = start + new Vector3(tiler.Rows * tiler.Size, 0, 0);
                Handles.DrawLine(start, end);
            }
        }

        [Shortcut("Activate Tiler Tool", typeof(SceneView), KeyCode.T)]
        public static void TilerToolShortcut() {
            if (Selection.GetFiltered<Tiler>(SelectionMode.TopLevel).Length > 0) {
                ToolManager.SetActiveTool<TilerTool>();
            } else {
                Debug.Log("No Tiler selected.");
            }
        }
    }
}