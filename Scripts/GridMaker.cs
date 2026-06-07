using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridMaker : MonoBehaviour
{
    const float stepLength = 0.48586827175664567818286387f; //Mathf.Sqrt(Mathf.Sqrt(5)-2);
    const float halfStep = 0.25926358732362497806075346f;   //(1-sqrt(1-a^2))/a where a = stepLength;

    [SerializeField]
    int GridSize;
    [SerializeField]
    List<int> GridObjects;

    [SerializeField]
    List<GameObject> TileObjects;


    //this is a path string consisting of the letters U - up, D - down, R - right, L - left
    //followed by a '$' and then a number - the tile ID

    [SerializeField]
    List<string> Tiles;

    private void Start()
    {
        if (GridSize>0)
            MakeFullGrid(GridSize);
        MakeFromIn();
    }

    void MakeFromIn()
    {
        for (int i = 0; i < Tiles.Count; i++)
        {
            string path = Tiles[i].TrimEnd("$1234567890".ToArray());
            string ID_string = Tiles[i].TrimStart("$UDLR".ToArray());
            int ID = 0;

            if (!int.TryParse(ID_string, out ID))
                Debug.LogError("Could not parse ID for tile #" + i.ToString() + " (" + Tiles[i] + ")");

            MakeTileAt(TracePath(path), TileObjects[ID]);
        }
    }
    void MakeFullGrid(int size)
    {
        var TakenPositions = new List<Complex>();

        var q = new Queue<Tuple<int, MobiusGyroposition>>();
        q.Enqueue(new Tuple<int,MobiusGyroposition>(0, new MobiusGyroposition()));

        while(q.Count > 0)
        {
            var top = q.Dequeue();
            if (top.Item1 >= size)
                continue;
            if (!CheckGridAviability(top.Item2, TakenPositions))
                continue;

            MakeTileAt(top.Item2, TileObjects[GridObjects[Random.Range(0, GridObjects.Count)]]);
            TakenPositions.Add(top.Item2.Gyrovector);
            q.Enqueue(new Tuple<int, MobiusGyroposition>(top.Item1 + 1, top.Item2.LMovedBy(new Complex(0, stepLength))));
            q.Enqueue(new Tuple<int, MobiusGyroposition>(top.Item1 + 1, top.Item2.LMovedBy(new Complex(0, -stepLength))));
            q.Enqueue(new Tuple<int, MobiusGyroposition>(top.Item1 + 1, top.Item2.LMovedBy(new Complex(stepLength, 0))));
            q.Enqueue(new Tuple<int, MobiusGyroposition>(top.Item1 + 1, top.Item2.LMovedBy(new Complex(-stepLength, 0))));
        }
    }

    bool CheckGridAviability(MobiusGyroposition current, List<Complex> TakenPositions)
    {
        foreach(Complex taken in TakenPositions)
        {
            if (Hyperbolic.VectorAddition(-current.Gyrovector, taken).sqrLength() <= stepLength * stepLength * 0.25f)
                return false;
        }
        return true;
    }

    void MakeTileAt(MobiusGyroposition gyroposition, GameObject TileObject)
    {
        GameObject newTile = Instantiate(TileObject, transform.position, Quaternion.identity, transform);

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetVector("_PositionGyrovector", (Vector2)gyroposition.Gyrovector);
        propertyBlock.SetFloat("_PositionGyration", gyroposition.Gyration);
        propertyBlock.SetVector("unity_SpriteProps", new Vector4(1, 1, -1, 0));
        propertyBlock.SetColor("unity_SpriteColor", Color.white);

        newTile.GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);

        TileHCollider col;
        if (!newTile.TryGetComponent(out col))
            return;

        col.gyroposition = gyroposition;

        return;
    }

    MobiusGyroposition TracePath(string path)
    {
        MobiusGyroposition mobiusGyroposition = new MobiusGyroposition(0, 0);
        
        for(int i = path.Length-1; i>=0; i--)
        {
            mobiusGyroposition.LMoveBy((Complex)(LetterToVector(path[i])*stepLength));
        }

        return mobiusGyroposition;
    }

    Vector2 LetterToVector(char c)
    {
        switch(c)
        {
            case 'U':
                return Vector2.up;
            case 'D':
                return Vector2.down;
            case 'R':
                return Vector2.right;
            case 'L':
                return Vector2.left;
        }
        return Vector2.zero;
    }
}
