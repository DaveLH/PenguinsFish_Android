using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to handle procedural setting up of tiles at start of game play.  Once the tiles
///   are set up, this class is no longer referenced. (Unless/Until a new game is started)
/// </summary>
/// <remarks>
/// TODO: Generate fish on tiles, based on tile types
/// </remarks>
/// 
public class SetupTiles : MonoBehaviour
{
    public GameObject [] TilePrefabs;    // Prefabs defining each single tile type

    // "Fudge factors" for fine-tuning tile spacing
    //
    public float X_Padding;
    public float Z_Padding;

    float m_fTileDist;    // Distance between adjacent tiles' centers

    const string ColNames = "abcdefgh";  // Use for naming tiles

    TileManager m_tileManager;


    // Distance between adjacent tiles' centers
    //
    public float TileDistance
    {
        get { return m_fTileDist; }
    }

    // Radius of circle inscribed in hexagonal tile
    //
    public float TileRadius
    {
        get { return m_fTileDist / 2.0f; }
    }


    /// <summary>
    /// Set up a sing;e row of tiles
    /// </summary>
    /// <param name="v3Start">Starting point (i.e. center of first tile)</param>
    /// <param name="row">Current row</param>
    /// <param name="num">Number of tiles to place in this row</param>
    /// 
    void BuildRow(Vector3 v3Start, int row, int num)
    {
        for(int x = 0; x < num; x++)
        {
            Vector3 v3NewTileCenter = v3Start + (Vector3.right * TileDistance * (float)x * X_Padding);

            CreateTile(row, x, v3NewTileCenter);
        }
    }


    /// <summary>
    /// Instantiate phyiscal game tile, AND its internal data object
    /// </summary>
    /// <param name="row">Row #</param>
    /// <param name="col">Column #</param>
    /// <param name="v3NewTileCenter">Position to place new tile</param>
    /// 
    void CreateTile(int row, int col, Vector3 v3NewTileCenter)
    {
        TileManager.TileType ttype = TileManager.GetRndType();

        GameObject gobNewTile = GameObject.Instantiate(TilePrefabs[(int)ttype], v3NewTileCenter, Quaternion.identity, this.transform);

        GameTile gtile = gobNewTile.GetComponent<GameTile>();

        gtile.InitTileParms(ColNames[col], row, Tile.TileType.Empty);   // TODO: Create different tiles

        gobNewTile.name = gtile.ToString();

        // Add to table
        //
        m_tileManager.tileTable.Add(gtile.ToString(), gtile);
    }


    /// <summary>
    /// Setup all the tiles
    /// </summary>
    /// <param name="v3Start">Starting point (i.e. center of first tile)</param>
    /// <param name="dim">Maximum number of tiles on a side</param>
    /// 
    public bool SetupAll(Vector3 v3Start, int dim)
    {
        // Tiles per row alternates, but tiles per column is constant (or so says the artist part of my brain)!
        //
        int rowSizeOdd  = dim - 1;
        int rowSizeEven = dim;

        // Row starting points alternate (again, my "artist" brain talking)
        //
        Vector3 v3RowStartOdd  = v3Start;
        Vector3 v3RowStartEven = new Vector3(v3Start.x - TileRadius, v3Start.y, v3Start.z);

        Vector3 v3RowStart = Vector3.zero;   // Where each row will start

        int rowSize = 0;  // Number of tiles on each row (Either dim or dim-1)

        for (int z = 1; z <= dim; z++)
        {
            switch (z % 2)
            {
                case 0:   // Even-numbered row
                    //
                    v3RowStart = v3RowStartEven + (Vector3.forward * TileDistance * (float)z * Z_Padding);
                    rowSize = rowSizeEven;
                    break;

                case 1:   // Odd-numbered row
                    //
                    v3RowStart = v3RowStartOdd + (Vector3.forward * TileDistance * (float)z * Z_Padding);
                    rowSize = rowSizeOdd;
                    break;
            }

            BuildRow(v3RowStart, z, rowSize);
        }

        return true;
    }


    void Awake ()
    {
        m_fTileDist = TilePrefabs[0].GetComponent<SphereCollider>().radius * 2.0f;

        m_tileManager = GetComponent<TileManager>();
    }
}
