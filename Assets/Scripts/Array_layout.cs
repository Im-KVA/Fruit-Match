using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Array_layout 
{
    [System.Serializable]
    public struct rowData
    {
        public bool[] row;
    }
    //Tạo grid có Y = 8, được điều chỉnh bằng Drawer.cs
    public rowData[] rows = new rowData[8]; 
}
