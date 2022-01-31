using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositionTester : MonoBehaviour
{
    CompositionSetter setter;

    public string[] skinToneColors;
    public string[] hairStyleNames;
    public string[] hairColors;

    [Range(0, 5)]
    public int sIndex;

    [Range(0, 7)]
    public int hsIndex;

    [Range(0, 12)]
    public int hcIndex;

    // Start is called before the first frame update
    void Start()
    {
        setter = GetComponent<CompositionSetter>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("Current combination:\n skin color: " + skinToneColors[sIndex] 
            + ", hair style: " + hairStyleNames[hsIndex] + ", hair color: " + hairColors[hcIndex] + "\n");
            setter.skinColor = skinToneColors[sIndex];
            setter.hairStyle = hairStyleNames[hsIndex];
            setter.hairColor = hairColors[hcIndex];
            setter.updateChar();
        }
        
    }
}
