using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GeodesicMaker : MonoBehaviour
{
    //copy of GridMaker
    const float stepLength = 0.48586827175664567818286387f; //Mathf.Sqrt(Mathf.Sqrt(5)-2);
    const float halfStep = 0.25926358732362497806075346f;   //(1-sqrt(1-a^2))/a where a = stepLength;

    [SerializeField]
    int HalfLineLength;

    [SerializeField]
    List<GameObject> TileObjects;


    //this is a path string consisting of the letters U - up, D - down, R - right, L - left
    //followed by a '$' and then a number - the tile ID

    [SerializeField]
    List<string> Lines;

    private void Start()
    {
        MakeFromIn();
    }

    void MakeFromIn()
    {
        for (int i = 0; i < Lines.Count; i++)
        {
            string path = Lines[i].TrimEnd("$1234567890".ToArray());
            string ID_string = Lines[i].TrimStart("$UDLR".ToArray());
            int ID = 0;

            if (!int.TryParse(ID_string, out ID))
                Debug.LogError("Could not parse ID for tile #" + i.ToString() + " (" + Lines[i] + ")");

            for(int j=-HalfLineLength; j <= HalfLineLength; j++)
            {
                MobiusGyroposition linePos = new MobiusGyroposition(Hyperbolic.ScalarMult(j, (Complex)Vector2.right * stepLength), 0);
                MakeTileAt(TracePath(path).Apply(linePos), TileObjects[ID]);
            }
        }
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

        for (int i = path.Length - 1; i >= 0; i--)
        {
            mobiusGyroposition.LMoveBy((Complex)(LetterToVector(path[i]) * stepLength));
        }

        return mobiusGyroposition;
    }

    Vector2 LetterToVector(char c)
    {
        switch (c)
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
