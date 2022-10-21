using System.Collections.Generic;
using System;

public class CsvRow {
    private List<string> data;
    private int index;

    public CsvRow(List<string> data) {
        this.data = data;
    }

    public int Count => data.Count;

    // random access apis
    //-------------------------------------------------------------------------
    public string AsString(int column) {
        return data[column];
    }

    public int AsInt(int column) {
        return Convert.ToInt32(data[column]);
    }

    public float AsFloat(int column) {
        return Convert.ToSingle(data[column]);
    }

    public double AsDouble(int column) {
        return Convert.ToDouble(data[column]);
    }

    public bool AsBool(int column) {
        return data[column] == "TRUE";
    }

    public T AsEnum<T>(int column) where T : struct {
        return (T)Enum.Parse(typeof(T), AsString(column), true);
    }


    // next series apis
    //-------------------------------------------------------------------------
    public string NextString() {
        if (data.Count == index) {
            return "";
        }

        return data[index++];
    }

    public int NextInt() {
        return Convert.ToInt32(NextString());
    }

    public float NextFloat() {
        return Convert.ToSingle(NextString());
    }

    public double NextDouble() {
        return Convert.ToDouble(NextString());
    }

    public bool NextBool() {
        string text = data[index++];
        return text == "TRUE" || text == "true";
    }

    public T NextEnum<T>() where T : struct {
        return (T)Enum.Parse(typeof(T), NextString(), true);
    }

    public bool HasNext() {
        return index < data.Count;
    }
}

public class CsvParser {
    private char[] delimiters = new char[] {'\r', '\n'};
    private List<CsvRow> data = new List<CsvRow>();

    public void Parse(string text, char delimiter = ',') {
        Parse(text, delimiter.ToString());
    }

    public void Parse(string text, string delimiter, StringSplitOptions option = StringSplitOptions.None) {
        string[] lines = text.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        char [] columnDelimiters = new char[delimiter.Length];
        for (int i=0; i<delimiter.Length; i++) {
            columnDelimiters[i] = delimiter[i];
        }

        data.Clear();
        foreach (string line in lines) {
            if (string.IsNullOrEmpty(line)) {
                continue;
            }

            if (line[0] == '#') {
                continue;
            }

            string[] token = null; 
            token = line.Split(columnDelimiters, option);
            List<string> parts = new List<string>();
            foreach(string word in token) {
                parts.Add(word);
            }

            data.Add(new CsvRow(parts));
        }
    }

    public int Count {
        get { return data.Count; }
    }

    public CsvRow GetRow(int index) {
        return data[index];
    }
}