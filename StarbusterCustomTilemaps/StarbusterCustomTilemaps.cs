using UnityEngine;
using UModFramework.API;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Collections;

namespace StarbusterCustomTilemaps
{
    [UMFScript]
    class StarbusterCustomTilemaps : MonoBehaviour
    {
        public static string currentSceneName = "";
        public static string previousSceneName = "";
        bool carnivalIsLoaded = false;
        bool carnivalApplied = false;

        Texture2D carnivalTexture = null;
        Sprite carnivalSprite = null;
        //Dictionary<string, Sprite> carnivalSprites = new Dictionary<string, Sprite>();
        Dictionary<string, Tile> carnivalTiles = new Dictionary<string, Tile>();

        Tilemap replaceMySprite = null;

        internal static void Log(string text, bool clean = false)
        {
            using (UMFLog log = new UMFLog()) log.Log(text, clean);
        }

        public static void DoubleLog(string txt) 
        {
            Debug.Log(txt);
            Log(txt);
        }

        public static void DebugLog(string txt)
        {
            DoubleLog(txt);
        }

        [UMFConfig]
        public static void LoadConfig()
        {
            StarbusterCustomTilemapsConfig.Load();
        }

		void Awake()
		{
			Log("StarbusterCustomTilemaps v" + UMFMod.GetModVersion().ToString(), true);
            LoadCarnivalNightSprite();

        }

        private void LoadCarnivalNightSprite()
        {
            StartCoroutine(IHaveNoIdeaWhatToCallThis());
        }

        // Use this for initialization
        IEnumerator IHaveNoIdeaWhatToCallThis()
        {
            Log("Carnival Part 1");
            WWW www = new WWW("file://" + @"D:\Games\SAGE 2021\Starbuster Demo 2021 V1.06 - Modded\union.png");
            while (!www.isDone)
                yield return null;

            carnivalIsLoaded = true;
            carnivalTexture = www.texture;
            carnivalTexture.filterMode = FilterMode.Point;
            Log("Carnival Part 2.");
        }


        void Update()
        {
            previousSceneName = currentSceneName;
            currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (previousSceneName != currentSceneName) 
            {
                Log("Scene changed to: " + currentSceneName + "; Attempting to dump tilemap data or replace sprites." );
                try
                {
                    FindTilemap();
                    FindAndReplaceUnionTilemap();
                }
                catch (System.Exception e) 
                {
                    DoubleLog("There was an error when attempting to use FindTilemap somewhere:\r\n");
                    DoubleLog(e.ToString());
                }
                

                carnivalApplied = false;
            }
            if (carnivalIsLoaded && !carnivalApplied) 
            {
                // Use the heirarchy object path finder to grab the correct tileset and assign the new sprite.
                carnivalApplied = true;
            }
        }

        private void FindAndReplaceUnionTilemap()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string tilemapPath = "";
            Tilemap[] tilemaps = Object.FindObjectsOfType<Tilemap>();
            if (tilemaps.Length >= 1)
            {
                Tilemap tm = null;
                string tileInfoString = "";
                Sprite currentTileSprite = null;
                int indexZ = 0;
                int indexX = 0;
                int indexY = 0;
                Vector3Int currentCellPosition = Vector3Int.zero;

                HashSet<string> tileNames = new HashSet<string>();

                DoubleLog("About to start the GIANT MESS OF NESTED LOOPS TO REPLACE TILEMAPS FOR: " + currentSceneName);
                for (int i = 0; i < tilemaps.Length; i++)
                {
                    tm = tilemaps[i];
                    tilemapPath += "\"tilemap:{\"" + i.ToString() + ": " + GetGameObjectPath(tm.transform) + "}";
                    tilemapPath += "\r\n";

                    if (currentSceneName.ToUpper().Contains("UNION")) 
                    {
                        DoubleLog("This tilemap appears to be related to Union. Do the replacement. Except not now, because you didn't add that yet.");
                    }
                    for (indexZ = tm.cellBounds.zMin; indexZ < tm.cellBounds.zMax; indexZ++)
                    {
                        for (indexX = tm.cellBounds.xMin; indexX < tm.cellBounds.xMax; indexX++)
                        {
                            for (indexY = tm.cellBounds.yMin; indexY < tm.cellBounds.yMax; indexY++)
                            {
                                currentCellPosition = new Vector3Int(indexX, indexY, indexZ);
                                TileBase currentTile = tm.GetTile(currentCellPosition);
                                if (currentTile != null)
                                {
                                    currentTileSprite = tm.GetSprite(currentCellPosition);
                                    if (currentTileSprite != null)
                                    {
                                        if (GetGameObjectPath(tm.transform).Equals("Grid/Fore") /*|| GetGameObjectPath(tm.transform).Equals("Grid/Mid")*/)
                                        {
                                            DoubleLog("Attempting to overwrite Union texture data.");
                                            //currentTileSprite.texture.LoadImage(carnivalTexture.GetRawTextureData());
                                            if (!carnivalTiles.ContainsKey(currentTile.name))
                                            {
                                                SwapTileWithNewCustomTile(tm, currentCellPosition, currentTileSprite, carnivalTexture, currentTile.name);
                                            }
                                            else 
                                            {
                                                SwapTileWithLoadedCustomTile(tm, currentCellPosition, carnivalTiles[currentTile.name]);
                                            }
                                        }
                                    }

                                    tileInfoString = currentCellPosition.ToString() + "; " + currentTile.name;
                                    tilemapPath += tileInfoString + "\r\n";

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                tilemapPath = "Couldn't find any tilemaps to replace...";
            }
            DoubleLog(tilemapPath);
            //SaveStringToFile(tilemapPath, sceneName + "-tilemaps.txt");
        }

        public void SwapTileWithNewCustomTile(Tilemap tilemap, Vector3Int tilePos, Sprite oldSprite, Texture2D newTexture, string newTileName) 
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            DoubleLog("Old Sprite Rectangle: " + oldSprite.textureRect.ToString() + ", RectOffset:" + oldSprite.textureRectOffset.ToString() 
                + ", Pivot:" + oldSprite.pivot.ToString());
            //Sprite newSprite = Sprite.Create(newTexture, oldSprite.textureRect, /*oldSprite.pivot*/ new Vector2(0.5f, 0.5f), oldSprite.pixelsPerUnit);
            Vector2 newPivot = (oldSprite.pivot / new Vector2(oldSprite.textureRect.width, oldSprite.textureRect.height));
            Sprite newSprite = Sprite.Create(newTexture, oldSprite.textureRect, newPivot, oldSprite.pixelsPerUnit);
            tile.sprite = newSprite;
            //Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(10, 10, 1));
            //tile.transform.
            tilemap.SetTile(tilePos, tile);

            carnivalTiles.Add(newTileName, tile);
        }

        public void SwapTileWithLoadedCustomTile(Tilemap tilemap, Vector3Int tilePos, Tile newTile)
        {
            tilemap.SetTile(tilePos, newTile);
        }

        private void Beh()
        {
            // convert texture to sprite if required otherwise load sprite directly from resources folder
            Texture2D myTexture = Resources.Load<Texture2D>("Images/SampleImage");
            
        }

        /*
    public override void OnInspectorGUI()
    {
        LevelScript myTarget = (LevelScript)target;

        myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
        EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
    }*/

        static void InsertObject()
        {
            DoubleLog("Inserting a GameObject");
            // Create a custom game object
            GameObject go = new GameObject("Custom Game Object");
        }

        public void FindTilemap()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string tilemapPath = "";
            Tilemap[] tilemaps = Object.FindObjectsOfType<Tilemap>();
            if (tilemaps.Length >= 1)
            {
                Tilemap tm = null;
                string tileInfoString = "";
                Sprite currentTileSprite = null;
                int indexZ = 0;
                int indexX = 0;
                int indexY = 0;
                Vector3Int currentCellPosition = Vector3Int.zero;

                HashSet<string> tileNames = new HashSet<string>();

                DoubleLog("About to start the GIANT MESS OF NESTED LOOPS FOR: " + currentSceneName);
                for (int i = 0; i < tilemaps.Length; i++)
                {
                    tm = tilemaps[i];
                    tilemapPath += i.ToString() + ": " + GetGameObjectPath(tm.transform);
                    tilemapPath += "\r\n";

                    for (indexZ = 0; indexZ < tm.size.z; indexZ++)
                    {
                        for (indexX = 0; indexX < tm.size.x; indexX++)
                        {
                            for (indexY = 0; indexY < tm.size.y; indexY++)
                            {
                                currentCellPosition = new Vector3Int(indexX, indexY, indexZ);
                                TileBase currentTile = tm.GetTile(currentCellPosition);
                                if (currentTile != null)
                                {
                                    if (!tileNames.Contains(currentTile.name))
                                    {
                                        currentTileSprite = tm.GetSprite(currentCellPosition);
                                        if (currentTileSprite != null)
                                        {
                                            try
                                            {
                                                if (string.IsNullOrEmpty(currentTile.name))
                                                {
                                                    currentTile.name = "Unknown-" + indexX.ToString() + "-" + indexY.ToString() + "-" + indexZ.ToString();
                                                }
                                                //SaveSpriteTextureToFile(currentTileSprite, currentTile.name + ".png");
                                            }
                                            catch (System.UnauthorizedAccessException e)
                                            {
                                                DoubleLog(e.ToString());
                                            }
                                        }
                                        else
                                        {
                                            DoubleLog("Attempted to save tile sprite \"" + currentTile.name + " \"But it appears to be null.");
                                        }
                                        tileNames.Add(currentTile.name);
                                    }
                                    tileInfoString = currentCellPosition.ToString() + "; " + currentTile.name;
                                    tilemapPath += tileInfoString + "\r\n";

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                tilemapPath = "Could not find any tilemaps.";
            }
            DoubleLog(tilemapPath);
            SaveStringToFile(tilemapPath, sceneName + "-tilemaps.txt");
            SaveCarnivalTilesToFile(carnivalTiles, sceneName + "-tiles.txt");
        }

        private static void SaveCarnivalTilesToFile(Dictionary<string, Tile> theCarnivalTiles, string fileName)
        {
            if (theCarnivalTiles == null) { return; }
            try 
            {
                string text = "";
                DoubleLog("About to do a Foreach");
                List<Tile> tiles = new List<Tile>(theCarnivalTiles.Values);
                foreach (Tile t in tiles)
                {
                    if (t == null) { break; }
                    text += "name:{" + t.name + "} ";
                    text += "textureRect:{" + t.sprite.textureRect + "} ";
                    text += "pivot:{" + t.sprite.pivot + "} ";
                    text += "pixelsPerUnit:{" + t.sprite.pixelsPerUnit + "} ";
                    text += "\r\n";
                }
                DoubleLog("Did the foreach, saving...");
                SaveStringToFile(text, fileName);
            }
            catch (System.NullReferenceException e) 
            {
                DoubleLog("Oh boy, we got a null in the Tiles saver. " + e.ToString());
            }
        }

        private static string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private static void SaveTextureToFile(Texture2D texture, string fileName)
        {
            string fullPath = Application.dataPath + "/" + fileName;
            DoubleLog("Attempting to write image: " + fullPath);

            try
            {
                var bytes = texture.EncodeToPNG();

                FileStream file = System.IO.File.Open(fullPath, FileMode.Create);
                BinaryWriter binary = new BinaryWriter(file);
                binary.Write(bytes);
                file.Close();
                DoubleLog("Succeeded.");
            }
            catch (System.Exception e)
            {
                DoubleLog("Failed.");
                DoubleLog(e.ToString());
            }
        }

        private static void SaveSpriteTextureToFile(Sprite sprite, string fileName)
        {
            Texture2D texture = sprite.texture;
            SaveTextureToFile(texture, fileName);
        }

        private static void SaveStringToFile(string text, string fileName)
        {
            string fullPath = Application.dataPath + "/" + fileName;

            DoubleLog("Attempting to write text file: " + fullPath);
            using (StreamWriter outputFile = new StreamWriter(fullPath))
            {
                outputFile.WriteLine(text);
            }
            DoubleLog("Succeeded.");
        }
    }
}