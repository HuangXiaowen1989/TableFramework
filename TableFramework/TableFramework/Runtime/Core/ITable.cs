using TableFramework;
using System;
using System.Collections;
using System.Collections.Generic;

public interface ITable
{
    void Deserialize(Reader reader);
}


public class TableAttribute : Attribute {

    public string tablePath;
    public TableAttribute(string path)
    {
        tablePath = path;
    }

}


public class TooltipAttribute : Attribute
{

    public string describe;
    public TooltipAttribute(string path)
    { 
        describe = path;
    }

}