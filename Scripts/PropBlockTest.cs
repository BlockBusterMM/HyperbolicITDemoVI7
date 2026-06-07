using UnityEngine;

public class PropBlockTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetFloat("_lig", 0.1f);
        GetComponent<MeshRenderer>().SetPropertyBlock(materialPropertyBlock);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
