/*

This script was automagically generated by SQL for Unity Workbench V.1.0.5 (c) Schema version V.1.0.1 Copyright SteveSmith.Software 2018. All Rights Reserved


You may modify this code as you wish. But any changes will be lost if you re-create the database

Should this script fail to compile and run under Unity it is probable that you have used either a C# or Unity reserved word as a column name.

If a Column Name is invalid rename the column in the SQL for Unity Workbench and Create the database again
HINT: Often changing the case or adding a prefix will be enough

If this fails to resolve the problem please report it at https://stevesmith.software
or email SQL4Unity@stevesmith.software

*/
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class mygamedb_playerpos: SQL4Unity.SQLRow {
public new ushort rowId { get { return Get<ushort>(18); } }
public int PlayerID { get { return Get<int>(20); } set { Set(20, value); } }
public int PosID { get { return Get<int>(22); } set { Set(22, value); } }
/// <summary>
/// Depreciated. This method included for legacy reasons only
/// </summary>
public override void Get(Dictionary<string,object> row) {  }
}
